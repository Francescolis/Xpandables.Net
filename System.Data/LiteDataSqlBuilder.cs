/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Reflection;
using System.Text;

namespace System.Data;

/// <summary>
/// Provides a SQLite implementation of <see cref="IDataSqlBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation generates SQLite compatible queries with:
/// <list type="bullet">
/// <item>Double-quote identifier quoting ("TableName")</item>
/// <item>@ parameter prefix</item>
/// <item>LIMIT/OFFSET for pagination</item>
/// <item>RETURNING clause for identity retrieval (SQLite 3.35+)</item>
/// </list>
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class LiteDataSqlBuilder : DataSqlBuilderBase
{
    /// <inheritdoc />
    public override SqlDialect Dialect => SqlDialect.SQLite;

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

        identifier = identifier.Trim('"');
        return $"\"{identifier}\"";
    }

    /// <inheritdoc />
    protected override void AppendPaging(StringBuilder sql, int? skip, int? take)
    {
        if (take.HasValue)
        {
            sql.Append(CultureInfo.InvariantCulture, $" LIMIT {take.Value}");

            if (skip.HasValue)
            {
                sql.Append(CultureInfo.InvariantCulture, $" OFFSET {skip.Value}");
            }
        }
        else if (skip.HasValue)
        {
            // SQLite requires LIMIT before OFFSET; -1 means no limit
            sql.Append(CultureInfo.InvariantCulture, $" LIMIT -1 OFFSET {skip.Value}");
        }
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Thrown when a join uses <see cref="SqlJoinType.Right"/> or <see cref="SqlJoinType.Full"/>,
    /// which are not supported by SQLite.
    /// </exception>
    protected override void AppendJoins(
        StringBuilder sql,
        IReadOnlyList<IJoinSpecification> joins,
        IReadOnlyList<TableBinding> bindings,
        List<SqlParameter> parameters)
    {
        for (int i = 0; i < joins.Count; i++)
        {
            SqlJoinType joinType = joins[i].JoinType;
            if (joinType is SqlJoinType.Right or SqlJoinType.Full)
            {
                throw new NotSupportedException(
                    $"SQLite does not support '{joinType}' joins. Use INNER JOIN, LEFT JOIN, or CROSS JOIN instead.");
            }
        }

        base.AppendJoins(sql, joins, bindings, parameters);
    }

    /// <inheritdoc />
    public override SqlQueryResult BuildInsertWithIdentity<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TData, TIdentity>(TData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ResetParameterIndex();

        var parameters = new List<SqlParameter>();
        var sql = new StringBuilder();

        string tableName = GetTableName<TData>();
        IReadOnlyDictionary<string, string> columnMappings = GetColumnMappings<TData>();

        PropertyInfo identityProperty = typeof(TData)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(IsDatabaseGeneratedIdentity)
            ?? throw new InvalidOperationException("No identity property found on data.");

        string identityColumnName = columnMappings.TryGetValue(identityProperty.Name, out string? mapped)
            ? mapped
            : identityProperty.Name;

        Type type = typeof(TData);
        var columns = new List<string>();
        var values = new List<string>();

        foreach ((string? propertyName, string? columnName) in columnMappings)
        {
            PropertyInfo? property = type.GetProperty(propertyName);
            if (property == null || !property.CanRead)
                continue;

            if (IsDatabaseGeneratedIdentity(property))
                continue;

            object? value = property.GetValue(data);
            string paramName = NextParameterName();

            columns.Add(QuoteIdentifier(columnName));
            values.Add($"{ParameterPrefix}{paramName}");
            parameters.Add(new SqlParameter(paramName, value));
        }

        sql.Append("INSERT INTO ");
        sql.Append(tableName);
        sql.Append(" (");
        sql.Append(string.Join(", ", columns));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", values));
        sql.Append(CultureInfo.InvariantCulture, $") RETURNING {QuoteIdentifier(identityColumnName)}; ");

        return new SqlQueryResult(sql.ToString(), parameters);
    }

    /// <summary>
    /// Translates string.Contains to SQLite LIKE with escape handling.
    /// </summary>
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
        return $"({column} LIKE {ParameterPrefix}{paramName} ESCAPE '\\')";
    }

    /// <summary>
    /// Translates string.StartsWith to SQLite LIKE with escape handling.
    /// </summary>
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
        return $"({column} LIKE {ParameterPrefix}{paramName} ESCAPE '\\')";
    }

    /// <summary>
    /// Translates string.EndsWith to SQLite LIKE with escape handling.
    /// </summary>
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
        return $"({column} LIKE {ParameterPrefix}{paramName} ESCAPE '\\')";
    }

    /// <summary>
    /// Escapes special characters in a LIKE pattern for SQLite.
    /// </summary>
    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
