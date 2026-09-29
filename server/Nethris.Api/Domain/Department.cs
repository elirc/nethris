namespace Nethris.Api.Domain;

public sealed class Department : IVersioned
{
    public long Version { get; set; } = 1;
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public required string Name { get; set; }
    public string? CostCenter { get; set; }
    public Guid? LeadEmployeeId { get; set; }
    public Employee? LeadEmployee { get; set; }
    public List<Employee> Employees { get; set; } = [];
}

