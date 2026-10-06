using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class Budget : BaseAuditableEntity
{
    private Budget()
    {
    }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public BudgetPeriod Period { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public static Budget Create(Guid categoryId, BudgetPeriod period, decimal amount, DateTime startDate, DateTime endDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (endDate < startDate)
            throw new InvalidOperationException("Budget end date must be on or after start date.");

        return new Budget
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Period = period,
            Amount = amount,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public void Update(Guid categoryId, BudgetPeriod period, decimal amount, DateTime startDate, DateTime endDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (endDate < startDate)
            throw new InvalidOperationException("Budget end date must be on or after start date.");

        CategoryId = categoryId;
        Period = period;
        Amount = amount;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
