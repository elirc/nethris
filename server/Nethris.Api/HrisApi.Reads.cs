using Microsoft.EntityFrameworkCore;
using Nethris.Api.Contracts;
using Nethris.Api.Data;
using Nethris.Api.Domain;
using Nethris.Api.Security;

namespace Nethris.Api;

public sealed partial class HrisApi
{
    /// <summary>Every GET: the actor's own view, the org chart, or one resource kind as a single record or a page.</summary>
    private static async Task Read(HttpContext http, HrisDbContext db, Actor actor, string[] parts)
    {
        var path = string.Join('/', parts);
        if (path == "me")
        {
            await HrisRequest.Send(http, actor);
            return;
        }

        if (path == "org-chart")
        {
            await OrgChart(http, db);
            return;
        }

        var page = new PageRequest(
            HrisRequest.Number(http, "offset", 0, 0, 100000),
            HrisRequest.Number(http, "limit", 25, 1, 100),
            HrisRules.OptionalText(http.Request.Query["search"], "Search", 120)?.ToLowerInvariant());
        Guid? id = parts.Length == 2 && Guid.TryParse(parts[1], out var parsed) ? parsed : null;
        if (parts.Length > 1 && id is null || parts.Length > 2)
        {
            throw HrisError.Missing();
        }

        switch (parts[0])
        {
            case "employees":
                await Employees(http, db, actor, page, id);
                break;
            case "departments":
                await Departments(http, db, page, id);
                break;
            case "job-roles":
                await JobRoles(http, db, actor, page, id);
                break;
            case "pto":
                await Pto(http, db, actor, page, id);
                break;
            case "accounts":
                await Accounts(http, db, page, id);
                break;
            case "audit-logs" when id is null:
                await AuditLogs(http, db, page);
                break;
            default:
                throw HrisError.Missing();
        }
    }

    private readonly record struct PageRequest(int Offset, int Limit, string? Search);

    private static async Task OrgChart(HttpContext http, HrisDbContext db)
    {
        var all = await HrisPeople.Employees(db).AsNoTracking()
            .OrderBy(e => e.LastName).ThenBy(e => e.Id)
            .Take(HrisRules.MaximumEmployees + 1)
            .ToListAsync();
        if (all.Count > HrisRules.MaximumEmployees)
        {
            throw HrisError.Conflict("Org chart exceeds the workshop limit.");
        }

        var managers = all.ToDictionary(e => e.Id, e => e.ManagerId);
        foreach (var e in all)
        {
            HrisRules.ManagerChain(managers, e.Id, e.ManagerId);
        }

        var children = all.ToLookup(e => e.ManagerId);
        OrgNodeResponse Node(Employee e) =>
            new(e.Id, e.FullName, e.JobRole!.Title, e.Department!.Name, e.Location, children[e.Id].Select(Node).ToList());
        await HrisRequest.Send(http, children[null].Select(Node).ToList());
    }

    private static async Task Employees(HttpContext http, HrisDbContext db, Actor actor, PageRequest page, Guid? id)
    {
        var query = HrisPeople.Employees(db).AsNoTracking();
        if (page.Search is string search)
        {
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) || e.LastName.ToLower().Contains(search) ||
                e.Email.Contains(search) || e.EmployeeNumber.ToLower().Contains(search));
        }

        if (http.Request.Query.TryGetValue("departmentId", out var dept))
        {
            if (!Guid.TryParse(dept, out var department))
            {
                throw HrisError.Invalid("Invalid department filter.");
            }

            query = query.Where(e => e.DepartmentId == department);
        }

        if (id is Guid employeeId)
        {
            var e = await query.SingleOrDefaultAsync(e => e.Id == employeeId) ?? throw HrisError.Missing();
            await HrisRequest.Send(http, HrisPeople.View(e, actor), tag: ETag(e.Id, e.Version));
            return;
        }

        var rows = await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.Id)
            .Skip(page.Offset).Take(page.Limit).ToListAsync();
        var items = rows.Select(e => HrisPeople.View(e, actor)).ToList();
        await HrisRequest.Send(http, new Page<EmployeeView>(items, await query.CountAsync(), page.Offset, page.Limit));
    }

    private static async Task Departments(HttpContext http, HrisDbContext db, PageRequest page, Guid? id)
    {
        var query = db.Departments.AsNoTracking().Include(d => d.LeadEmployee).Include(d => d.Employees).AsQueryable();
        if (page.Search is string search)
        {
            query = query.Where(d => d.Name.ToLower().Contains(search));
        }

        if (id is Guid departmentId)
        {
            var d = await query.SingleOrDefaultAsync(d => d.Id == departmentId) ?? throw HrisError.Missing();
            await HrisRequest.Send(http, HrisPeople.View(d), tag: ETag(d.Id, d.Version));
            return;
        }

        var rows = await query.OrderBy(d => d.Name).ThenBy(d => d.Id).Skip(page.Offset).Take(page.Limit).ToListAsync();
        var items = rows.Select(HrisPeople.View).ToList();
        await HrisRequest.Send(http, new Page<DepartmentView>(items, await query.CountAsync(), page.Offset, page.Limit));
    }

    private static async Task JobRoles(HttpContext http, HrisDbContext db, Actor actor, PageRequest page, Guid? id)
    {
        var query = db.JobRoles.AsNoTracking().AsQueryable();
        if (page.Search is string search)
        {
            query = query.Where(r => r.Title.ToLower().Contains(search) || r.Level.ToLower().Contains(search));
        }

        if (id is Guid roleId)
        {
            var r = await query.SingleOrDefaultAsync(r => r.Id == roleId) ?? throw HrisError.Missing();
            await HrisRequest.Send(http, HrisPeople.View(r, actor), tag: ETag(r.Id, r.Version));
            return;
        }

        var rows = await query.OrderBy(r => r.Title).ThenBy(r => r.Level).ThenBy(r => r.Id).Skip(page.Offset).Take(page.Limit).ToListAsync();
        var items = rows.Select(r => HrisPeople.View(r, actor)).ToList();
        await HrisRequest.Send(http, new Page<JobRoleView>(items, await query.CountAsync(), page.Offset, page.Limit));
    }

    private static async Task Pto(HttpContext http, HrisDbContext db, Actor actor, PageRequest page, Guid? id)
    {
        var query = HrisPto.Visible(db, actor).AsNoTracking();
        if (page.Search is string search)
        {
            query = query.Where(p =>
                p.Reason.ToLower().Contains(search) || p.Employee!.FirstName.ToLower().Contains(search) ||
                p.Employee.LastName.ToLower().Contains(search));
        }

        if (id is Guid requestId)
        {
            var p = await query.SingleOrDefaultAsync(p => p.Id == requestId) ?? throw HrisError.Missing();
            await HrisRequest.Send(http, HrisPto.View(p), tag: ETag(p.Id, p.Version));
            return;
        }

        var rows = await query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id).Skip(page.Offset).Take(page.Limit).ToListAsync();
        var items = rows.Select(HrisPto.View).ToList();
        await HrisRequest.Send(http, new Page<PtoView>(items, await query.CountAsync(), page.Offset, page.Limit));
    }

    private static async Task Accounts(HttpContext http, HrisDbContext db, PageRequest page, Guid? id)
    {
        var query = db.UserAccounts.AsNoTracking().Include(u => u.Employee).AsQueryable();
        if (page.Search is string search)
        {
            query = query.Where(u => u.Email.Contains(search));
        }

        if (id is Guid accountId)
        {
            var a = await query.SingleOrDefaultAsync(u => u.Id == accountId) ?? throw HrisError.Missing();
            await HrisRequest.Send(http, HrisAuth.ToView(a), tag: ETag(a.Id, a.Version));
            return;
        }

        var rows = await query.OrderBy(u => u.Email).ThenBy(u => u.Id).Skip(page.Offset).Take(page.Limit).ToListAsync();
        var items = rows.Select(HrisAuth.ToView).ToList();
        await HrisRequest.Send(http, new Page<AccountView>(items, await query.CountAsync(), page.Offset, page.Limit));
    }

    private static async Task AuditLogs(HttpContext http, HrisDbContext db, PageRequest page)
    {
        var logs = db.AuditLogs.AsNoTracking();
        var items = await logs.OrderByDescending(a => a.CreatedAt).ThenBy(a => a.Id)
            .Skip(page.Offset).Take(page.Limit)
            .Select(a => new AuditLogResponse(a.Id, a.CreatedAt, a.ActorEmail, a.Action, a.EntityType, a.EntityId, a.Details))
            .ToListAsync();
        await HrisRequest.Send(http, new Page<AuditLogResponse>(items, await logs.CountAsync(), page.Offset, page.Limit));
    }
}
