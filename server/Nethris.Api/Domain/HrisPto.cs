using Microsoft.EntityFrameworkCore;
using Nethris.Api.Contracts;
using Nethris.Api.Data;
using Nethris.Api.Security;

namespace Nethris.Api.Domain;

public static class HrisPto
{
    public static IQueryable<PtoRequest> Requests(HrisDbContext db) => db.PtoRequests.Include(p => p.Employee)!.ThenInclude(e => e!.Department)
        .Include(p => p.Approvals).ThenInclude(a => a.ApproverEmployee);
    public static IQueryable<PtoRequest> Visible(HrisDbContext db, Actor actor) => Requests(db).Where(p => actor.Role >= SecurityRole.Hr || p.EmployeeId == actor.EmployeeId ||
        (actor.Role == SecurityRole.Manager && p.AssignedManagerId == actor.EmployeeId && p.Employee!.ManagerId == actor.EmployeeId));
    public static PtoView View(PtoRequest p) => new(p.Id, p.Version, p.EmployeeId, p.Employee?.FullName ?? "", p.Employee?.Department?.Name ?? "",
        p.StartDate, p.EndDate, p.Hours, p.Reason, p.Status, p.AssignedManagerId, p.CreatedAt,
        p.Approvals.OrderBy(a => a.CreatedAt).ThenBy(a => a.Id).Select(a => new ApprovalView(a.Id, a.ApproverEmployeeId, a.ApproverEmployee?.FullName ?? "", a.Stage, a.Decision, a.Comment, a.CreatedAt)).ToList());
    public static bool Pending(PtoStatus status) => status is PtoStatus.PendingManager or PtoStatus.PendingHr;
    public static async Task<PtoView> Save(HrisDbContext db, Actor actor, Guid? id, long version, PtoInput input)
    {
        HrisRules.PtoDates(input.StartDate, input.EndDate, input.Hours, DateOnly.FromDateTime(DateTime.UtcNow));
        var reason = HrisRules.Text(input.Reason, "Reason", 500);
        var employee = await db.Employees.SingleAsync(e => e.Id == actor.EmployeeId);
        PtoRequest p;
        if (id is null)
        {
            if (await db.PtoRequests.CountAsync() >= 10000) throw HrisError.Conflict("Maximum 10,000 PTO requests in this learning database.");
            p = new PtoRequest { Id = Guid.NewGuid(), EmployeeId = actor.EmployeeId, Reason = reason };
        }
        else
        {
            p = await Requests(db).SingleOrDefaultAsync(p => p.Id == id && p.EmployeeId == actor.EmployeeId) ?? throw HrisError.Missing();
            HrisRules.Version(p, version);
            if (!Pending(p.Status) || p.Approvals.Count > 0) throw HrisError.Conflict("Only pending requests without decisions can be edited. Cancel and submit a new request after review starts.");
        }
        var open = await db.PtoRequests.Where(other => other.Id != p.Id && other.EmployeeId == actor.EmployeeId &&
            (other.Status == PtoStatus.PendingManager || other.Status == PtoStatus.PendingHr || other.Status == PtoStatus.Approved)).ToListAsync();
        if (open.Any(other => other.StartDate <= input.EndDate && other.EndDate >= input.StartDate)) throw HrisError.Conflict("These dates overlap an active PTO request.");
        if (open.Where(other => Pending(other.Status)).Sum(other => other.Hours) + input.Hours > employee.PtoBalanceHours)
            throw HrisError.Conflict("Insufficient balance after reserving other pending requests.");
        var manager = employee.ManagerId;
        if (manager is not null && !await db.UserAccounts.AnyAsync(u => u.EmployeeId == manager && u.IsActive && u.Role >= SecurityRole.Manager && u.Employee!.Status != EmploymentStatus.Terminated)) manager = null;
        p.AssignedManagerId = manager; p.Status = manager is null ? PtoStatus.PendingHr : PtoStatus.PendingManager;
        p.StartDate = input.StartDate; p.EndDate = input.EndDate; p.Hours = input.Hours; p.Reason = reason;
        if (id is null) db.PtoRequests.Add(p); else db.Entry(p).Property(p => p.Version).IsModified = true;
        await db.SaveChangesAsync(); return View(await Requests(db).SingleAsync(r => r.Id == p.Id));
    }
    public static async Task<PtoView> Decide(HrisDbContext db, Actor actor, Guid id, long version, DecisionInput input)
    {
        HrisRules.EnumValue(input.Decision);
        var p = await Requests(db).SingleOrDefaultAsync(p => p.Id == id) ?? throw HrisError.Missing();
        if (actor.EmployeeId == p.EmployeeId || actor.Role < SecurityRole.Manager) throw HrisError.Forbidden("You cannot approve your own request.");
        HrisRules.Version(p, version);
        if (!Pending(p.Status)) throw HrisError.Conflict("This PTO request is already closed.");
        var stage = p.Status == PtoStatus.PendingManager ? "Manager" : "Hr";
        if (stage == "Manager" && (p.AssignedManagerId != actor.EmployeeId || p.Employee!.ManagerId != actor.EmployeeId))
            throw HrisError.Forbidden("The assigned current direct manager must make the first decision. The requester can cancel and resubmit after a manager change.");
        if (stage == "Hr" && (actor.Role < SecurityRole.Hr || p.Approvals.Any(a => a.ApproverEmployeeId == actor.EmployeeId)))
            throw HrisError.Forbidden("A different HR/Admin reviewer must make the final decision.");
        if (p.Employee!.Status == EmploymentStatus.Terminated) throw HrisError.Conflict("Terminated employees cannot receive PTO decisions.");
        if (input.Decision == PtoDecision.Rejected) p.Status = PtoStatus.Rejected;
        else if (stage == "Manager") p.Status = PtoStatus.PendingHr;
        else
        {
            if (p.Employee.PtoBalanceHours < p.Hours) throw HrisError.Conflict("PTO balance is insufficient.");
            p.Employee.PtoBalanceHours -= p.Hours; p.Status = PtoStatus.Approved;
        }
        db.PtoApprovals.Add(new PtoApproval { Id = Guid.NewGuid(), PtoRequestId = id, ApproverEmployeeId = actor.EmployeeId, Stage = stage,
            Decision = input.Decision, Comment = HrisRules.OptionalText(input.Comment, "Comment", 500) });
        await db.SaveChangesAsync(); return View(await Requests(db).SingleAsync(r => r.Id == id));
    }
    public static async Task<object> Remove(HrisDbContext db, Actor actor, Guid id, long version, bool cancel)
    {
        var p = await Requests(db).SingleOrDefaultAsync(p => p.Id == id && p.EmployeeId == actor.EmployeeId) ?? throw HrisError.Missing();
        HrisRules.Version(p, version);
        if (!Pending(p.Status)) throw HrisError.Conflict("Only pending PTO can be cancelled or deleted. Approved history and its balance deduction are immutable in this workshop.");
        if (!cancel && p.Approvals.Count > 0) throw HrisError.Conflict("A request with decisions must be cancelled, not deleted.");
        if (cancel) p.Status = PtoStatus.Cancelled; else db.PtoRequests.Remove(p);
        await db.SaveChangesAsync(); return cancel ? View(p) : new { deleted = id };
    }
}
