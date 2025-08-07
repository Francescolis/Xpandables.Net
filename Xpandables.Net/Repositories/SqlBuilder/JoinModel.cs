//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Represents the type of SQL join.
/// </summary>
public enum JoinType
{
    /// <summary>
    /// Inner join.
    /// </summary>
    Inner,
    
    /// <summary>
    /// Left outer join.
    /// </summary>
    Left,
    
    /// <summary>
    /// Right outer join.
    /// </summary>
    Right,
    
    /// <summary>
    /// Full outer join.
    /// </summary>
    Full,
    
    /// <summary>
    /// Cross join.
    /// </summary>
    Cross
}

/// <summary>
/// Represents a join in a SQL query.
/// </summary>
public sealed class JoinModel
{
    /// <summary>
    /// Gets the type of join.
    /// </summary>
    public JoinType JoinType { get; init; }
    
    /// <summary>
    /// Gets the table being joined.
    /// </summary>
    public TableModel Table { get; init; } = default!;
    
    /// <summary>
    /// Gets the join condition.
    /// </summary>
    public string Condition { get; init; } = default!;
    
    /// <summary>
    /// Gets the SQL representation of the join.
    /// </summary>
    public string ToSql()
    {
        var joinKeyword = JoinType switch
        {
            JoinType.Inner => "INNER JOIN",
            JoinType.Left => "LEFT OUTER JOIN",
            JoinType.Right => "RIGHT OUTER JOIN", 
            JoinType.Full => "FULL OUTER JOIN",
            JoinType.Cross => "CROSS JOIN",
            _ => throw new InvalidOperationException($"Unknown join type: {JoinType}")
        };
        
        return JoinType == JoinType.Cross 
            ? $"{joinKeyword} {Table.TableReference}"
            : $"{joinKeyword} {Table.TableReference} ON {Condition}";
    }
    
    /// <summary>
    /// Creates a new join model.
    /// </summary>
    /// <param name="joinType">The type of join.</param>
    /// <param name="table">The table being joined.</param>
    /// <param name="condition">The join condition.</param>
    /// <returns>A new join model instance.</returns>
    public static JoinModel Create(JoinType joinType, TableModel table, string condition = "")
    {
        ArgumentNullException.ThrowIfNull(table);
        
        if (joinType != JoinType.Cross)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(condition);
        }
        
        return new JoinModel
        {
            JoinType = joinType,
            Table = table,
            Condition = condition
        };
    }
}