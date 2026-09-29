using Microsoft.EntityFrameworkCore;
using Nethris.Api.Contracts;
using Nethris.Api.Data;
using Nethris.Api.Security;

namespace Nethris.Api.Domain;

public static class HrisPeople
{
    public static IQueryable<Employee> Employees(HrisDbContext db) => db.Employees.Include(e => e.Department)
        .Include(e => e.JobRole).Include(e => e.Manager).Include(e => e.DirectReports);
    public static EmployeeView View(Employee e, Actor actor) => new(e.Id, e.Version, e.EmployeeNumber, e.FirstName,
        e.LastName, e.FullName, e.Email, e.Phone, e.Location, e.HireDate, e.Status,
        actor.Role >= SecurityRole.Hr || actor.EmployeeId == e.Id || (actor.Role == SecurityRole.Manager && e.ManagerId == actor.EmployeeId) ? e.PtoBalanceHours : null,
        e.DepartmentId, e.Department?.Name ?? "", e.JobRoleId, e.JobRole?.Title ?? "", e.JobRole?.Level ?? "",
        e.ManagerId, e.Manager?.FullName, e.DirectReports.Count);
    public static DepartmentView View(Department d) => new(d.Id, d.Version, d.Name, d.CostCenter, d.LeadEmployeeId,
        d.LeadEmployee?.FullName, d.IsActive, d.Employees.Count);
    public static JobRoleView View(JobRole r, Actor actor) => new(r.Id, r.Version, r.Title, r.Level,
        actor.Role >= SecurityRole.Hr ? r.SalaryBandMin : null, actor.Role >= SecurityRole.Hr ? r.SalaryBandMax : null, r.IsActive);

    public static async Task<EmployeeView> SaveEmployee(HrisDbContext db, Actor actor, Guid? id, long version, EmployeeInput input)
    {
        RequireHr(actor);
        var email = HrisRules.Email(input.Email); var number = HrisRules.Text(input.EmployeeNumber, "Employee number", 40);
        var first = HrisRules.Text(input.FirstName, "First name", 80); var last = HrisRules.Text(input.LastName, "Last name", 80);
        HrisRules.EnumValue(input.Status); HrisRules.QuarterUnits(input.PtoBalanceHours, true, 10000);
        if (input.HireDate.Year < 1900 || input.HireDate > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)) throw HrisError.Invalid("Hire date must be from 1900 through one year ahead.");
        Employee employee;
        if (id is null)
        {
            if (await db.Employees.CountAsync() >= HrisRules.MaximumEmployees) throw HrisError.Conflict("This learning database supports at most 2,000 employees.");
            employee = new Employee { Id = Guid.NewGuid(), EmployeeNumber = number, FirstName = first, LastName = last, Email = email };
            db.Employees.Add(employee);
        }
        else
        {
            employee = await db.Employees.Include(e => e.UserAccount).SingleOrDefaultAsync(e => e.Id == id) ?? throw HrisError.Missing();
            HrisRules.Version(employee, version);
            if (employee.UserAccount?.Role == SecurityRole.Admin && actor.Role != SecurityRole.Admin) throw HrisError.Forbidden("Only Admin can edit an Admin employee.");
            if (employee.UserAccount is { Role: SecurityRole.Admin, IsActive: true } admin && input.Status == EmploymentStatus.Terminated)
                await HrisAuth.PreserveAdmin(db, admin.Id);
            if (employee.DepartmentId != input.DepartmentId && await db.Departments.AnyAsync(d => d.LeadEmployeeId == employee.Id))
                throw HrisError.Conflict("Clear the department lead before moving this employee.");
        }
        if (await db.Employees.AnyAsync(e => e.Id != employee.Id && (e.Email == email || e.EmployeeNumber == number)))
            throw HrisError.Conflict("Employee email or number already exists.");
        var department = await db.Departments.FindAsync(input.DepartmentId) ?? throw HrisError.Invalid("Department does not exist.");
        var role = await db.JobRoles.FindAsync(input.JobRoleId) ?? throw HrisError.Invalid("Job role does not exist.");
        if ((!department.IsActive && employee.DepartmentId != department.Id) || (!role.IsActive && employee.JobRoleId != role.Id))
            throw HrisError.Conflict("Choose active reference records for new assignments.");
        var managers = await db.Employees.AsNoTracking().ToDictionaryAsync(e => e.Id, e => e.ManagerId);
        managers[employee.Id] = input.ManagerId;
        // Validate every descendant too: changing one manager can deepen many chains.
        foreach (var pair in managers) HrisRules.ManagerChain(managers, pair.Key, pair.Value);
        if (input.ManagerId is Guid manager && !await db.Employees.AnyAsync(e => e.Id == manager && e.Status != EmploymentStatus.Terminated))
            throw HrisError.Invalid("Choose a non-terminated manager.");
        var pending = await db.PtoRequests.Where(p => p.EmployeeId == employee.Id && (p.Status == PtoStatus.PendingManager || p.Status == PtoStatus.PendingHr)).Select(p => p.Hours).ToListAsync();
        if (input.PtoBalanceHours < pending.Sum()) throw HrisError.Conflict("Balance cannot be lower than reserved pending hours.");
        if (input.Status == EmploymentStatus.Terminated && (pending.Count > 0 || await db.Employees.AnyAsync(e => e.ManagerId == employee.Id) || await db.Departments.AnyAsync(d => d.LeadEmployeeId == employee.Id)))
            throw HrisError.Conflict("Resolve pending PTO, direct reports and department leadership before termination.");
        employee.EmployeeNumber = number; employee.FirstName = first; employee.LastName = last; employee.Email = email;
        employee.Phone = HrisRules.OptionalText(input.Phone, "Phone", 40); employee.Location = HrisRules.OptionalText(input.Location, "Location", 120);
        employee.HireDate = input.HireDate; employee.Status = input.Status; employee.PtoBalanceHours = input.PtoBalanceHours;
        employee.DepartmentId = department.Id; employee.JobRoleId = role.Id; employee.ManagerId = input.ManagerId;
        if (employee.UserAccount is not null) employee.UserAccount.Email = email;
        if (id is not null) db.Entry(employee).Property(e => e.Version).IsModified = true;
        await db.SaveChangesAsync();
        return View(await Employees(db).SingleAsync(e => e.Id == employee.Id), actor);
    }

    public static async Task<DepartmentView> SaveDepartment(HrisDbContext db, Actor actor, Guid? id, long version, DepartmentInput input)
    {
        RequireHr(actor); var name = HrisRules.Text(input.Name, "Department name");
        var item = id is null ? new Department { Id = Guid.NewGuid(), Name = name } : await db.Departments.FindAsync(id) ?? throw HrisError.Missing();
        if (id is not null) HrisRules.Version(item, version);
        if (await db.Departments.AnyAsync(d => d.Id != item.Id && d.Name == name)) throw HrisError.Conflict("Department name already exists.");
        if (input.LeadEmployeeId is Guid lead && !await db.Employees.AnyAsync(e => e.Id == lead && e.DepartmentId == item.Id && e.Status != EmploymentStatus.Terminated))
            throw HrisError.Invalid("A department lead must be a non-terminated employee of that department.");
        if (id is null && await db.Departments.CountAsync() >= 200) throw HrisError.Conflict("Maximum 200 departments.");
        item.Name = name; item.CostCenter = HrisRules.OptionalText(input.CostCenter, "Cost center", 40); item.LeadEmployeeId = input.LeadEmployeeId; item.IsActive = input.IsActive;
        if (id is null) db.Departments.Add(item); else db.Entry(item).Property(d => d.Version).IsModified = true;
        await db.SaveChangesAsync();
        return View(await db.Departments.Include(d => d.Employees).Include(d => d.LeadEmployee).SingleAsync(d => d.Id == item.Id));
    }
    public static async Task<JobRoleView> SaveJobRole(HrisDbContext db, Actor actor, Guid? id, long version, JobRoleInput input)
    {
        RequireHr(actor); var title = HrisRules.Text(input.Title, "Title"); var level = HrisRules.Text(input.Level, "Level", 40);
        HrisRules.Cents(input.SalaryBandMin); HrisRules.Cents(input.SalaryBandMax);
        if (input.SalaryBandMax < input.SalaryBandMin) throw HrisError.Invalid("Maximum salary must be at least the minimum.");
        var item = id is null ? new JobRole { Id = Guid.NewGuid(), Title = title, Level = level } : await db.JobRoles.FindAsync(id) ?? throw HrisError.Missing();
        if (id is not null) HrisRules.Version(item, version);
        if (await db.JobRoles.AnyAsync(r => r.Id != item.Id && r.Title == title && r.Level == level)) throw HrisError.Conflict("Title and level already exist.");
        if (id is null && await db.JobRoles.CountAsync() >= 200) throw HrisError.Conflict("Maximum 200 job roles.");
        item.Title = title; item.Level = level; item.SalaryBandMin = input.SalaryBandMin; item.SalaryBandMax = input.SalaryBandMax; item.IsActive = input.IsActive;
        if (id is null) db.JobRoles.Add(item); else db.Entry(item).Property(r => r.Version).IsModified = true;
        await db.SaveChangesAsync(); return View(item, actor);
    }
    public static async Task Delete(HrisDbContext db, Actor actor, string kind, Guid id, long version)
    {
        RequireHr(actor);
        IVersioned item = kind switch
        {
            "employees" => await db.Employees.FindAsync(id) ?? throw HrisError.Missing(),
            "departments" => await db.Departments.FindAsync(id) ?? throw HrisError.Missing(),
            "job-roles" => await db.JobRoles.FindAsync(id) ?? throw HrisError.Missing(),
            _ => throw HrisError.Missing()
        };
        HrisRules.Version(item, version);
        var referenced = kind switch
        {
            "employees" => await db.UserAccounts.AnyAsync(u => u.EmployeeId == id) || await db.Employees.AnyAsync(e => e.ManagerId == id) ||
                await db.Departments.AnyAsync(d => d.LeadEmployeeId == id) || await db.PtoRequests.AnyAsync(p => p.EmployeeId == id || p.AssignedManagerId == id) || await db.PtoApprovals.AnyAsync(a => a.ApproverEmployeeId == id),
            "departments" => await db.Employees.AnyAsync(e => e.DepartmentId == id),
            _ => await db.Employees.AnyAsync(e => e.JobRoleId == id)
        };
        if (referenced) throw HrisError.Conflict("Referenced records cannot be deleted. Archive the reference record, or resolve employee relationships and history first.");
        db.Remove(item); await db.SaveChangesAsync();
    }
    public static void RequireHr(Actor actor) { if (actor.Role < SecurityRole.Hr) throw HrisError.Forbidden(); }
}
