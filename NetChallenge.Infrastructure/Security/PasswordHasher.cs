using System.Security.Cryptography;

namespace NetChallenge.Infrastructure.Security;

public static class PasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int KeySizeBytes = 32;
    private const int Iterations = 100_000;

    public static (string passwordHash, string passwordSalt) HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySizeBytes
        );

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool VerifyPassword(string password, string expectedHashBase64, string saltBase64)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(expectedHashBase64) || string.IsNullOrWhiteSpace(saltBase64))
        {
            return false;
        }

        var salt = Convert.FromBase64String(saltBase64);
        var expected = Convert.FromBase64String(expectedHashBase64);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            expected.Length
        );

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

