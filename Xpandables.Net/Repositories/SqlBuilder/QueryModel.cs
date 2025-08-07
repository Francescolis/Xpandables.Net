//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

using System.Collections.ObjectModel;

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Internal model representing the complete state of a SQL query.
/// </summary>
internal sealed class QueryModel
{
    /// <summary>
    /// Gets the main table for the FROM clause.
    /// </summary>
    public TableModel? FromTable { get; set; }
    
    /// <summary>
    /// Gets the SELECT columns/expressions.
    /// </summary>
    public List<string> SelectColumns { get; } = [];
    
    /// <summary>
    /// Gets the JOIN clauses.
    /// </summary>
    public List<JoinModel> Joins { get; } = [];
    
    /// <summary>
    /// Gets the WHERE conditions.
    /// </summary>
    public List<string> WhereConditions { get; } = [];
    
    /// <summary>
    /// Gets the GROUP BY expressions.
    /// </summary>
    public List<string> GroupByColumns { get; } = [];
    
    /// <summary>
    /// Gets the HAVING conditions.
    /// </summary>
    public List<string> HavingConditions { get; } = [];
    
    /// <summary>
    /// Gets the ORDER BY clauses.
    /// </summary>
    public List<OrderByModel> OrderByColumns { get; } = [];
    
    /// <summary>
    /// Gets or sets the SKIP value for pagination.
    /// </summary>
    public int? Skip { get; set; }
    
    /// <summary>
    /// Gets or sets the TAKE value for pagination.
    /// </summary>
    public int? Take { get; set; }
    
    /// <summary>
    /// Gets the query parameters.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; } = [];
    
    /// <summary>
    /// Gets the table aliases for lookup.
    /// </summary>
    public Dictionary<Type, string> TableAliases { get; } = [];
    
    /// <summary>
    /// Gets whether the query is distinct.
    /// </summary>
    public bool IsDistinct { get; set; }
    
    /// <summary>
    /// Gets all tables referenced in the query (main table + joined tables).
    /// </summary>
    public ReadOnlyCollection<TableModel> AllTables
    {
        get
        {
            var tables = new List<TableModel>();
            if (FromTable is not null)
            {
                tables.Add(FromTable);
            }
            
            tables.AddRange(Joins.Select(j => j.Table));
            return tables.AsReadOnly();
        }
    }
    
    /// <summary>
    /// Gets whether the query has any joins.
    /// </summary>
    public bool HasJoins => Joins.Count > 0;
    
    /// <summary>
    /// Gets whether the query has a WHERE clause.
    /// </summary>
    public bool HasWhere => WhereConditions.Count > 0;
    
    /// <summary>
    /// Gets whether the query has a GROUP BY clause.
    /// </summary>
    public bool HasGroupBy => GroupByColumns.Count > 0;
    
    /// <summary>
    /// Gets whether the query has a HAVING clause.
    /// </summary>
    public bool HasHaving => HavingConditions.Count > 0;
    
    /// <summary>
    /// Gets whether the query has an ORDER BY clause.
    /// </summary>
    public bool HasOrderBy => OrderByColumns.Count > 0;
    
    /// <summary>
    /// Gets whether the query has pagination.
    /// </summary>
    public bool HasPagination => Skip.HasValue || Take.HasValue;
    
    /// <summary>
    /// Adds a parameter to the query.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The parameter name with prefix.</returns>
    public string AddParameter(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        var paramName = $"@{name}";
        var counter = 0;
        var originalName = paramName;
        
        while (Parameters.ContainsKey(paramName))
        {
            paramName = $"{originalName}_{++counter}";
        }
        
        Parameters[paramName] = value;
        return paramName;
    }
    
    /// <summary>
    /// Gets the alias for a table type.
    /// </summary>
    /// <typeparam name="T">The table type.</typeparam>
    /// <returns>The table alias.</returns>
    public string? GetTableAlias<T>()
    {
        return TableAliases.TryGetValue(typeof(T), out var alias) ? alias : null;
    }
    
    /// <summary>
    /// Registers a table alias for a type.
    /// </summary>
    /// <typeparam name="T">The table type.</typeparam>
    /// <param name="alias">The table alias.</param>
    public void RegisterTableAlias<T>(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        TableAliases[typeof(T)] = alias;
    }
    
    /// <summary>
    /// Clears all query state.
    /// </summary>
    public void Clear()
    {
        FromTable = null;
        SelectColumns.Clear();
        Joins.Clear();
        WhereConditions.Clear();
        GroupByColumns.Clear();
        HavingConditions.Clear();
        OrderByColumns.Clear();
        Parameters.Clear();
        TableAliases.Clear();
        Skip = null;
        Take = null;
        IsDistinct = false;
    }
}