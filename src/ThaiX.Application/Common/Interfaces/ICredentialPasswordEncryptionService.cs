namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Encrypts and decrypts credential account passwords using a dedicated key.
/// </summary>
public interface ICredentialPasswordEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
