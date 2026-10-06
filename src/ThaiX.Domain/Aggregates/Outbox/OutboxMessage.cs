using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Outbox;

public sealed class OutboxMessage : BaseEntity
{
    private OutboxMessage() { }

    public OutboxMessage(string type, string content)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Event type cannot be null or whitespace.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Event content cannot be null or whitespace.", nameof(content));
        }

        Type = type;
        Content = content;
        OccurredOnUtc = DateTime.UtcNow;
        ProcessedOnUtc = null;
        Error = null;
    }

    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));
        }

        ProcessedOnUtc = DateTime.UtcNow;
        Error = error;
    }
}
