using System.Net;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Constants;

public static class AuthErrorCodes
{
    public const string AccountNotAllowed = "AUTH_ACCOUNT_NOT_ALLOWED";
    public const string Lockout = "AUTH_LOCKOUT";
    public const string AccountLocked = "AUTH_ACCOUNT_LOCKED";

    public static bool IsLockout(ApiException ex) =>
        ex.StatusCode == HttpStatusCode.Locked
        || string.Equals(ex.Code, Lockout, StringComparison.OrdinalIgnoreCase)
        || string.Equals(ex.Code, AccountLocked, StringComparison.OrdinalIgnoreCase)
        || ex.Code.Contains("LOCKOUT", StringComparison.OrdinalIgnoreCase);

    public static bool IsAccountNotAllowed(ApiException ex) =>
        string.Equals(ex.Code, AccountNotAllowed, StringComparison.OrdinalIgnoreCase);
}
