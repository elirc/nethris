using Microsoft.EntityFrameworkCore;
using Nethris.Api.Domain;
using Nethris.Api.Security;

namespace Nethris.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(HrisDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Employees.AnyAsync())
        {
            return;
        }

        var engineering = new Department { Id = Guid.NewGuid(), Name = "Engineering", CostCenter = "ENG-100" };
        var people = new Department { Id = Guid.NewGuid(), Name = "People", CostCenter = "PPL-200" };
        var finance = new Department { Id = Guid.NewGuid(), Name = "Finance", CostCenter = "FIN-300" };
        var ops = new Department { Id = Guid.NewGuid(), Name = "Operations", CostCenter = "OPS-400" };

        var ceoRole = new JobRole { Id = Guid.NewGuid(), Title = "Chief Executive Officer", Level = "Executive", SalaryBandMin = 220000, SalaryBandMax = 320000 };
        var vpPeopleRole = new JobRole { Id = Guid.NewGuid(), Title = "VP People", Level = "Executive", SalaryBandMin = 170000, SalaryBandMax = 240000 };
        var engManagerRole = new JobRole { Id = Guid.NewGuid(), Title = "Engineering Manager", Level = "M4", SalaryBandMin = 150000, SalaryBandMax = 210000 };
        var seniorEngineerRole = new JobRole { Id = Guid.NewGuid(), Title = "Senior Software Engineer", Level = "IC4", SalaryBandMin = 130000, SalaryBandMax = 180000 };
        var analystRole = new JobRole { Id = Guid.NewGuid(), Title = "People Ops Analyst", Level = "IC2", SalaryBandMin = 70000, SalaryBandMax = 95000 };
        var accountantRole = new JobRole { Id = Guid.NewGuid(), Title = "Staff Accountant", Level = "IC2", SalaryBandMin = 72000, SalaryBandMax = 98000 };

        var riley = Employee("NET-1001", "Riley", "Chen", "admin@nethris.local", "New York", new DateOnly(2019, 1, 14), ops, ceoRole, null, 184);
        var jordan = Employee("NET-1002", "Jordan", "Patel", "hr@nethris.local", "Remote", new DateOnly(2020, 3, 2), people, vpPeopleRole, riley, 168);
        var maya = Employee("NET-1003", "Maya", "Singh", "maya@nethris.local", "San Francisco", new DateOnly(2021, 8, 9), engineering, engManagerRole, riley, 156);
        var alex = Employee("NET-1004", "Alex", "Rivera", "alex@nethris.local", "San Francisco", new DateOnly(2022, 6, 21), engineering, seniorEngineerRole, maya, 132);
        var sam = Employee("NET-1005", "Sam", "Okafor", "sam@nethris.local", "Austin", new DateOnly(2023, 4, 10), people, analystRole, jordan, 96);
        var priya = Employee("NET-1006", "Priya", "Iyer", "priya@nethris.local", "Chicago", new DateOnly(2021, 11, 15), finance, accountantRole, riley, 148);

        db.Departments.AddRange(engineering, people, finance, ops);
        db.JobRoles.AddRange(ceoRole, vpPeopleRole, engManagerRole, seniorEngineerRole, analystRole, accountantRole);
        db.Employees.AddRange(riley, jordan, maya, alex, sam, priya);
        db.UserAccounts.AddRange(
            User(riley, SecurityRole.Admin),
            User(jordan, SecurityRole.Hr),
            User(maya, SecurityRole.Manager),
            User(alex, SecurityRole.Employee),
            User(sam, SecurityRole.Employee),
            User(priya, SecurityRole.Manager));

        var pending = new PtoRequest
        {
            Id = Guid.NewGuid(),
            Employee = alex,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(16)),
            Hours = 24,
            Reason = "Family trip",
            Status = PtoStatus.PendingManager
        };
        var approved = new PtoRequest
        {
            Id = Guid.NewGuid(),
            Employee = sam,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(4)),
            Hours = 16,
            Reason = "Long weekend",
            Status = PtoStatus.Approved
        };

        db.PtoRequests.AddRange(pending, approved);
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorEmail = "system@nethris.local",
            Action = "Seeded",
            EntityType = "Tenant",
            EntityId = "initial",
            Details = "Created seed departments, employees, user accounts, and PTO requests."
        });

        await db.SaveChangesAsync();

        engineering.LeadEmployeeId = maya.Id;
        people.LeadEmployeeId = jordan.Id;
        finance.LeadEmployeeId = priya.Id;
        ops.LeadEmployeeId = riley.Id;
        await db.SaveChangesAsync();
    }

    private static Employee Employee(
        string number,
        string firstName,
        string lastName,
        string email,
        string location,
        DateOnly hireDate,
        Department department,
        JobRole role,
        Employee? manager,
        decimal ptoBalance) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = number,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Location = location,
            HireDate = hireDate,
            Department = department,
            JobRole = role,
            Manager = manager,
            PtoBalanceHours = ptoBalance,
            Phone = "(555) 014-0000"
        };

    private static UserAccount User(Employee employee, SecurityRole role) =>
        new()
        {
            Id = Guid.NewGuid(),
            Employee = employee,
            Email = employee.Email,
            Role = role,
            PasswordHash = PasswordService.Hash("Password123!")
        };
}
