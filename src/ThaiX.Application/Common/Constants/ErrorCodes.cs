namespace ThaiX.Application.Common.Constants;

/// <summary>
/// Centralized error codes for the application.
/// Used across all layers for consistent error handling.
/// </summary>
public static class ErrorCodes
{
    // Error code prefixes
    public const string VALIDATION_PREFIX = "VAL_";
    public const string BUSINESS_PREFIX = "BIZ_";

    // Authentication & Authorization Errors (AUTH_*)
    public const string INVALID_CREDENTIALS = "AUTH_INVALID_CREDENTIALS";
    public const string ACCOUNT_LOCKED = "AUTH_ACCOUNT_LOCKED";
    public const string ACCOUNT_NOT_ALLOWED = "AUTH_ACCOUNT_NOT_ALLOWED";
    public const string EMAIL_NOT_CONFIRMED = "AUTH_EMAIL_NOT_CONFIRMED";
    public const string INVALID_TOKEN = "AUTH_INVALID_TOKEN";
    public const string TOKEN_EXPIRED = "AUTH_TOKEN_EXPIRED";
    public const string UNAUTHORIZED = "AUTH_UNAUTHORIZED";
    public const string FORBIDDEN = "AUTH_FORBIDDEN";
    public const string INSUFFICIENT_PERMISSIONS = "AUTH_INSUFFICIENT_PERMISSIONS";

    // Validation Errors (VAL_*)
    public const string VALIDATION_FAILED = "VAL_VALIDATION_FAILED";
    public const string INVALID_REQUEST = "VAL_INVALID_REQUEST";
    public const string REQUIRED_FIELD = "VAL_REQUIRED_FIELD";
    public const string INVALID_FORMAT = "VAL_INVALID_FORMAT";
    public const string INVALID_LENGTH = "VAL_INVALID_LENGTH";
    public const string INVALID_RANGE = "VAL_INVALID_RANGE";
    public const string INVALID_PAGE_NUMBER = "VAL_INVALID_PAGE_NUMBER";
    public const string INVALID_PAGE_SIZE = "VAL_INVALID_PAGE_SIZE";
    public const string INVALID_SORT_FIELD = "VAL_INVALID_SORT_FIELD";
    public const string INVALID_SEARCH_TERM = "VAL_INVALID_SEARCH_TERM";
    public const string PASSWORD_MISMATCH = "VAL_PASSWORD_MISMATCH";

    // Authentication Flow Errors
    public const string REGISTRATION_FAILED = "AUTH_REGISTRATION_FAILED";
    public const string CHANGE_PASSWORD_FAILED = "AUTH_CHANGE_PASSWORD_FAILED";
    public const string TWO_FACTOR_REQUIRED = "AUTH_TWO_FACTOR_REQUIRED";
    public const string TWO_FACTOR_INVALID = "AUTH_TWO_FACTOR_INVALID";
    public const string TWO_FACTOR_FAILED = "AUTH_TWO_FACTOR_FAILED";
    public const string EMAIL_CONFIRMATION_FAILED = "AUTH_EMAIL_CONFIRMATION_FAILED";
    public const string PASSWORD_RESET_FAILED = "AUTH_PASSWORD_RESET_FAILED";
    public const string EXTERNAL_LOGIN_FAILED = "AUTH_EXTERNAL_LOGIN_FAILED";
    public const string RECOVERY_CODE_INVALID = "AUTH_RECOVERY_CODE_INVALID";

    // Business Logic Errors (BIZ_*)
    public const string BUSINESS_RULE_VIOLATION = "BIZ_BUSINESS_RULE_VIOLATION";
    public const string DUPLICATE_ENTRY = "BIZ_DUPLICATE_ENTRY";
    public const string RESOURCE_NOT_FOUND = "BIZ_RESOURCE_NOT_FOUND";
    public const string RESOURCE_ALREADY_EXISTS = "BIZ_RESOURCE_ALREADY_EXISTS";
    public const string OPERATION_NOT_ALLOWED = "BIZ_OPERATION_NOT_ALLOWED";
    public const string INVALID_STATE = "BIZ_INVALID_STATE";
    public const string CONCURRENCY_CONFLICT = "BIZ_CONCURRENCY_CONFLICT";

    // File Errors (FILE_*)
    public const string FILE_NOT_FOUND = "FILE_NOT_FOUND";
    public const string FILE_TOO_LARGE = "FILE_TOO_LARGE";
    public const string FILE_TYPE_NOT_ALLOWED = "FILE_TYPE_NOT_ALLOWED";

    // Contact Errors (CONTACT_*)
    public const string CONTACT_NOT_FOUND = "CONTACT_NOT_FOUND";
    public const string CONTACT_ITEM_NOT_FOUND = "CONTACT_ITEM_NOT_FOUND";
    public const string CONTACT_DUPLICATE_TAG = "CONTACT_DUPLICATE_TAG";
    /// <summary>CSV does not match the standard import template (required headers: FirstName, LastName, etc.).</summary>
    public const string CONTACT_IMPORT_INVALID_CSV_TEMPLATE = "CONTACT_IMPORT_INVALID_CSV_TEMPLATE";

    // API Client Errors (CLIENT_*)
    public const string INVALID_CLIENT_CREDENTIALS = "CLIENT_INVALID_CREDENTIALS";
    public const string CLIENT_NOT_FOUND = "CLIENT_NOT_FOUND";
    public const string CLIENT_INACTIVE = "CLIENT_INACTIVE";
    public const string UNAUTHORIZED_SCOPE = "CLIENT_UNAUTHORIZED_SCOPE";
    public const string CLIENT_ALREADY_EXISTS = "CLIENT_ALREADY_EXISTS";

    // System Errors (SYS_*)
    public const string INTERNAL_ERROR = "SYS_INTERNAL_ERROR";
    public const string DATABASE_ERROR = "SYS_DATABASE_ERROR";
    public const string EXTERNAL_SERVICE_ERROR = "SYS_EXTERNAL_SERVICE_ERROR";
    public const string CONFIGURATION_ERROR = "SYS_CONFIGURATION_ERROR";
    public const string TIMEOUT = "SYS_TIMEOUT";

    // Rate Limiting Errors (RATE_*)
    public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";
    public const string TOO_MANY_REQUESTS = "RATE_TOO_MANY_REQUESTS";
}

/// <summary>
/// User-friendly error messages corresponding to error codes.
/// Can be localized in the future.
/// </summary>
public static class ErrorMessages
{
    // Authentication & Authorization
    public const string INVALID_CREDENTIALS = "Invalid email or password.";
    public const string ACCOUNT_LOCKED = "Your account has been locked. Please contact support.";
    public const string ACCOUNT_NOT_ALLOWED = "Sign in not allowed. Please confirm your email.";
    public const string EMAIL_NOT_CONFIRMED = "Please confirm your email address before signing in.";
    public const string INVALID_TOKEN = "The provided token is invalid.";
    public const string TOKEN_EXPIRED = "Your session has expired. Please sign in again.";
    public const string UNAUTHORIZED = "You are not authorized to access this resource.";
    public const string FORBIDDEN = "You do not have permission to perform this action.";
    public const string INSUFFICIENT_PERMISSIONS = "Insufficient permissions to perform this action.";

    // Validation
    public const string VALIDATION_FAILED = "One or more validation errors occurred.";
    public const string INVALID_REQUEST = "The request is invalid. Please check the input.";
    public const string REQUIRED_FIELD = "This field is required.";
    public const string INVALID_FORMAT = "The format is invalid.";

    // Business Logic
    public const string RESOURCE_NOT_FOUND = "The requested resource was not found.";
    public const string RESOURCE_ALREADY_EXISTS = "A resource with the same identifier already exists.";
    public const string OPERATION_NOT_ALLOWED = "This operation is not allowed in the current state.";
    public const string CONCURRENCY_CONFLICT = "The resource has been modified by another user. Please refresh and try again.";

    // API Client
    public const string INVALID_CLIENT_CREDENTIALS = "Invalid client credentials.";
    public const string CLIENT_INACTIVE = "The API client is inactive.";
    public const string UNAUTHORIZED_SCOPE = "The requested scope is not authorized for this client.";

    // System
    public const string INTERNAL_ERROR = "An unexpected error occurred. Please try again later.";
    public const string DATABASE_ERROR = "A database error occurred. Please contact support.";
    public const string EXTERNAL_SERVICE_ERROR = "An external service is currently unavailable.";

    // Rate Limiting
    public const string RATE_LIMIT_EXCEEDED = "Rate limit exceeded. Please try again later.";
}
