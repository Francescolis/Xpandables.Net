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
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

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

        foreach (var param in Parameters)
        {
            var dbParam = command.CreateParameter();
            dbParam.ParameterName = param.Name;
            dbParam.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(dbParam);
        }
    }
}

/// <summary>
/// Represents a SQL parameter with a name and value.
/// </summary>
/// <param name="Name">The parameter name (without the @ or : prefix).</param>
/// <param name="Value">The parameter value.</param>
public readonly record struct SqlParameter(string Name, object? Value);

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
/// </remarks>
public interface IDataSqlBuilder
{
    /// <summary>
    /// Gets the SQL dialect this builder generates.
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
