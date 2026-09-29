using Nethris.Api.Domain;
using Nethris.Api.Security;

namespace Nethris.Api.Data;

/// <summary>Optional fictional colleagues, created inside the bootstrap transaction.</summary>
public static class WorkshopSeed
{
    public static async Task AddColleagues(HrisDbContext db, Employee administrator, string workshopPassword)
    {
        var engineering = new Department { Id = Guid.NewGuid(), Name = "Engineering", CostCenter = "ENG-100" };
        var people = new Department { Id = Guid.NewGuid(), Name = "People", CostCenter = "PPL-200" };
        var managerRole = new JobRole { Id = Guid.NewGuid(), Title = "Engineering manager", Level = "M1", SalaryBandMin = 110000, SalaryBandMax = 170000 };
        var engineerRole = new JobRole { Id = Guid.NewGuid(), Title = "Software engineer", Level = "IC2", SalaryBandMin = 80000, SalaryBandMax = 120000 };
        var peopleRole = new JobRole { Id = Guid.NewGuid(), Title = "People partner", Level = "IC3", SalaryBandMin = 85000, SalaryBandMax = 130000 };
        Employee Person(string number, string first, string last, string email, Department department, JobRole role, Employee manager) => new()
        { Id = Guid.NewGuid(), EmployeeNumber = number, FirstName = first, LastName = last, Email = email, Department = department,
            JobRole = role, Manager = manager, HireDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1), PtoBalanceHours = 120, Location = "Remote" };
        var hr = Person("EMP-0002", "Jordan", "Patel", "hr@example.test", people, peopleRole, administrator);
        var manager = Person("EMP-0003", "Maya", "Singh", "manager@example.test", engineering, managerRole, administrator);
        var employee = Person("EMP-0004", "Alex", "Rivera", "employee@example.test", engineering, engineerRole, manager);
        var colleague = Person("EMP-0005", "Sam", "Okafor", "colleague@example.test", engineering, engineerRole, manager);
        var reviewer = Person("EMP-0006", "Priya", "Iyer", "reviewer@example.test", people, peopleRole, administrator);
        db.AddRange(engineering, people, managerRole, engineerRole, peopleRole, hr, manager, employee, colleague, reviewer);
        foreach (var (person, role) in new[] { (hr, SecurityRole.Hr), (manager, SecurityRole.Manager), (employee, SecurityRole.Employee), (colleague, SecurityRole.Employee), (reviewer, SecurityRole.Hr) })
            db.UserAccounts.Add(new UserAccount { Id = Guid.NewGuid(), Employee = person, Email = person.Email, Role = role, PasswordHash = PasswordService.Hash(workshopPassword) });
        await db.SaveChangesAsync();
        engineering.LeadEmployeeId = manager.Id; people.LeadEmployeeId = hr.Id;
        await db.SaveChangesAsync();
    }
}
