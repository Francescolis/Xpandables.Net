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

namespace System.Entities.Data;

/// <summary>
/// Provides a PostgreSQL implementation of <see cref="ISqlBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation generates PostgreSQL compatible queries with:
/// <list type="bullet">
/// <item>Double quote identifier quoting ("TableName")</item>
/// <item>$ parameter prefix with positional parameters ($1, $2)</item>
/// <item>LIMIT/OFFSET for pagination</item>
/// <item>ILIKE for case-insensitive pattern matching (optional)</item>
/// </list>
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class PostgreSqlBuilder : SqlBuilderBase
{
    /// <inheritdoc />
    public override SqlDialect Dialect => SqlDialect.PostgreSql;

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

        // Remove any existing quotes and re-wrap
        identifier = identifier.Trim('"');
        return $"\"{identifier}\"";
    }

    /// <inheritdoc />
    protected override void AppendPaging(StringBuilder sql, int? skip, int? take)
    {
        // PostgreSQL uses LIMIT/OFFSET
        if (take.HasValue)
        {
            sql.Append(CultureInfo.InvariantCulture, $" LIMIT {take.Value}");
        }

        if (skip.HasValue && skip.Value > 0)
        {
            sql.Append(CultureInfo.InvariantCulture, $" OFFSET {skip.Value}");
        }
    }

    /// <summary>
    /// Translates string.Contains to PostgreSQL LIKE with escape handling.
    /// </summary>
    /// <remarks>
    /// Uses standard LIKE. For case-insensitive matching, use ILIKE instead.
    /// </remarks>
    protected override string TranslateStringContains(
        System.Linq.Expressions.MethodCallExpression methodCall,
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
    /// Translates string.StartsWith to PostgreSQL LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringStartsWith(
        System.Linq.Expressions.MethodCallExpression methodCall,
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
    /// Translates string.EndsWith to PostgreSQL LIKE with escape handling.
    /// </summary>
    protected override string TranslateStringEndsWith(
        System.Linq.Expressions.MethodCallExpression methodCall,
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
        /// Escapes special characters in a LIKE pattern for PostgreSQL.
        /// </summary>
        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
        }
    }
