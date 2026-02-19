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

namespace System.Data;

/// <summary>
/// Provides a default implementation of <see cref="IDataSqlBuilderFactory"/> that creates 
/// SQL builders for different database dialects.
/// </summary>
[RequiresDynamicCode("SQL builders require dynamic code generation for expression compilation.")]
public sealed class DataSqlBuilderFactory : IDataSqlBuilderFactory
{
    private static readonly Lazy<MsDataSqlBuilder> _sqlServerBuilder = new(() => new MsDataSqlBuilder());
    private static readonly Lazy<PostgreDataSqlBuilder> _postgreSqlBuilder = new(() => new PostgreDataSqlBuilder());
    private static readonly Lazy<MyDataSqlBuilder> _mySqlBuilder = new(() => new MyDataSqlBuilder());

    /// <inheritdoc />
    public IDataSqlBuilder Create(SqlDialect dialect)
    {
        return dialect switch
        {
            SqlDialect.SqlServer => _sqlServerBuilder.Value,
            SqlDialect.PostgreSql => _postgreSqlBuilder.Value,
            SqlDialect.MySql => _mySqlBuilder.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(dialect), dialect, $"Unsupported SQL dialect: {dialect}")
        };
    }

    /// <inheritdoc />
    public IDataSqlBuilder Create(string providerInvariantName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);

		SqlDialect dialect = GetDialectFromProvider(providerInvariantName);
        return Create(dialect);
    }

    /// <summary>
    /// Gets the SQL dialect for a provider invariant name.
    /// </summary>
    /// <param name="providerInvariantName">The provider invariant name.</param>
    /// <returns>The corresponding SQL dialect.</returns>
    /// <exception cref="ArgumentException">Thrown when the provider is not recognized.</exception>
    public static SqlDialect GetDialectFromProvider(string providerInvariantName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);

		// Normalize for comparison
		string normalized = providerInvariantName.ToUpperInvariant();

        return normalized switch
        {
            _ when normalized.Contains("SQLCLIENT", StringComparison.Ordinal) => SqlDialect.SqlServer,
            _ when normalized.Contains("NPGSQL", StringComparison.Ordinal) => SqlDialect.PostgreSql,
            _ when normalized.Contains("MYSQL", StringComparison.Ordinal) => SqlDialect.MySql,
            DbProviders.MsSqlServer.InvariantName => SqlDialect.SqlServer,
            DbProviders.PostgreSql.InvariantName => SqlDialect.PostgreSql,
            DbProviders.MySql.InvariantName => SqlDialect.MySql,
            _ => throw new ArgumentException(
                $"Unknown database provider: '{providerInvariantName}'. " +
                $"Supported providers: {DbProviders.MsSqlServer.InvariantName}, {DbProviders.PostgreSql.InvariantName}, {DbProviders.MySql.InvariantName}",
                nameof(providerInvariantName))
        };
    }
}
