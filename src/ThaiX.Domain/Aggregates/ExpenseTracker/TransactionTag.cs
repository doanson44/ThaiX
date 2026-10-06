using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class TransactionTag : BaseAuditableEntity
{
    private TransactionTag()
    {
    }

    public Guid TransactionId { get; private set; }
    public Transaction Transaction { get; private set; } = null!;
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;

    public static TransactionTag Create(Guid transactionId, Guid tagId)
    {
        return new TransactionTag
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            TagId = tagId
        };
    }

    public void Update(Guid transactionId, Guid tagId)
    {
        TransactionId = transactionId;
        TagId = tagId;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
