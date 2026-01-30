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

namespace System.Entities.Data;

/// <summary>
/// Provides constants and utilities for working with database providers in a provider-agnostic manner.
/// </summary>
/// <remarks>
/// This class contains invariant names for common database providers that can be used with 
/// <see cref="DbProviderFactories.GetFactory(string)"/> to create provider-specific instances.
/// Before using a provider, ensure it is registered using <c>DbProviderFactories.RegisterFactory</c>.
/// </remarks>
public static class DbProviders
{
    /// <summary>
    /// Microsoft SQL Server provider information.
    /// </summary>
    public static class MsSqlServer
    {
        /// <summary>
        /// The invariant name for Microsoft SQL Server provider.
        /// </summary>
        /// <remarks>Uses Microsoft.Data.SqlClient package.</remarks>
        public const string InvariantName = "Microsoft.Data.SqlClient";

        /// <summary>
        /// The display name for SQL Server.
        /// </summary>
        public const string DisplayName = "Microsoft SQL Server";
    }

    /// <summary>
    /// PostgreSQL provider information.
    /// </summary>
    public static class PostgreSql
    {
        /// <summary>
        /// The invariant name for PostgreSQL provider.
        /// </summary>
        /// <remarks>Uses Npgsql package.</remarks>
        public const string InvariantName = "Npgsql";

        /// <summary>
        /// The display name for PostgreSQL.
        /// </summary>
        public const string DisplayName = "PostgreSQL";
    }

    /// <summary>
    /// MySQL provider information.
    /// </summary>
    public static class MySql
    {
        /// <summary>
        /// The invariant name for MySQL provider.
        /// </summary>
        /// <remarks>Uses MySqlConnector package (recommended over MySql.Data).</remarks>
        public const string InvariantName = "MySqlConnector";

        /// <summary>
        /// The display name for MySQL.
        /// </summary>
        public const string DisplayName = "MySQL";
    }

    /// <summary>
    /// Gets the <see cref="DbProviderFactory"/> for the specified provider invariant name.
    /// </summary>
    /// <param name="providerInvariantName">The invariant name of the provider 
    /// (e.g., <see cref="MsSqlServer.InvariantName"/>).</param>
    /// <returns>The <see cref="DbProviderFactory"/> instance for the specified provider.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provider is not registered or the invariant name is invalid.
    /// </exception>
    /// <example>
    /// <code>
    /// // Ensure the provider is registered first
    /// DbProviderFactories.RegisterFactory(
    ///     DbProviders.MsSqlServer.InvariantName, 
    ///     Microsoft.Data.SqlClient.SqlClientFactory.Instance);
    /// 
    /// // Then get the factory
    /// var factory = DbProviders.GetFactory(DbProviders.SqlServer.InvariantName);
    /// using var connection = factory.CreateConnection();
    /// </code>
    /// </example>
    public static DbProviderFactory GetFactory(string providerInvariantName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);

        if (!DbProviderFactories.TryGetFactory(providerInvariantName, out var factory))
        {
            throw new ArgumentException(
                $"Database provider '{providerInvariantName}' is not registered. " +
                $"Call DbProviderFactories.RegisterFactory() to register the provider first.",
                nameof(providerInvariantName));
        }

        return factory;
    }

    /// <summary>
    /// Attempts to get the <see cref="DbProviderFactory"/> for the specified provider invariant name.
    /// </summary>
    /// <param name="providerInvariantName">The invariant name of the provider.</param>
    /// <param name="factory">When this method returns, contains the factory if found; otherwise, null.</param>
    /// <returns><see langword="true"/> if the factory was found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetFactory(string providerInvariantName, out DbProviderFactory? factory)
    {
        if (string.IsNullOrWhiteSpace(providerInvariantName))
        {
            factory = null;
            return false;
        }

        return DbProviderFactories.TryGetFactory(providerInvariantName, out factory);
    }

    /// <summary>
    /// Checks whether a provider with the specified invariant name is registered.
    /// </summary>
    /// <param name="providerInvariantName">The invariant name of the provider.</param>
    /// <returns><see langword="true"/> if the provider is registered; otherwise, <see langword="false"/>.</returns>
    public static bool IsRegistered(string providerInvariantName)
    {
        if (string.IsNullOrWhiteSpace(providerInvariantName))
            return false;

        return DbProviderFactories.TryGetFactory(providerInvariantName, out _);
    }

    /// <summary>
    /// Registers a database provider factory if it is not already registered.
    /// </summary>
    /// <param name="providerInvariantName">The invariant name for the provider.</param>
    /// <param name="factory">The <see cref="DbProviderFactory"/> instance to register.</param>
    /// <returns><see langword="true"/> if the factory was registered; 
    /// <see langword="false"/> if it was already registered.</returns>
    public static bool RegisterIfNotExists(string providerInvariantName, DbProviderFactory factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);
        ArgumentNullException.ThrowIfNull(factory);

        if (IsRegistered(providerInvariantName))
            return false;

        DbProviderFactories.RegisterFactory(providerInvariantName, factory);
        return true;
    }
}
