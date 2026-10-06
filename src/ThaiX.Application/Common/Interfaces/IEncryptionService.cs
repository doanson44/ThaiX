namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data (e.g., bank account numbers).
/// Implementation resides in the Infrastructure layer.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts the given plain text value.
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts the given cipher text back to plain text.
    /// </summary>
    string Decrypt(string cipherText);
}
