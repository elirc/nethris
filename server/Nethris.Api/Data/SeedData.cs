namespace Nethris.Api.Data;

/// <summary>The original automatic default-password seeder is intentionally retired.</summary>
public static class SeedData
{
    [Obsolete("Initialize a new learning database through POST /api/setup and WorkshopSeed.")]
    public static Task EnsureSeededAsync(HrisDbContext db) => throw new InvalidOperationException("Automatic legacy seeding is disabled. Use the workshop setup screen.");
}
