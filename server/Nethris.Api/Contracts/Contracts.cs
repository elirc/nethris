using Nethris.Api.Domain;

namespace Nethris.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);
public sealed record OrgNodeResponse(Guid Id, string Name, string Title, string Department, string? Location, IReadOnlyList<OrgNodeResponse> Reports);
public sealed record AuditLogResponse(Guid Id, DateTimeOffset CreatedAt, string ActorEmail, string Action, string EntityType, string EntityId, string? Details);
