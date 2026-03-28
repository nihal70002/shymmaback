using System.Security.Cryptography;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            16,              // salt size
            100_000,         // iterations
            HashAlgorithmName.SHA256
        );

        var salt = deriveBytes.Salt;
        var key = deriveBytes.GetBytes(32);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedKey = Convert.FromBase64String(parts[1]);

        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256
        );

        var key = deriveBytes.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(key, storedKey);
    }
}
