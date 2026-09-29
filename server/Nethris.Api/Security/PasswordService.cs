using System.Security.Cryptography;
using Nethris.Api.Domain;

namespace Nethris.Api.Security;

public static class PasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600_000;
    public static string Hash(string password)
    {
        HrisRules.Password(password);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
    public static bool Verify(string? password, string? storedHash)
    {
        if (password is null || password.Length is < 12 or > 128 || storedHash is null || storedHash.Length > 256) return false;
        var parts = storedHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations) || iterations is < 100_000 or > 1_000_000) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[1]); var expected = Convert.FromBase64String(parts[2]);
            if (salt.Length != SaltSize || expected.Length != KeySize) return false;
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySize);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
    public static bool NeedsRehash(string storedHash) => !storedHash.StartsWith(Iterations + ".", StringComparison.Ordinal);
    public static string DummyHash => Iterations + "." + Convert.ToBase64String(new byte[SaltSize]) + "." + Convert.ToBase64String(new byte[KeySize]);
}
