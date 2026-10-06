namespace ThaiX.Application.Common.Exceptions;

/// <summary>
/// Exception for expected business/application failures.
/// Carries error code and message for API mapping.
/// </summary>
public sealed class OperationFailedException : Exception
{
    public string ErrorCode { get; }

    public OperationFailedException(string errorCode, string errorMessage)
        : base(errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            throw new ArgumentException("Error code is required.", nameof(errorCode));

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required.", nameof(errorMessage));

        ErrorCode = errorCode;
    }
}
