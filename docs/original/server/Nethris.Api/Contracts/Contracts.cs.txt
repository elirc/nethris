using Nethris.Api.Domain;

namespace Nethris.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, CurrentUserResponse User);

public sealed record CurrentUserResponse(
    Guid UserId,
    Guid EmployeeId,
    string Email,
    string Name,
    SecurityRole Role);

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    string? CostCenter,
    string? LeadName,
    int Headcount);

public sealed record JobRoleResponse(
    Guid Id,
    string Title,
    string Level,
    decimal SalaryBandMin,
    decimal SalaryBandMax);

public sealed record EmployeeResponse(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? Location,
    DateOnly HireDate,
    EmploymentStatus Status,
    decimal PtoBalanceHours,
    Guid DepartmentId,
    string Department,
    Guid JobRoleId,
    string JobRole,
    string RoleLevel,
    Guid? ManagerId,
    string? ManagerName,
    int DirectReportCount);

public sealed record UpsertEmployeeRequest(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Location,
    DateOnly HireDate,
    EmploymentStatus Status,
    decimal PtoBalanceHours,
    Guid DepartmentId,
    Guid JobRoleId,
    Guid? ManagerId,
    SecurityRole? AccountRole);

public sealed record OrgNodeResponse(
    Guid Id,
    string Name,
    string Title,
    string Department,
    string? Location,
    IReadOnlyList<OrgNodeResponse> Reports);

public sealed record CreatePtoRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Hours,
    string Reason);

public sealed record PtoResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string Department,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Hours,
    string Reason,
    PtoStatus Status,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PtoApprovalResponse> Approvals);

public sealed record PtoApprovalResponse(
    Guid Id,
    string ApproverName,
    PtoDecision Decision,
    string? Comment,
    DateTimeOffset CreatedAt);

public sealed record DecidePtoRequest(PtoDecision Decision, string? Comment);

public sealed record AuditLogResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    string ActorEmail,
    string Action,
    string EntityType,
    string EntityId,
    string? Details);

