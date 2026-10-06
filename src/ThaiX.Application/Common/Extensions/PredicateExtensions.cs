using System.Linq.Expressions;

namespace ThaiX.Application.Common.Extensions;

/// <summary>
/// Extension methods for building dynamic LINQ predicates.
/// Allows composable filtering logic without breaking EF Core translation.
/// </summary>
public static class PredicateExtensions
{
    /// <summary>
    /// Creates a predicate that always returns true.
    /// Use as starting point for building dynamic filters.
    /// </summary>
    public static Expression<Func<T, bool>> True<T>()
    {
        return x => true;
    }

    /// <summary>
    /// Creates a predicate that always returns false.
    /// </summary>
    public static Expression<Func<T, bool>> False<T>()
    {
        return x => false;
    }

    /// <summary>
    /// Combines two predicates with AND logic.
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        var combined = Expression.AndAlso(left!, right!);

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Combines two predicates with OR logic.
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        var combined = Expression.OrElse(left!, right!);

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Negates a predicate (NOT logic).
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(
        this Expression<Func<T, bool>> expr)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var visitor = new ReplaceExpressionVisitor(expr.Parameters[0], parameter);
        var body = visitor.Visit(expr.Body);

        var negated = Expression.Not(body!);

        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }

    /// <summary>
    /// Internal visitor class for replacing expression parameters.
    /// Required for combining expressions with different parameter instances.
    /// </summary>
    private sealed class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}
