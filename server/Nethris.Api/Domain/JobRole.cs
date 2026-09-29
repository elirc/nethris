namespace Nethris.Api.Domain;

public sealed class JobRole : IVersioned
{
    public long Version { get; set; } = 1;
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public required string Title { get; set; }
    public required string Level { get; set; }
    public decimal SalaryBandMin { get; set; }
    public decimal SalaryBandMax { get; set; }
    public List<Employee> Employees { get; set; } = [];
}

