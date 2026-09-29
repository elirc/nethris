namespace Nethris.Api.Domain;

public enum EmploymentStatus
{
    Active,
    OnLeave,
    Terminated
}

public enum SecurityRole
{
    Employee,
    Manager,
    Hr,
    Admin
}

public enum PtoStatus
{
    PendingManager,
    PendingHr,
    Approved,
    Rejected,
    Cancelled
}

public enum PtoDecision
{
    Approved,
    Rejected
}

