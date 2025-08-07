//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Provides SQL functions that can be used in queries.
/// </summary>
public static class SqlFunction
{
    /// <summary>
    /// Generates a COUNT expression.
    /// </summary>
    /// <param name="expression">The expression to count.</param>
    /// <returns>The COUNT SQL expression.</returns>
    public static string Count(string expression = "*")
    {
        return $"COUNT({expression})";
    }
    
    /// <summary>
    /// Generates a COUNT expression for a specific column.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>The COUNT SQL expression.</returns>
    public static string Count<T>(System.Linq.Expressions.Expression<Func<T, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var expression = ParseColumnFromExpression(selector);
        return $"COUNT({expression})";
    }
    
    private static string ParseColumnFromExpression<T>(System.Linq.Expressions.Expression<Func<T, object>> selector)
    {
        if (selector.Body is System.Linq.Expressions.MemberExpression member)
        {
            if (member.Expression is System.Linq.Expressions.ParameterExpression param)
            {
                return $"[{param.Name}].[{member.Member.Name}]";
            }
        }
        else if (selector.Body is System.Linq.Expressions.UnaryExpression { Operand: System.Linq.Expressions.MemberExpression memberExp })
        {
            if (memberExp.Expression is System.Linq.Expressions.ParameterExpression param)
            {
                return $"[{param.Name}].[{memberExp.Member.Name}]";
            }
        }
        
        throw new NotSupportedException("Only simple member access expressions are supported");
    }
    
    /// <summary>
    /// Generates a SUM expression.
    /// </summary>
    /// <param name="expression">The expression to sum.</param>
    /// <returns>The SUM SQL expression.</returns>
    public static string Sum(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        return $"SUM({expression})";
    }
    
    /// <summary>
    /// Generates a SUM expression for a specific column.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>The SUM SQL expression.</returns>
    public static string Sum<T>(System.Linq.Expressions.Expression<Func<T, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var expression = ParseColumnFromExpression(selector);
        return $"SUM({expression})";
    }
    
    /// <summary>
    /// Generates an AVG expression.
    /// </summary>
    /// <param name="expression">The expression to average.</param>
    /// <returns>The AVG SQL expression.</returns>
    public static string Avg(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        return $"AVG({expression})";
    }
    
    /// <summary>
    /// Generates an AVG expression for a specific column.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>The AVG SQL expression.</returns>
    public static string Avg<T>(System.Linq.Expressions.Expression<Func<T, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var expression = ParseColumnFromExpression(selector);
        return $"AVG({expression})";
    }
    
    /// <summary>
    /// Generates a MIN expression.
    /// </summary>
    /// <param name="expression">The expression to find minimum.</param>
    /// <returns>The MIN SQL expression.</returns>
    public static string Min(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        return $"MIN({expression})";
    }
    
    /// <summary>
    /// Generates a MIN expression for a specific column.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>The MIN SQL expression.</returns>
    public static string Min<T>(System.Linq.Expressions.Expression<Func<T, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var expression = ParseColumnFromExpression(selector);
        return $"MIN({expression})";
    }
    
    /// <summary>
    /// Generates a MAX expression.
    /// </summary>
    /// <param name="expression">The expression to find maximum.</param>
    /// <returns>The MAX SQL expression.</returns>
    public static string Max(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        return $"MAX({expression})";
    }
    
    /// <summary>
    /// Generates a MAX expression for a specific column.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>The MAX SQL expression.</returns>
    public static string Max<T>(System.Linq.Expressions.Expression<Func<T, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var expression = ParseColumnFromExpression(selector);
        return $"MAX({expression})";
    }
}