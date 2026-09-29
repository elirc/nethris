using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nethris.Api.Contracts;
using Nethris.Api.Data;
using Nethris.Api.Domain;
using Nethris.Api.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HrisDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PeopleOps", policy => policy.RequireRole(nameof(SecurityRole.Hr), nameof(SecurityRole.Admin)));
    options.AddPolicy("Approvers", policy => policy.RequireRole(nameof(SecurityRole.Manager), nameof(SecurityRole.Hr), nameof(SecurityRole.Admin)));
});

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HrisDbContext>();
    await SeedData.EnsureSeededAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "Nethris.Api" }));

app.MapPost("/api/auth/login", async (LoginRequest request, HrisDbContext db, JwtTokenService tokens) =>
{
    var user = await db.UserAccounts
        .Include(u => u.Employee)
        .SingleOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive);

    if (user is null || !PasswordService.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var response = new LoginResponse(tokens.Create(user), ToCurrentUser(user));
    return Results.Ok(response);
});

app.MapGet("/api/me", async (ClaimsPrincipal principal, HrisDbContext db) =>
{
    var userId = principal.UserId();
    var user = await db.UserAccounts.Include(u => u.Employee).SingleOrDefaultAsync(u => u.Id == userId);
    return user is null ? Results.NotFound() : Results.Ok(ToCurrentUser(user));
}).RequireAuthorization();

app.MapGet("/api/departments", async (HrisDbContext db) =>
{
    var departments = await db.Departments
        .Include(d => d.LeadEmployee)
        .Include(d => d.Employees)
        .OrderBy(d => d.Name)
        .Select(d => new DepartmentResponse(
            d.Id,
            d.Name,
            d.CostCenter,
            d.LeadEmployee == null ? null : d.LeadEmployee.FullName,
            d.Employees.Count))
        .ToListAsync();
    return Results.Ok(departments);
}).RequireAuthorization();

app.MapGet("/api/job-roles", async (HrisDbContext db) =>
{
    var roles = await db.JobRoles
        .OrderBy(r => r.Title)
        .Select(r => new JobRoleResponse(r.Id, r.Title, r.Level, r.SalaryBandMin, r.SalaryBandMax))
        .ToListAsync();
    return Results.Ok(roles);
}).RequireAuthorization();

app.MapGet("/api/employees", async (string? search, Guid? departmentId, HrisDbContext db) =>
{
    var query = db.Employees
        .Include(e => e.Department)
        .Include(e => e.JobRole)
        .Include(e => e.Manager)
        .Include(e => e.DirectReports)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(e =>
            e.FirstName.ToLower().Contains(term) ||
            e.LastName.ToLower().Contains(term) ||
            e.Email.ToLower().Contains(term) ||
            e.EmployeeNumber.ToLower().Contains(term));
    }

    if (departmentId.HasValue)
    {
        query = query.Where(e => e.DepartmentId == departmentId);
    }

    var employees = await query
        .OrderBy(e => e.LastName)
        .ThenBy(e => e.FirstName)
        .ToListAsync();

    return Results.Ok(employees.Select(ToEmployeeResponse));
}).RequireAuthorization();

app.MapGet("/api/employees/{id:guid}", async (Guid id, HrisDbContext db) =>
{
    var employee = await EmployeeIncludes(db.Employees).SingleOrDefaultAsync(e => e.Id == id);
    return employee is null ? Results.NotFound() : Results.Ok(ToEmployeeResponse(employee));
}).RequireAuthorization();

app.MapPost("/api/employees", async (
    UpsertEmployeeRequest request,
    ClaimsPrincipal principal,
    HrisDbContext db) =>
{
    if (await db.Employees.AnyAsync(e => e.Email == request.Email || e.EmployeeNumber == request.EmployeeNumber))
    {
        return Results.Conflict(new { message = "Employee email or employee number already exists." });
    }

    var employee = new Employee
    {
        Id = Guid.NewGuid(),
        EmployeeNumber = request.EmployeeNumber,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email.ToLowerInvariant(),
        Phone = request.Phone,
        Location = request.Location,
        HireDate = request.HireDate,
        Status = request.Status,
        PtoBalanceHours = request.PtoBalanceHours,
        DepartmentId = request.DepartmentId,
        JobRoleId = request.JobRoleId,
        ManagerId = request.ManagerId
    };

    db.Employees.Add(employee);

    if (request.AccountRole.HasValue)
    {
        db.UserAccounts.Add(new UserAccount
        {
            Id = Guid.NewGuid(),
            Employee = employee,
            Email = employee.Email,
            Role = request.AccountRole.Value,
            PasswordHash = PasswordService.Hash("Password123!")
        });
    }

    db.AuditLogs.Add(Audit(principal, "Created", nameof(Employee), employee.Id.ToString(), employee.FullName));
    await db.SaveChangesAsync();

    var saved = await EmployeeIncludes(db.Employees).SingleAsync(e => e.Id == employee.Id);
    return Results.Created($"/api/employees/{employee.Id}", ToEmployeeResponse(saved));
}).RequireAuthorization("PeopleOps");

app.MapPut("/api/employees/{id:guid}", async (
    Guid id,
    UpsertEmployeeRequest request,
    ClaimsPrincipal principal,
    HrisDbContext db) =>
{
    var employee = await db.Employees.Include(e => e.UserAccount).SingleOrDefaultAsync(e => e.Id == id);
    if (employee is null)
    {
        return Results.NotFound();
    }

    employee.EmployeeNumber = request.EmployeeNumber;
    employee.FirstName = request.FirstName;
    employee.LastName = request.LastName;
    employee.Email = request.Email.ToLowerInvariant();
    employee.Phone = request.Phone;
    employee.Location = request.Location;
    employee.HireDate = request.HireDate;
    employee.Status = request.Status;
    employee.PtoBalanceHours = request.PtoBalanceHours;
    employee.DepartmentId = request.DepartmentId;
    employee.JobRoleId = request.JobRoleId;
    employee.ManagerId = request.ManagerId == id ? null : request.ManagerId;

    if (request.AccountRole.HasValue)
    {
        if (employee.UserAccount is null)
        {
            db.UserAccounts.Add(new UserAccount
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                Email = employee.Email,
                Role = request.AccountRole.Value,
                PasswordHash = PasswordService.Hash("Password123!")
            });
        }
        else
        {
            employee.UserAccount.Email = employee.Email;
            employee.UserAccount.Role = request.AccountRole.Value;
        }
    }

    db.AuditLogs.Add(Audit(principal, "Updated", nameof(Employee), employee.Id.ToString(), employee.FullName));
    await db.SaveChangesAsync();

    var saved = await EmployeeIncludes(db.Employees).SingleAsync(e => e.Id == employee.Id);
    return Results.Ok(ToEmployeeResponse(saved));
}).RequireAuthorization("PeopleOps");

app.MapGet("/api/org-chart", async (HrisDbContext db) =>
{
    var employees = await db.Employees
        .Include(e => e.Department)
        .Include(e => e.JobRole)
        .AsNoTracking()
        .ToListAsync();

    var roots = employees
        .Where(e => e.ManagerId is null)
        .OrderBy(e => e.LastName)
        .Select(e => ToOrgNode(e, employees))
        .ToList();

    return Results.Ok(roots);
}).RequireAuthorization();

app.MapGet("/api/pto", async (ClaimsPrincipal principal, HrisDbContext db) =>
{
    var role = principal.Role();
    var employeeId = principal.EmployeeId();
    var query = PtoIncludes(db.PtoRequests).AsQueryable();

    if (role == SecurityRole.Employee)
    {
        query = query.Where(p => p.EmployeeId == employeeId);
    }
    else if (role == SecurityRole.Manager)
    {
        var reportIds = await db.Employees.Where(e => e.ManagerId == employeeId).Select(e => e.Id).ToListAsync();
        query = query.Where(p => p.EmployeeId == employeeId || reportIds.Contains(p.EmployeeId));
    }

    var requests = await query
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();

    return Results.Ok(requests.Select(ToPtoResponse));
}).RequireAuthorization();

app.MapPost("/api/pto", async (
    CreatePtoRequest request,
    ClaimsPrincipal principal,
    HrisDbContext db) =>
{
    if (request.EndDate < request.StartDate || request.Hours <= 0)
    {
        return Results.BadRequest(new { message = "PTO dates and hours must be valid." });
    }

    var employeeId = principal.EmployeeId();
    var pto = new PtoRequest
    {
        Id = Guid.NewGuid(),
        EmployeeId = employeeId,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Hours = request.Hours,
        Reason = request.Reason,
        Status = PtoStatus.PendingManager
    };

    db.PtoRequests.Add(pto);
    db.AuditLogs.Add(Audit(principal, "Requested", nameof(PtoRequest), pto.Id.ToString(), $"{request.Hours} PTO hours"));
    await db.SaveChangesAsync();

    var saved = await PtoIncludes(db.PtoRequests).SingleAsync(p => p.Id == pto.Id);
    return Results.Created($"/api/pto/{pto.Id}", ToPtoResponse(saved));
}).RequireAuthorization();

app.MapPost("/api/pto/{id:guid}/decision", async (
    Guid id,
    DecidePtoRequest request,
    ClaimsPrincipal principal,
    HrisDbContext db) =>
{
    var pto = await PtoIncludes(db.PtoRequests).SingleOrDefaultAsync(p => p.Id == id);
    if (pto is null)
    {
        return Results.NotFound();
    }

    if (pto.Status is PtoStatus.Approved or PtoStatus.Rejected or PtoStatus.Cancelled)
    {
        return Results.Conflict(new { message = "This PTO request is already closed." });
    }

    var approverId = principal.EmployeeId();
    var role = principal.Role();
    var isManagerForEmployee = pto.Employee?.ManagerId == approverId;
    if (role == SecurityRole.Manager && !isManagerForEmployee)
    {
        return Results.Forbid();
    }

    if (role == SecurityRole.Manager && pto.Status == PtoStatus.PendingHr)
    {
        return Results.Forbid();
    }

    pto.Approvals.Add(new PtoApproval
    {
        Id = Guid.NewGuid(),
        ApproverEmployeeId = approverId,
        Decision = request.Decision,
        Comment = request.Comment
    });

    if (request.Decision == PtoDecision.Rejected)
    {
        pto.Status = PtoStatus.Rejected;
    }
    else if (role is SecurityRole.Hr or SecurityRole.Admin)
    {
        pto.Status = PtoStatus.Approved;
        if (pto.Employee is not null)
        {
            pto.Employee.PtoBalanceHours -= pto.Hours;
        }
    }
    else
    {
        pto.Status = PtoStatus.PendingHr;
    }

    db.AuditLogs.Add(Audit(principal, request.Decision.ToString(), nameof(PtoRequest), pto.Id.ToString(), pto.Status.ToString()));
    await db.SaveChangesAsync();

    var saved = await PtoIncludes(db.PtoRequests).SingleAsync(p => p.Id == id);
    return Results.Ok(ToPtoResponse(saved));
}).RequireAuthorization("Approvers");

app.MapGet("/api/audit-logs", async (HrisDbContext db) =>
{
    var logs = await db.AuditLogs
        .OrderByDescending(a => a.CreatedAt)
        .Take(200)
        .Select(a => new AuditLogResponse(a.Id, a.CreatedAt, a.ActorEmail, a.Action, a.EntityType, a.EntityId, a.Details))
        .ToListAsync();
    return Results.Ok(logs);
}).RequireAuthorization("PeopleOps");

app.Run();

static IQueryable<Employee> EmployeeIncludes(DbSet<Employee> employees) =>
    employees
        .Include(e => e.Department)
        .Include(e => e.JobRole)
        .Include(e => e.Manager)
        .Include(e => e.DirectReports);

static IQueryable<PtoRequest> PtoIncludes(DbSet<PtoRequest> requests) =>
    requests
        .Include(p => p.Employee)!.ThenInclude(e => e!.Department)
        .Include(p => p.Employee)!.ThenInclude(e => e!.Manager)
        .Include(p => p.Approvals).ThenInclude(a => a.ApproverEmployee);

static EmployeeResponse ToEmployeeResponse(Employee e) =>
    new(
        e.Id,
        e.EmployeeNumber,
        e.FirstName,
        e.LastName,
        e.FullName,
        e.Email,
        e.Phone,
        e.Location,
        e.HireDate,
        e.Status,
        e.PtoBalanceHours,
        e.DepartmentId,
        e.Department?.Name ?? "Unassigned",
        e.JobRoleId,
        e.JobRole?.Title ?? "Unassigned",
        e.JobRole?.Level ?? "Unknown",
        e.ManagerId,
        e.Manager?.FullName,
        e.DirectReports.Count);

static OrgNodeResponse ToOrgNode(Employee employee, IReadOnlyCollection<Employee> employees)
{
    var reports = employees
        .Where(e => e.ManagerId == employee.Id)
        .OrderBy(e => e.LastName)
        .Select(e => ToOrgNode(e, employees))
        .ToList();

    return new OrgNodeResponse(
        employee.Id,
        employee.FullName,
        employee.JobRole?.Title ?? "Unassigned",
        employee.Department?.Name ?? "Unassigned",
        employee.Location,
        reports);
}

static PtoResponse ToPtoResponse(PtoRequest p) =>
    new(
        p.Id,
        p.EmployeeId,
        p.Employee?.FullName ?? "Unknown",
        p.Employee?.Department?.Name ?? "Unknown",
        p.StartDate,
        p.EndDate,
        p.Hours,
        p.Reason,
        p.Status,
        p.CreatedAt,
        p.Approvals
            .OrderBy(a => a.CreatedAt)
            .Select(a => new PtoApprovalResponse(
                a.Id,
                a.ApproverEmployee?.FullName ?? "Unknown",
                a.Decision,
                a.Comment,
                a.CreatedAt))
            .ToList());

static CurrentUserResponse ToCurrentUser(UserAccount user) =>
    new(
        user.Id,
        user.EmployeeId,
        user.Email,
        user.Employee?.FullName ?? user.Email,
        user.Role);

static AuditLog Audit(ClaimsPrincipal principal, string action, string entityType, string entityId, string? details = null) =>
    new()
    {
        Id = Guid.NewGuid(),
        ActorEmail = principal.FindFirstValue(ClaimTypes.Email) ?? "unknown@nethris.local",
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        Details = details
    };

public static class ClaimsPrincipalExtensions
{
    public static Guid UserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static Guid EmployeeId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue("employeeId")!);

    public static SecurityRole Role(this ClaimsPrincipal principal) =>
        Enum.Parse<SecurityRole>(principal.FindFirstValue(ClaimTypes.Role)!);
}
