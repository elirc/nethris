namespace Nethris.Api.Domain;

public sealed class Department
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? CostCenter { get; set; }
    public Guid? LeadEmployeeId { get; set; }
    public Employee? LeadEmployee { get; set; }
    public List<Employee> Employees { get; set; } = [];
}

