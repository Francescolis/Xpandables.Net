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
/// Provides a MySQL implementation of <see cref="IDataSqlBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation generates MySQL compatible queries with:
/// <list type="bullet">
/// <item>Backtick identifier quoting (`TableName`)</item>
/// <item>@ parameter prefix (for MySqlConnector)</item>
/// <item>LIMIT/OFFSET for pagination</item>
/// </list>
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class MyDataSqlBuilder : DataSqlBuilderBase
{
    /// <inheritdoc />
    public override SqlDialect Dialect => SqlDialect.MySql;

    /// <inheritdoc />
    public override string ParameterPrefix => "@";

    /// <inheritdoc />
    protected override string LimitKeyword => "LIMIT";

    /// <inheritdoc />
    protected override bool LimitBeforeColumns => false;

    /// <inheritdoc />
    public override string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        // Remove any existing backticks and re-wrap
        identifier = identifier.Trim('`');
        return $"`{identifier}`";
    }

    /// <inheritdoc />
    protected override void AppendPaging(StringBuilder sql, int? skip, int? take)
    {
        // MySQL uses LIMIT/OFFSET
        // Note: MySQL requires LIMIT when using OFFSET
        if (take.HasValue || skip.HasValue)
        {
            if (take.HasValue)
            {
                sql.Append(CultureInfo.InvariantCulture, $" LIMIT {take.Value}");
            }
            else if (skip.HasValue)
            {
                // MySQL requires LIMIT when using OFFSET, use a very large number
                sql.Append(" LIMIT 18446744073709551615");
            }

            if (skip.HasValue && skip.Value > 0)
            {
                sql.Append(CultureInfo.InvariantCulture, $" OFFSET {skip.Value}");
            }
        }
    }

    /// <summary>
    /// Translates string.Contains to MySQL LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringContains(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, columnMappings, parameters);
        var value = ExtractConstantValue(methodCall.Arguments[0]);
        var escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{escapedValue}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.StartsWith to MySQL LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringStartsWith(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, columnMappings, parameters);
        var value = ExtractConstantValue(methodCall.Arguments[0]);
        var escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"{escapedValue}%"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Translates string.EndsWith to MySQL LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringEndsWith(
        Linq.Expressions.MethodCallExpression methodCall,
        IReadOnlyDictionary<string, string> columnMappings,
        List<SqlParameter> parameters)
    {
        var column = TranslateExpression(methodCall.Object!, columnMappings, parameters);
        var value = ExtractConstantValue(methodCall.Arguments[0]);
        var escapedValue = EscapeLikePattern(value?.ToString() ?? string.Empty);
        var paramName = NextParameterName();
        parameters.Add(new SqlParameter(paramName, $"%{escapedValue}"));
        return $"({column} LIKE {ParameterPrefix}{paramName})";
    }

    /// <summary>
    /// Escapes special characters in a LIKE pattern for MySQL.
    /// </summary>
    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
