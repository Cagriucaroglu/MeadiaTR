using BCrypt.Net;
using MediaTR.Domain.Services;

namespace MediaTR.Infrastructure.Services.Authentication;

/// <summary>
/// BCrypt implementation of IPasswordHasher.
/// Uses work factor 12 (2^12 = 4096 iterations) which is the 2025 security standard.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // 2^12 iterations (2025 standardı)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            // Invalid hash format
            return false;
        }
    }
}
