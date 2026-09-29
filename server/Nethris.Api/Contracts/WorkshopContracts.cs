using Nethris.Api.Domain;

namespace Nethris.Api.Contracts;

public sealed record EmployeeInput(string EmployeeNumber, string FirstName, string LastName, string Email,
    string? Phone, string? Location, DateOnly HireDate, EmploymentStatus Status, decimal PtoBalanceHours,
    Guid DepartmentId, Guid JobRoleId, Guid? ManagerId);
public sealed record DepartmentInput(string Name, string? CostCenter, Guid? LeadEmployeeId, bool IsActive = true);
public sealed record JobRoleInput(string Title, string Level, decimal SalaryBandMin, decimal SalaryBandMax, bool IsActive = true);
public sealed record PtoInput(DateOnly StartDate, DateOnly EndDate, decimal Hours, string Reason);
public sealed record DecisionInput(PtoDecision Decision, string? Comment);
public sealed record EmployeeView(Guid Id, long Version, string EmployeeNumber, string FirstName, string LastName,
    string FullName, string Email, string? Phone, string? Location, DateOnly HireDate, EmploymentStatus Status,
    decimal? PtoBalanceHours, Guid DepartmentId, string Department, Guid JobRoleId, string JobRole,
    string RoleLevel, Guid? ManagerId, string? ManagerName, int DirectReportCount);
public sealed record DepartmentView(Guid Id, long Version, string Name, string? CostCenter, Guid? LeadEmployeeId,
    string? LeadName, bool IsActive, int Headcount);
public sealed record JobRoleView(Guid Id, long Version, string Title, string Level, decimal? SalaryBandMin,
    decimal? SalaryBandMax, bool IsActive);
public sealed record PtoView(Guid Id, long Version, Guid EmployeeId, string EmployeeName, string Department,
    DateOnly StartDate, DateOnly EndDate, decimal Hours, string Reason, PtoStatus Status, Guid? AssignedManagerId,
    DateTimeOffset CreatedAt, IReadOnlyList<ApprovalView> Approvals);
public sealed record ApprovalView(Guid Id, Guid ApproverEmployeeId, string ApproverName, string Stage,
    PtoDecision Decision, string? Comment, DateTimeOffset CreatedAt);
public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int Offset, int Limit);
