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
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace System.Data;

/// <summary>
/// Represents the result of building a SQL query, containing the SQL text and parameters.
/// </summary>
/// <param name="Sql">The generated SQL query text.</param>
/// <param name="Parameters">The parameters to use with the query.</param>
public readonly record struct SqlQueryResult(string Sql, IReadOnlyList<SqlParameter> Parameters)
{
	/// <summary>
	/// Creates an empty result with no SQL and no parameters.
	/// </summary>
	public static SqlQueryResult Empty => new(string.Empty, []);

	/// <summary>
	/// Applies the parameters to a <see cref="DbCommand"/>.
	/// </summary>
	/// <param name="command">The command to add parameters to.</param>
	public void ApplyParameters(DbCommand command)
	{
		ArgumentNullException.ThrowIfNull(command);

		foreach (SqlParameter param in Parameters)
		{
			DbParameter dbParam = command.CreateParameter();
			dbParam.ParameterName = param.Name;
			dbParam.Value = UnwrapParameterValue(param.Value) ?? DBNull.Value;
			command.Parameters.Add(dbParam);
		}
	}

	/// <summary>
	/// Unwraps a parameter value to an ADO.NET-compatible type.
	/// Handles enums (to underlying integer) and custom wrapper types
	/// such as strongly-typed IDs that implement <c>IPrimitive</c>.
	/// </summary>
	private static object? UnwrapParameterValue(object? value)
	{
		if (value is null or DBNull)
		{
			return value;
		}

		Type type = value.GetType();

		if (IsAdoNetCompatible(type))
		{
			return value;
		}

		Func<object, object>? unwrapper = UnwrapperCache.GetOrAdd(type, static t => BuildUnwrapper(t));

		return unwrapper?.Invoke(value) ?? value;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2070:DynamicallyAccessedMembers on Type.GetMethods",
		Justification = "Parameter wrapper types are expected to have public implicit conversion operators.")]
	private static Func<object, object>? BuildUnwrapper(Type t)
	{
		// Enum → underlying integer type
		if (t.IsEnum)
		{
			Type underlyingType = Enum.GetUnderlyingType(t);
			return v => Convert.ChangeType(v, underlyingType, CultureInfo.InvariantCulture);
		}

		// Implicit conversion to a known ADO.NET type (covers IPrimitive<T> types)
		foreach (MethodInfo method in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
		{
			if (method.Name == "op_Implicit"
				&& method.GetParameters() is [{ ParameterType: var paramType }]
				&& paramType == t
				&& IsAdoNetCompatible(method.ReturnType))
			{
				return v => method.Invoke(null, [v])!;
			}
		}

		// No unwrapping found
		return null;
	}

	private static bool IsAdoNetCompatible(Type type)
	{
		Type t = Nullable.GetUnderlyingType(type) ?? type;
		return t.IsPrimitive
			|| t == typeof(string)
			|| t == typeof(decimal)
			|| t == typeof(Guid)
			|| t == typeof(DateTime)
			|| t == typeof(DateTimeOffset)
			|| t == typeof(TimeSpan)
			|| t == typeof(byte[]);
	}

	private static readonly ConcurrentDictionary<Type, Func<object, object>?> UnwrapperCache = new();
}

/// <summary>
/// Represents a SQL parameter with a name and value.
/// </summary>
/// <param name="Name">The parameter name (without the @ or : prefix).</param>
/// <param name="Value">The parameter value.</param>
public readonly record struct SqlParameter(string Name, object? Value);

/// <summary>
/// Defines a contract for accessing SQL dialect metadata such as table names,
/// column mappings, identifier quoting, and parameter formatting.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides the schema-level information needed to map CLR types
/// to database objects without generating full SQL statements.
/// It is intended for components that need metadata only (e.g., result mappers),
/// without depending on the full query-building surface.
/// </para>
/// </remarks>
public interface IDataSqlMetadata
{
	/// <summary>
	/// Gets the SQL dialect this metadata describes.
	/// </summary>
	SqlDialect Dialect { get; }

	/// <summary>
	/// Gets the parameter prefix used by this dialect (e.g., "@" for SQL Server, "$" for PostgreSQL).
	/// </summary>
	string ParameterPrefix { get; }

	/// <summary>
	/// Quotes an identifier (table name, column name) for this dialect.
	/// </summary>
	/// <param name="identifier">The identifier to quote.</param>
	/// <returns>The quoted identifier.</returns>
	string QuoteIdentifier(string identifier);

	/// <summary>
	/// Gets the table name for an entity type.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <returns>The table name to use in SQL queries.</returns>
	string GetTableName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
		where TEntity : class;

	/// <summary>
	/// Gets the column names for an entity type.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <returns>A dictionary mapping property names to column names.</returns>
	IReadOnlyDictionary<string, string> GetColumnMappings<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
		where TEntity : class;
}

/// <summary>
/// Defines a contract for building SQL queries from specifications and entity operations.
/// </summary>
/// <remarks>
/// <para>
/// Implementations of this interface translate <see cref="IDataSpecification{TEntity, TResult}"/>
/// and entity operations into provider-specific SQL statements with parameterized queries.
/// </para>
/// <para>
/// The builder handles:
/// <list type="bullet">
/// <item>SELECT queries with filtering, ordering, and paging</item>
/// <item>INSERT statements for single and batch operations</item>
/// <item>UPDATE statements with SET clauses</item>
/// <item>DELETE statements with WHERE clauses</item>
/// </list>
/// </para>
/// <para>
/// This interface extends <see cref="IDataSqlMetadata"/> to also provide
/// schema-level metadata (table names, column mappings, quoting).
/// </para>
/// </remarks>
public interface IDataSqlBuilder : IDataSqlMetadata
{

	/// <summary>
	/// Builds a SELECT query from a query specification.
	/// </summary>
	/// <typeparam name="TEntity">The entity type to query.</typeparam>
	/// <typeparam name="TResult">The result type after projection.</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <returns>The SQL query result with parameterized query.</returns>
	SqlQueryResult BuildSelect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity, TResult>(
		IDataSpecification<TEntity, TResult> specification)
		where TEntity : class;

	/// <summary>
	/// Builds a COUNT query from a query specification.
	/// </summary>
	/// <typeparam name="TEntity">The entity type to query.</typeparam>
	/// <typeparam name="TResult">The result type (used for specification compatibility).</typeparam>
	/// <param name="specification">The query specification.</param>
	/// <returns>The SQL query result with parameterized query.</returns>
	SqlQueryResult BuildCount<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity, TResult>(
		IDataSpecification<TEntity, TResult> specification)
		where TEntity : class;

	/// <summary>
	/// Builds an INSERT statement for a single entity.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <param name="entity">The entity to insert.</param>
	/// <returns>The SQL query result with parameterized query.</returns>
	SqlQueryResult BuildInsert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
		TEntity entity)
		where TEntity : class;

	/// <summary>
	/// Builds a SQL insert statement for the specified data and returns a query result that retrieves the generated
	/// identity value.
	/// </summary>
	/// <typeparam name="TData">The type of the data to insert. Must be a reference type with public properties.</typeparam>
	/// <typeparam name="TIdentity">The type of the identity value to be returned. Must be non-nullable.</typeparam>
	/// <param name="data">The data instance to insert into the database. Cannot be null.</param>
	/// <returns>A SqlQueryResult representing the insert statement and the retrieval of the generated identity value.</returns>
	SqlQueryResult BuildInsertWithIdentity<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TData, TIdentity>(TData data)
		where TData : class
		where TIdentity : struct;

	/// <summary>
	/// Builds an INSERT statement for multiple entities.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <param name="entities">The entities to insert.</param>
	/// <returns>The SQL query result with parameterized query.</returns>
	SqlQueryResult BuildInsertBatch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
		IEnumerable<TEntity> entities)
		where TEntity : class;

	/// <summary>
	/// Builds an UPDATE statement from a specification and updater.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <param name="specification">The specification defining which entities to update.</param>
	/// <param name="updater">The updater defining the SET clauses.</param>
	/// <returns>The SQL query result with parameterized query.</returns>
	SqlQueryResult BuildUpdate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
		IDataSpecification<TEntity, TEntity> specification,
		DataUpdater<TEntity> updater)
		where TEntity : class;

	/// <summary>
	/// Builds a DELETE statement from a specification.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <param name="specification">The specification defining which entities to delete.</param>
	/// <returns>The SQL query result with parameterized query.</returns>
	SqlQueryResult BuildDelete<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
		IDataSpecification<TEntity, TEntity> specification)
		where TEntity : class;
}

/// <summary>
/// Provides factory methods for creating SQL builders for specific dialects.
/// </summary>
public interface IDataSqlBuilderFactory
{
	/// <summary>
	/// Creates a SQL builder for the specified dialect.
	/// </summary>
	/// <param name="dialect">The SQL dialect.</param>
	/// <returns>A SQL builder for the specified dialect.</returns>
	IDataSqlBuilder Create(SqlDialect dialect);

	/// <summary>
	/// Creates a SQL builder for the specified provider invariant name.
	/// </summary>
	/// <param name="providerInvariantName">The provider invariant name 
	/// (e.g., <see cref="DbProviders.MsSqlServer.InvariantName"/>).</param>
	/// <returns>A SQL builder for the provider's dialect.</returns>
	IDataSqlBuilder Create(string providerInvariantName);
}
