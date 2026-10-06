namespace ThaiX.Application.Common.Models;

public sealed record NotificationSendResult
{
    public required bool Success { get; init; }

    public string? ProviderMessageId { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public TimeSpan? RetryAfter { get; init; }

    public static NotificationSendResult Sent(string? providerMessageId)
    {
        return new NotificationSendResult
        {
            Success = true,
            ProviderMessageId = providerMessageId
        };
    }

    public static NotificationSendResult Failed(string errorCode, string errorMessage, TimeSpan? retryAfter = null)
    {
        return new NotificationSendResult
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            RetryAfter = retryAfter
        };
    }
}
