using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ThaiX.Infrastructure.Security;

/// <summary>
/// Provides secure password hashing for API client secrets.
/// Uses PBKDF2 with HMACSHA256.
/// </summary>
public static class SecretHasher
{
    private const int SaltSize = 128 / 8; // 128 bits
    private const int HashSize = 256 / 8; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    /// <summary>
    /// Hashes a plaintext secret.
    /// Returns a base64-encoded string containing salt and hash.
    /// </summary>
    public static string HashSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be empty.", nameof(secret));

        // Generate random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the secret
        byte[] hash = KeyDerivation.Pbkdf2(
            password: secret,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Combine salt and hash
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a plaintext secret against a hash.
    /// </summary>
    public static bool VerifySecret(string secret, string hash)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // Decode the hash
            byte[] hashBytes = Convert.FromBase64String(hash);

            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            // Extract salt
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract stored hash
            byte[] storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // Hash the provided secret with the extracted salt
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: secret,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            // Compare hashes (constant-time comparison)
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random secret.
    /// Returns a base64-encoded string of specified byte length.
    /// </summary>
    public static string GenerateSecret(int byteLength = 32)
    {
        if (byteLength < 16)
            throw new ArgumentException("Secret must be at least 16 bytes.", nameof(byteLength));

        byte[] secretBytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(secretBytes);
    }
}
