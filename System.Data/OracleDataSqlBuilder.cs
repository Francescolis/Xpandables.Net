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
/// Provides an Oracle Database implementation of <see cref="IDataSqlBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation generates Oracle SQL compatible queries with:
/// <list type="bullet">
/// <item>Double quote identifier quoting ("TableName")</item>
/// <item>: parameter prefix (for ODP.NET / Oracle.ManagedDataAccess)</item>
/// <item>OFFSET/FETCH for pagination (Oracle 12c+)</item>
/// <item>RETURNING INTO for identity retrieval</item>
/// </list>
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class OracleDataSqlBuilder : DataSqlBuilderBase
{
	/// <inheritdoc />
	public override SqlDialect Dialect => SqlDialect.Oracle;

	/// <inheritdoc />
	public override string ParameterPrefix => ":";

	/// <inheritdoc />
	protected override string LimitKeyword => "FETCH FIRST";

	/// <inheritdoc />
	protected override bool LimitBeforeColumns => false;

	/// <inheritdoc />
	public override string QuoteIdentifier(string identifier)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

		// Remove any existing double quotes and re-wrap
		identifier = identifier.Trim('"');
		return $"\"{identifier}\"";
	}

	/// <inheritdoc />
	protected override void AppendPaging(StringBuilder sql, int? skip, int? take)
	{
		// Oracle 12c+ uses OFFSET/FETCH NEXT (requires ORDER BY)
		if (skip.HasValue || take.HasValue)
		{
			// OFFSET/FETCH requires an ORDER BY clause in Oracle
			if (!sql.ToString().Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
			{
				sql.Append(" ORDER BY NULL");
			}

			if (skip.HasValue && skip.Value > 0)
			{
				sql.Append(CultureInfo.InvariantCulture, $" OFFSET {skip.Value} ROWS");
			}
			else if (take.HasValue)
			{
				sql.Append(" OFFSET 0 ROWS");
			}

			if (take.HasValue)
			{
				sql.Append(CultureInfo.InvariantCulture, $" FETCH NEXT {take.Value} ROWS ONLY");
			}
		}
	}

	/// <inheritdoc />
	public override SqlQueryResult BuildInsertWithIdentity<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TData, TIdentity>(TData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		ResetParameterIndex();

		var parameters = new List<SqlParameter>();
		var sql = new StringBuilder();

		var tableName = GetTableName<TData>();
		var columnMappings = GetColumnMappings<TData>();
		PropertyInfo identityProperty = typeof(TData).GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.FirstOrDefault(IsDatabaseGeneratedIdentity)
			?? throw new InvalidOperationException("No identity property found on data.");

		string columnKey = columnMappings.TryGetValue(identityProperty.Name, out string? mappedColumn)
			? mappedColumn
			: identityProperty.Name;

		var type = typeof(TData);
		var columns = new List<string>();
		var values = new List<string>();

		foreach (var (propertyName, columnName) in columnMappings)
		{
			var property = type.GetProperty(propertyName);
			if (property == null || !property.CanRead)
			{
				continue;
			}

			if (IsDatabaseGeneratedIdentity(property))
			{
				continue;
			}

			var value = property.GetValue(data);
			var paramName = NextParameterName();

			columns.Add(QuoteIdentifier(columnName));
			values.Add($"{ParameterPrefix}{paramName}");
			parameters.Add(new SqlParameter(paramName, value));
		}

		string returnParamName = NextParameterName();

		sql.Append("INSERT INTO ");
		sql.Append(tableName);
		sql.Append(" (");
		sql.Append(string.Join(", ", columns));
		sql.Append(") VALUES (");
		sql.Append(string.Join(", ", values));
		sql.Append(CultureInfo.InvariantCulture, $") RETURNING {QuoteIdentifier(columnKey)} INTO {ParameterPrefix}{returnParamName}");

		parameters.Add(new SqlParameter(returnParamName, null));

		return new SqlQueryResult(sql.ToString(), parameters);
	}

	/// <summary>
	/// Translates string.Contains to Oracle LIKE with escape handling.
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
	/// Translates string.StartsWith to Oracle LIKE with escape handling.
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
	/// Translates string.EndsWith to Oracle LIKE with escape handling.
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
	/// Translates string.IsNullOrWhiteSpace to Oracle SQL using table bindings.
	/// </summary>
	/// <remarks>Oracle uses TRIM instead of LTRIM(RTRIM(...)).</remarks>
	protected override string TranslateStringIsNullOrWhiteSpace(
		Linq.Expressions.MethodCallExpression methodCall,
		IReadOnlyDictionary<Linq.Expressions.ParameterExpression, TableBinding> bindings,
		List<SqlParameter> parameters)
	{
		string column = TranslateExpression(methodCall.Arguments[0], bindings, parameters);
		return $"({column} IS NULL OR TRIM({column}) = '')";
	}

	/// <summary>
	/// Escapes special characters in a LIKE pattern for Oracle.
	/// </summary>
	private static string EscapeLikePattern(string value)
	{
		return value
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("%", "\\%", StringComparison.Ordinal)
			.Replace("_", "\\_", StringComparison.Ordinal);
	}
}
