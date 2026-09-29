namespace Nethris.Api.Domain;

public sealed class UserAccount
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public SecurityRole Role { get; set; } = SecurityRole.Employee;
    public bool IsActive { get; set; } = true;
}

