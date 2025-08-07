//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Represents the direction of ordering.
/// </summary>
public enum OrderDirection
{
    /// <summary>
    /// Ascending order.
    /// </summary>
    Ascending,
    
    /// <summary>
    /// Descending order.
    /// </summary>
    Descending
}

/// <summary>
/// Represents an ORDER BY clause in a SQL query.
/// </summary>
public sealed class OrderByModel
{
    /// <summary>
    /// Gets the column expression to order by.
    /// </summary>
    public string Expression { get; init; } = default!;
    
    /// <summary>
    /// Gets the ordering direction.
    /// </summary>
    public OrderDirection Direction { get; init; }
    
    /// <summary>
    /// Gets the position in the ORDER BY clause (for ThenBy scenarios).
    /// </summary>
    public int Position { get; init; }
    
    /// <summary>
    /// Gets the SQL representation of this ordering.
    /// </summary>
    public string ToSql()
    {
        var direction = Direction == OrderDirection.Ascending ? "ASC" : "DESC";
        return $"{Expression} {direction}";
    }
    
    /// <summary>
    /// Creates a new order by model.
    /// </summary>
    /// <param name="expression">The column expression.</param>
    /// <param name="direction">The ordering direction.</param>
    /// <param name="position">The position in the ORDER BY clause.</param>
    /// <returns>A new order by model instance.</returns>
    public static OrderByModel Create(string expression, OrderDirection direction, int position = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        
        return new OrderByModel
        {
            Expression = expression,
            Direction = direction,
            Position = position
        };
    }
}