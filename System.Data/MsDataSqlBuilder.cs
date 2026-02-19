/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace System.Data;

/// <summary>
/// Provides a SQL Server (T-SQL) implementation of <see cref="IDataSqlBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation generates T-SQL compatible queries with:
/// <list type="bullet">
/// <item>Square bracket identifier quoting ([TableName])</item>
/// <item>@ parameter prefix</item>
/// <item>OFFSET/FETCH for pagination (SQL Server 2012+)</item>
/// <item>TOP for simple limit without offset</item>
/// </list>
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class MsDataSqlBuilder : DataSqlBuilderBase
{
    /// <inheritdoc />
    public override SqlDialect Dialect => SqlDialect.SqlServer;

    /// <inheritdoc />
    public override string ParameterPrefix => "@";

    /// <inheritdoc />
    protected override string LimitKeyword => "TOP";

    /// <inheritdoc />
    protected override bool LimitBeforeColumns => true;

    /// <inheritdoc />
    public override string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        // Remove any existing brackets and re-wrap
        identifier = identifier.Trim('[', ']');
        return $"[{identifier}]";
    }

    /// <inheritdoc />
    protected override void AppendPaging(StringBuilder sql, int? skip, int? take)
    {
        // SQL Server uses OFFSET/FETCH (requires ORDER BY)
        // For TOP without offset, it's already handled in BuildSelect

        if (skip.HasValue || (take.HasValue && skip.HasValue))
        {
            // OFFSET/FETCH requires an ORDER BY clause
            // If no ORDER BY was specified, we need to add a default one
            if (!sql.ToString().Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
            {
                sql.Append(" ORDER BY (SELECT NULL)");
            }

            sql.Append(CultureInfo.InvariantCulture, $" OFFSET {skip ?? 0} ROWS");

            if (take.HasValue)
            {
                sql.Append(CultureInfo.InvariantCulture, $" FETCH NEXT {take.Value} ROWS ONLY");
            }
        }
    }

    /// <summary>
    /// Translates string.Contains to SQL Server LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringContains(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        (Linq.Expressions.Expression? columnExpr, Linq.Expressions.Expression? valueExpr) = GetStringMethodOperands(methodCall);
		string column = TranslateExpression(columnExpr, columnMappings, parameters);
		object? value = ExtractConstantValue(valueExpr);
		string escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
		string paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{escapedValue}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <inheritdoc />
    protected override string TranslateStringContains(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<Linq.Expressions.ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        (Linq.Expressions.Expression? columnExpr, Linq.Expressions.Expression? valueExpr) = GetStringMethodOperands(methodCall);
		string column = TranslateExpression(columnExpr, bindings, parameters);
		object? value = ExtractConstantValue(valueExpr);
		string escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
		string paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{escapedValue}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.StartsWith to SQL Server LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringStartsWith(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        (Linq.Expressions.Expression? columnExpr, Linq.Expressions.Expression? valueExpr) = GetStringMethodOperands(methodCall);
		string column = TranslateExpression(columnExpr, columnMappings, parameters);
		object? value = ExtractConstantValue(valueExpr);
		string escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
		string paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"{escapedValue}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <inheritdoc />
    protected override string TranslateStringStartsWith(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<Linq.Expressions.ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        (Linq.Expressions.Expression? columnExpr, Linq.Expressions.Expression? valueExpr) = GetStringMethodOperands(methodCall);
		string column = TranslateExpression(columnExpr, bindings, parameters);
		object? value = ExtractConstantValue(valueExpr);
		string escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
		string paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"{escapedValue}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.EndsWith to SQL Server LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringEndsWith(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        (Linq.Expressions.Expression? columnExpr, Linq.Expressions.Expression? valueExpr) = GetStringMethodOperands(methodCall);
		string column = TranslateExpression(columnExpr, columnMappings, parameters);
		object? value = ExtractConstantValue(valueExpr);
		string escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
		string paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{escapedValue}"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <inheritdoc />
    protected override string TranslateStringEndsWith(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<Linq.Expressions.ParameterExpression, TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        (Linq.Expressions.Expression? columnExpr, Linq.Expressions.Expression? valueExpr) = GetStringMethodOperands(methodCall);
		string column = TranslateExpression(columnExpr, bindings, parameters);
		object? value = ExtractConstantValue(valueExpr);
		string escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
		string paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{escapedValue}"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Escapes special characters in a LIKE pattern for SQL Server.
    /// </summary>
    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
    }
}
