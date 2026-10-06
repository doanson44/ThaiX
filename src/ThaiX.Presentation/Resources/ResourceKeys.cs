namespace ThaiX.Presentation.Resources;

/// <summary>
/// Centralized localization resource key constants for the API layer.
/// Use these instead of hardcoded string literals with IStringLocalizer.
/// </summary>
public static class ResourceKeys
{
    public static class Success
    {
        public const string TwoFactorRequired = "Success.TwoFactorRequired";
        public const string LoginSuccessful = "Success.LoginSuccessful";
        public const string RegistrationSuccessful = "Success.RegistrationSuccessful";
        public const string LogoutSuccessful = "Success.LogoutSuccessful";
        public const string PasswordChanged = "Success.PasswordChanged";
        public const string EmailConfirmed = "Success.EmailConfirmed";
        public const string ConfirmationEmailSent = "Success.ConfirmationEmailSent";
        public const string PasswordResetEmailSent = "Success.PasswordResetEmailSent";
        public const string PasswordResetCompleted = "Success.PasswordResetCompleted";
        public const string TwoFactorDisabled = "Success.TwoFactorDisabled";
        public const string AuthenticatorReset = "Success.AuthenticatorReset";
        public const string ExternalLoginRemoved = "Success.ExternalLoginRemoved";
        public const string AccountDeleted = "Success.AccountDeleted";
    }

    public static class Error
    {
        public const string PasswordMismatch = "Error.PasswordMismatch";
        public const string UserNotAuthenticated = "Error.UserNotAuthenticated";
        public const string UserNotFound = "Error.UserNotFound";
        public const string EmailConfirmationFailed = "Error.EmailConfirmationFailed";
        public const string PasswordResetFailed = "Error.PasswordResetFailed";
        public const string TwoFactorCodeInvalid = "Error.TwoFactorCodeInvalid";
        public const string InvalidCredentials = "Error.InvalidCredentials";
        public const string ValidationFailed = "Error.ValidationFailed";
        public const string Forbidden = "Error.Forbidden";
        public const string InternalError = "Error.InternalError";
        public const string AccountLocked = "Error.AccountLocked";
        public const string AccountNotAllowed = "Error.AccountNotAllowed";
        public const string Unauthorized = "Error.Unauthorized";
        public const string InsufficientPermissions = "Error.InsufficientPermissions";
        public const string InvalidClientCredentials = "Error.InvalidClientCredentials";
        public const string ClientInactive = "Error.ClientInactive";
        public const string UnauthorizedScope = "Error.UnauthorizedScope";
        public const string ResourceNotFound = "Error.ResourceNotFound";
        public const string DuplicateEntry = "Error.DuplicateEntry";
        public const string OperationNotAllowed = "Error.OperationNotAllowed";
        public const string ConcurrencyConflict = "Error.ConcurrencyConflict";
        public const string DatabaseError = "Error.DatabaseError";
        public const string ExternalServiceError = "Error.ExternalServiceError";
        public const string RateLimitExceeded = "Error.RateLimitExceeded";
        public const string ContactImportInvalidCsvTemplate = "Error.ContactImportInvalidCsvTemplate";
    }
}
