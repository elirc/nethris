namespace Nethris.Api.Domain;

public interface IVersioned
{
    Guid Id { get; set; }
    long Version { get; set; }
}

public sealed class WorkshopSettings
{
    public int Id { get; set; } = 1;
    public required string Name { get; set; }
    public required string SigningKey { get; set; }
}

public sealed class AuthSession
{
    public Guid Id { get; set; }
    public Guid UserAccountId { get; set; }
    public UserAccount? UserAccount { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class OperationReceipt
{
    public Guid Id { get; set; }
    public Guid UserAccountId { get; set; }
    public UserAccount? UserAccount { get; set; }
    public required string Key { get; set; }
    public required string Fingerprint { get; set; }
    public required string ResponseJson { get; set; }
    public int StatusCode { get; set; }
    public string? ETag { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class HrisError(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public static HrisError Invalid(string message) => new(422, "validation", message);
    public static HrisError Conflict(string message) => new(409, "conflict", message);
    public static HrisError Missing() => new(404, "not_found", "Record not found or not visible to your account.");
    public static HrisError Forbidden(string message = "Your current account cannot perform this action.") => new(403, "permission", message);
}
