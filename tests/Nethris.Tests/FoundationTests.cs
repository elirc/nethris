using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Nethris.Api.Data;
using Nethris.Api.Domain;

namespace Nethris.Tests;

public sealed class FoundationTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "nethris-foundation-" + Guid.NewGuid().ToString("N"));
    private readonly HrisDatabase database;
    public FoundationTests() => database = new HrisDatabase(Path.Combine(directory, "test.sqlite"));
    public void Dispose() => Directory.Delete(directory, recursive: true);

    private async Task<(Guid Employee, Guid Manager, Guid Pto)> Seed()
    {
        await using var db = database.Create();
        var department = new Department { Id = Guid.NewGuid(), Name = "Engineering" };
        var role = new JobRole { Id = Guid.NewGuid(), Title = "Engineer", Level = "IC2", SalaryBandMin = 70000.25m, SalaryBandMax = 100000.50m };
        Employee Person(string name) => new() { Id = Guid.NewGuid(), EmployeeNumber = name, FirstName = name, LastName = "Test",
            Email = name.ToLowerInvariant() + "@example.test", Department = department, JobRole = role, PtoBalanceHours = 40.25m,
            HireDate = new DateOnly(2026, 1, 1) };
        var manager = Person("Manager"); var employee = Person("Employee"); employee.Manager = manager;
        var pto = new PtoRequest { Id = Guid.NewGuid(), Employee = employee, AssignedManager = manager,
            StartDate = new DateOnly(2026, 10, 1), EndDate = new DateOnly(2026, 10, 1), Hours = 8, Reason = "Fixture" };
        db.AddRange(department, role, manager, employee, pto); await db.SaveChangesAsync();
        return (employee.Id, manager.Id, pto.Id);
    }

    [Fact] public async Task TimestampOrderingExecutesInSqlite()
    {
        await using var db = database.Create();
        foreach (var seconds in new[] { 20, 10, 30 })
            db.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), ActorEmail = "fixture@example.test", Action = seconds.ToString(), EntityType = "Test", EntityId = "fixture",
                CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000 + seconds * 1000) });
        await db.SaveChangesAsync();
        var ordered = await db.AuditLogs.OrderBy(a => a.CreatedAt).Select(a => a.Action).ToArrayAsync();
        Assert.Equal(["10", "20", "30"], ordered);
        var ids = await Seed();
        Assert.Single(await db.PtoRequests.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }
    [Fact] public async Task AttachedApprovalWithAssignedGuidIsInserted()
    {
        var ids = await Seed();
        await using var db = database.Create();
        var pto = await db.PtoRequests.Include(p => p.Approvals).SingleAsync(p => p.Id == ids.Pto);
        pto.Approvals.Add(new PtoApproval { Id = Guid.NewGuid(), ApproverEmployeeId = ids.Manager, Decision = PtoDecision.Approved, Stage = "Manager" });
        pto.Status = PtoStatus.PendingHr;
        await db.SaveChangesAsync();
        Assert.Single(await db.PtoApprovals.ToListAsync());
        Assert.Equal(2, pto.Version);
    }
    [Fact] public async Task CompetingTrackedEditsPreserveTheWinner()
    {
        var ids = await Seed(); await using var first = database.Create(); await using var second = database.Create();
        var winner = await first.Employees.SingleAsync(e => e.Id == ids.Employee);
        var loser = await second.Employees.SingleAsync(e => e.Id == ids.Employee);
        winner.FirstName = "Winner"; await first.SaveChangesAsync();
        loser.FirstName = "Loser";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await using var read = database.Create(); var stored = await read.Employees.SingleAsync(e => e.Id == ids.Employee);
        Assert.Equal("Winner", stored.FirstName); Assert.Equal(2, stored.Version);
    }
    [Fact] public async Task RestartKeepsPrivateKeyAndExactScaledValues()
    {
        var ids = await Seed(); var reopened = new HrisDatabase(database.Path);
        Assert.Equal(database.SigningKey, reopened.SigningKey);
        await using var db = reopened.Create();
        Assert.Equal(40.25m, (await db.Employees.SingleAsync(e => e.Id == ids.Employee)).PtoBalanceHours);
        var role = await db.JobRoles.SingleAsync(); Assert.Equal(70000.25m, role.SalaryBandMin); Assert.Equal(100000.50m, role.SalaryBandMax);
    }
    [Fact] public void UnknownFileIsRefusedWithoutAdoption()
    {
        var path = Path.Combine(directory, "unknown.sqlite"); File.WriteAllBytes(path, []);
        Assert.Throws<InvalidDataException>(() => new HrisDatabase(path)); Assert.Equal(0, new FileInfo(path).Length);
    }
    [Fact] public async Task RollbackIncludesEarlierSaveChanges()
    {
        var ids = await Seed();
        await using (var db = database.Create())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var employee = await db.Employees.SingleAsync(e => e.Id == ids.Employee); employee.FirstName = "Rolled back";
            await db.SaveChangesAsync(); await transaction.RollbackAsync();
        }
        await using var read = database.Create(); Assert.Equal("Employee", (await read.Employees.SingleAsync(e => e.Id == ids.Employee)).FirstName);
    }
    [Theory] [InlineData("0.10")] [InlineData("-1")] [InlineData("240.25")]
    public void InvalidPtoUnitsAreRejected(string value) =>
        Assert.Throws<HrisError>(() => HrisRules.QuarterUnits(decimal.Parse(value, CultureInfo.InvariantCulture), false, 240));
    [Fact] public void QuarterHoursAndMoneyStayExact()
    {
        Assert.Equal(5, HrisRules.QuarterUnits(1.25m, false, 240)); Assert.Equal(12345, HrisRules.Cents(123.45m));
        Assert.Throws<HrisError>(() => HrisRules.Cents(1.005m));
    }
    [Fact] public void PtoDatesRejectOverlongAndPastRequests()
    {
        var today = new DateOnly(2026, 9, 12);
        Assert.Throws<HrisError>(() => HrisRules.PtoDates(today.AddDays(-1), today, 8, today));
        Assert.Throws<HrisError>(() => HrisRules.PtoDates(today, today.AddDays(31), 8, today));
        Assert.Throws<HrisError>(() => HrisRules.PtoDates(today, today, 8.25m, today));
        HrisRules.PtoDates(today, today, 8, today);
    }
    [Fact] public void ManagerChainRejectsSelfAndIndirectCycles()
    {
        var first = Guid.NewGuid(); var second = Guid.NewGuid(); var third = Guid.NewGuid();
        var managers = new Dictionary<Guid, Guid?> { [first] = null, [second] = first, [third] = second };
        Assert.Throws<HrisError>(() => HrisRules.ManagerChain(managers, first, first));
        Assert.Throws<HrisError>(() => HrisRules.ManagerChain(managers, first, third));
        HrisRules.ManagerChain(managers, third, first);
    }
    [Fact] public void ManagerChainRejectsMissingAndExcessiveDepth()
    {
        var ids = Enumerable.Range(0, 35).Select(_ => Guid.NewGuid()).ToArray();
        var managers = ids.Select((id, index) => (id, manager: index == 0 ? (Guid?)null : ids[index - 1])).ToDictionary(x => x.id, x => x.manager);
        Assert.Throws<HrisError>(() => HrisRules.ManagerChain(managers, ids[^1], Guid.NewGuid()));
        Assert.Throws<HrisError>(() => HrisRules.ManagerChain(managers, ids[^1], ids[^2]));
    }
}
