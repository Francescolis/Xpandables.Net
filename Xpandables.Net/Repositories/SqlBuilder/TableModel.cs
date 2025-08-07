//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Represents a table in a SQL query with alias management.
/// </summary>
public sealed class TableModel
{
    /// <summary>
    /// Gets the table name or expression.
    /// </summary>
    public string Name { get; init; } = default!;
    
    /// <summary>
    /// Gets the table alias.
    /// </summary>
    public string Alias { get; init; } = default!;
    
    /// <summary>
    /// Gets the schema name if specified.
    /// </summary>
    public string? Schema { get; init; }
    
    /// <summary>
    /// Gets the full qualified table name including schema.
    /// </summary>
    public string QualifiedName => !string.IsNullOrEmpty(Schema) 
        ? $"[{Schema}].[{Name}]" 
        : $"[{Name}]";
    
    /// <summary>
    /// Gets the table reference for use in FROM/JOIN clauses.
    /// </summary>
    public string TableReference => $"{QualifiedName} AS [{Alias}]";
    
    /// <summary>
    /// Creates a new table model.
    /// </summary>
    /// <param name="name">The table name.</param>
    /// <param name="alias">The table alias.</param>
    /// <param name="schema">The schema name.</param>
    /// <returns>A new table model instance.</returns>
    public static TableModel Create(string name, string alias, string? schema = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        
        return new TableModel 
        { 
            Name = name, 
            Alias = alias, 
            Schema = schema 
        };
    }
    
    /// <summary>
    /// Creates a table model from a type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="alias">The table alias.</param>
    /// <param name="schema">The schema name.</param>
    /// <returns>A new table model instance.</returns>
    public static TableModel Create<T>(string alias, string? schema = null)
    {
        var tableName = typeof(T).Name;
        return Create(tableName, alias, schema);
    }
}