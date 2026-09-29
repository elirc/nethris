using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Nethris.Api.Data;
using Nethris.Api.Domain;

namespace Nethris.Api.Security;

public sealed record Actor(Guid UserId, Guid EmployeeId, string Email, string Name, SecurityRole Role, long Version);
public sealed record AuthResponse(string Token, Actor User, DateTimeOffset ExpiresAt);
public sealed record SetupInput(string CompanyName, string FirstName, string LastName, string Email, string Password, bool SeedDemo = false);
public sealed record AccountCreate(Guid EmployeeId, SecurityRole Role, string Password);
public sealed record AccountEdit(SecurityRole Role, bool IsActive, string? Password = null);
public sealed record AccountView(Guid Id, Guid EmployeeId, string Email, string EmployeeName, SecurityRole Role, bool IsActive, long Version);

/// <summary>All methods receive the caller's transaction-scoped EF context.</summary>
public sealed class HrisAuth(JwtTokenService tokens)
{
    public async Task<Actor> Require(HrisDbContext db, ClaimsPrincipal principal, SecurityRole minimum = SecurityRole.Employee)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId) ||
            !Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Jti), out var sessionId)) throw Unauthorized();
        var now = DateTimeOffset.UtcNow;
        var session = await db.Sessions.AsNoTracking().Include(s => s.UserAccount)!.ThenInclude(u => u!.Employee)
            .SingleOrDefaultAsync(s => s.Id == sessionId && s.UserAccountId == userId && s.ExpiresAt > now);
        var user = session?.UserAccount;
        if (user is null || !user.IsActive || user.Employee is null || user.Employee.Status == EmploymentStatus.Terminated) throw Unauthorized();
        if (user.Role < minimum) throw HrisError.Forbidden();
        return ToActor(user);
    }

    public async Task<AuthResponse> Setup(HrisDbContext db, SetupInput input)
    {
        if (await db.UserAccounts.AnyAsync()) throw HrisError.Conflict("This workshop is already initialized. Sign in instead.");
        var email = HrisRules.Email(input.Email); var password = HrisRules.Password(input.Password);
        var company = HrisRules.Text(input.CompanyName, "Company name");
        var first = HrisRules.Text(input.FirstName, "First name", 80); var last = HrisRules.Text(input.LastName, "Last name", 80);
        var department = new Department { Id = Guid.NewGuid(), Name = "Administration", CostCenter = "ADM-100" };
        var role = new JobRole { Id = Guid.NewGuid(), Title = "Workshop administrator", Level = "Admin" };
        var employee = new Employee { Id = Guid.NewGuid(), EmployeeNumber = "EMP-0001", FirstName = first, LastName = last,
            Email = email, Department = department, JobRole = role, HireDate = DateOnly.FromDateTime(DateTime.UtcNow), PtoBalanceHours = 160 };
        var user = new UserAccount { Id = Guid.NewGuid(), Employee = employee, Email = email, Role = SecurityRole.Admin, PasswordHash = PasswordService.Hash(password) };
        db.AddRange(department, role, employee, user);
        var settings = await db.WorkshopSettings.SingleAsync(); settings.Name = company;
        await db.SaveChangesAsync();
        department.LeadEmployeeId = employee.Id;
        if (input.SeedDemo) await WorkshopSeed.AddColleagues(db, employee, password);
        db.AuditLogs.Add(Audit(ToActor(user), "Initialized", "Workshop", "1"));
        await db.SaveChangesAsync();
        return await IssueSession(db, user);
    }

    public async Task<AuthResponse> Login(HrisDbContext db, string emailInput, string passwordInput)
    {
        var email = HrisRules.Email(emailInput); var password = HrisRules.Password(passwordInput);
        var user = await db.UserAccounts.Include(u => u.Employee).SingleOrDefaultAsync(u => u.Email == email);
        var valid = PasswordService.Verify(password, user?.PasswordHash ?? PasswordService.DummyHash);
        if (!valid || user is null || !user.IsActive || user.Employee?.Status == EmploymentStatus.Terminated)
            throw new HrisError(401, "credentials", "Email or password is incorrect, or the account is inactive.");
        if (PasswordService.NeedsRehash(user.PasswordHash)) user.PasswordHash = PasswordService.Hash(password);
        return await IssueSession(db, user);
    }
    private async Task<AuthResponse> IssueSession(HrisDbContext db, UserAccount user)
    {
        var now = DateTimeOffset.UtcNow;
        await db.Sessions.Where(s => s.ExpiresAt <= now).ExecuteDeleteAsync();
        var expires = now.AddHours(8); var sessionId = Guid.NewGuid();
        db.Sessions.Add(new AuthSession { Id = sessionId, UserAccountId = user.Id, ExpiresAt = expires });
        await db.SaveChangesAsync();
        return new AuthResponse(tokens.Create(user, sessionId, expires), ToActor(user), expires);
    }
    public async Task Logout(HrisDbContext db, ClaimsPrincipal principal)
    {
        if (Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Jti), out var id))
            await db.Sessions.Where(s => s.Id == id).ExecuteDeleteAsync();
    }
    public async Task<AccountView> CreateAccount(HrisDbContext db, Actor actor, AccountCreate input)
    {
        if (actor.Role != SecurityRole.Admin) throw HrisError.Forbidden();
        HrisRules.EnumValue(input.Role);
        var employee = await db.Employees.SingleOrDefaultAsync(e => e.Id == input.EmployeeId) ?? throw HrisError.Missing();
        if (employee.Status == EmploymentStatus.Terminated) throw HrisError.Conflict("A terminated employee cannot receive an active account.");
        if (await db.UserAccounts.AnyAsync(u => u.EmployeeId == employee.Id)) throw HrisError.Conflict("This employee already has an account.");
        var user = new UserAccount { Id = Guid.NewGuid(), Employee = employee, Email = employee.Email, Role = input.Role, PasswordHash = PasswordService.Hash(input.Password) };
        db.UserAccounts.Add(user); await db.SaveChangesAsync(); return ToView(user);
    }
    public async Task<AccountView> EditAccount(HrisDbContext db, Actor actor, Guid id, long expected, AccountEdit input)
    {
        if (actor.Role != SecurityRole.Admin) throw HrisError.Forbidden();
        HrisRules.EnumValue(input.Role);
        var user = await db.UserAccounts.Include(u => u.Employee).SingleOrDefaultAsync(u => u.Id == id) ?? throw HrisError.Missing();
        HrisRules.Version(user, expected);
        if (user.Role == SecurityRole.Admin && user.IsActive && (input.Role != SecurityRole.Admin || !input.IsActive))
            await PreserveAdmin(db, id);
        if (input.IsActive && user.Employee!.Status == EmploymentStatus.Terminated) throw HrisError.Conflict("Restore the employee's status before enabling the account.");
        user.Role = input.Role; user.IsActive = input.IsActive;
        if (input.Password is not null)
        {
            user.PasswordHash = PasswordService.Hash(input.Password);
            await db.Sessions.Where(s => s.UserAccountId == id).ExecuteDeleteAsync();
        }
        db.Entry(user).Property(u => u.Version).IsModified = true;
        await db.SaveChangesAsync(); return ToView(user);
    }
    public static async Task PreserveAdmin(HrisDbContext db, Guid changingUserId)
    {
        if (!await db.UserAccounts.AnyAsync(u => u.Id != changingUserId && u.Role == SecurityRole.Admin && u.IsActive && u.Employee!.Status != EmploymentStatus.Terminated))
            throw HrisError.Conflict("Keep at least one active Admin account.");
    }
    public static Actor ToActor(UserAccount user) => new(user.Id, user.EmployeeId, user.Email, user.Employee?.FullName ?? user.Email, user.Role, user.Version);
    public static AccountView ToView(UserAccount user) => new(user.Id, user.EmployeeId, user.Email, user.Employee?.FullName ?? user.Email, user.Role, user.IsActive, user.Version);
    public static AuditLog Audit(Actor actor, string action, string entity, string id, string? details = null) => new()
    { Id = Guid.NewGuid(), ActorEmail = actor.Email, Action = action, EntityType = entity, EntityId = id, Details = details };
    private static HrisError Unauthorized() => new(401, "session", "Session expired or revoked. Sign in again.");
}
