namespace Nethris.Api.Domain;

public sealed class PtoRequest
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Hours { get; set; }
    public required string Reason { get; set; }
    public PtoStatus Status { get; set; } = PtoStatus.PendingManager;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<PtoApproval> Approvals { get; set; } = [];
}

public sealed class PtoApproval
{
    public Guid Id { get; set; }
    public Guid PtoRequestId { get; set; }
    public PtoRequest? PtoRequest { get; set; }
    public Guid ApproverEmployeeId { get; set; }
    public Employee? ApproverEmployee { get; set; }
    public PtoDecision Decision { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

