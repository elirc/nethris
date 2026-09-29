using System.Globalization;
using System.Net.Mail;

namespace Nethris.Api.Domain;

public static class HrisRules
{
    public const int MaximumEmployees = 2000;
    public const int MaximumManagerDepth = 32;
    public static string Text(string? value, string field, int maximum = 120)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximum || normalized.Any(char.IsControl))
            throw HrisError.Invalid($"{field} must contain 1–{maximum} characters without control characters.");
        return normalized;
    }
    public static string? OptionalText(string? value, string field, int maximum = 250) =>
        string.IsNullOrWhiteSpace(value) ? null : Text(value, field, maximum);
    public static string Email(string? value)
    {
        var email = Text(value, "Email", 254).ToLowerInvariant();
        if (!MailAddress.TryCreate(email, out var address) || address.Address != email || !email.Contains('.'))
            throw HrisError.Invalid("Enter a valid email address.");
        return email;
    }
    public static string Password(string? value)
    {
        if (value is null || value.Length < 12 || value.Length > 128)
            throw HrisError.Invalid("Use a password containing 12–128 characters.");
        return value;
    }
    public static int QuarterUnits(decimal hours, bool allowZero, decimal maximum)
    {
        if (hours < (allowZero ? 0m : .25m) || hours > maximum || decimal.Truncate(hours * 4m) != hours * 4m)
            throw HrisError.Invalid($"Hours must be in quarter-hour increments between {(allowZero ? "0" : "0.25")} and {maximum.ToString(CultureInfo.InvariantCulture)}.");
        return checked((int)(hours * 4m));
    }
    public static long Cents(decimal amount)
    {
        if (amount < 0 || amount > 100_000_000m || decimal.Round(amount, 2) != amount)
            throw HrisError.Invalid("Salary amounts must be 0–100,000,000 with at most two decimal places.");
        return checked((long)(amount * 100m));
    }
    public static T EnumValue<T>(T value) where T : struct, Enum =>
        Enum.IsDefined(value) ? value : throw HrisError.Invalid("Unknown enum value.");
    public static void Version(IVersioned value, long expected)
    {
        if (value.Version != expected) throw HrisError.Conflict("This record changed. Reload and review your draft before saving.");
    }
    public static void PtoDates(DateOnly start, DateOnly end, decimal hours, DateOnly today)
    {
        QuarterUnits(hours, false, 240);
        if (start < today || start > today.AddYears(1) || end < start || end.DayNumber - start.DayNumber > 30)
            throw HrisError.Invalid("Choose a future/current start within one year and an inclusive span of at most 31 calendar dates.");
        if (hours > (end.DayNumber - start.DayNumber + 1) * 8m)
            throw HrisError.Invalid("This workshop permits at most eight PTO hours per calendar date. Weekends are not excluded automatically.");
    }
    public static void ManagerChain(IReadOnlyDictionary<Guid, Guid?> managers, Guid employeeId, Guid? proposedManager)
    {
        var visited = new HashSet<Guid> { employeeId };
        var current = proposedManager;
        while (current is not null)
        {
            if (!visited.Add(current.Value)) throw HrisError.Conflict("A manager relationship cannot form a cycle.");
            if (visited.Count > MaximumManagerDepth + 1) throw HrisError.Conflict("Manager chains are limited to 32 levels in this workshop.");
            if (!managers.TryGetValue(current.Value, out current)) throw HrisError.Invalid("Manager does not exist.");
        }
    }
}
