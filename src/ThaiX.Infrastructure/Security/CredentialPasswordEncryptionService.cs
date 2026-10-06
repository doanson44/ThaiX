using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.Security;

/// <summary>
/// AES-256 encryption dedicated to credential account passwords.
/// Key source: CredentialEncryptionKey from configuration/environment.
/// </summary>
public sealed class CredentialPasswordEncryptionService : ICredentialPasswordEncryptionService
{
    private readonly byte[] _key;

    public CredentialPasswordEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["CredentialEncryptionKey"];
        if (string.IsNullOrWhiteSpace(keyBase64))
        {
            throw new InvalidOperationException(
                "CredentialEncryptionKey is not configured. Provide a base64-encoded 32-byte key via configuration or environment variables.");
        }

        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32)
        {
            throw new InvalidOperationException("CredentialEncryptionKey must be exactly 32 bytes (256 bits).");
        }
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
