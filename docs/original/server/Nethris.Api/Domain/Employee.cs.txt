namespace Nethris.Api.Domain;

public sealed class Employee
{
    public Guid Id { get; set; }
    public required string EmployeeNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public DateOnly HireDate { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public decimal PtoBalanceHours { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid JobRoleId { get; set; }
    public JobRole? JobRole { get; set; }
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public List<Employee> DirectReports { get; set; } = [];
    public UserAccount? UserAccount { get; set; }
    public List<PtoRequest> PtoRequests { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}";
}

