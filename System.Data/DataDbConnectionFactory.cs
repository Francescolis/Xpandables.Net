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

namespace System.Data;

/// <summary>
/// Provides a default implementation of <see cref="IDataDbConnectionFactory"/> that creates 
/// database connections using <see cref="DbProviderFactories"/>.
/// </summary>
/// <remarks>
/// This factory uses the registered <see cref="DbProviderFactory"/> to create connections.
/// Ensure the provider is registered before using this factory by calling 
/// <c>DbProviderFactories.RegisterFactory</c>.
/// </remarks>
/// <param name="providerInvariantName">The invariant name of the database provider 
/// (e.g., <see cref="DbProviders.MsSqlServer.InvariantName"/>).</param>
/// <param name="connectionString">The connection string to use when creating connections.</param>
/// <exception cref="ArgumentException">
/// Thrown when <paramref name="providerInvariantName"/> or <paramref name="connectionString"/> is null or empty.
/// </exception>
public sealed class DataDbConnectionFactory(string providerInvariantName, string connectionString) : IDataDbConnectionFactory
{
	private readonly DbProviderFactory _providerFactory = DbProviders.GetFactory(providerInvariantName);

	/// <inheritdoc />
	public string ProviderInvariantName { get; } = !string.IsNullOrWhiteSpace(providerInvariantName)
		? providerInvariantName
		: throw new ArgumentException("Provider invariant name cannot be null or empty.", nameof(providerInvariantName));

	/// <inheritdoc />
	public string ConnectionString { get; } = !string.IsNullOrWhiteSpace(connectionString)
		? connectionString
		: throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

	/// <inheritdoc />
	public DbConnection CreateConnection()
	{
		DbConnection connection = _providerFactory.CreateConnection()
			?? throw new InvalidOperationException(
				$"The provider '{ProviderInvariantName}' returned a null connection. " +
				$"Ensure the provider factory is correctly implemented.");

		connection.ConnectionString = ConnectionString;
		return connection;
	}

	/// <inheritdoc />
	public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
	{
		DbConnection connection = CreateConnection();

		try
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
			return connection;
		}
		catch
		{
			await connection.DisposeAsync().ConfigureAwait(false);
			throw;
		}
	}

	/// <inheritdoc />
	public DbConnection CreateOpenConnection()
	{
		DbConnection connection = CreateConnection();

		try
		{
			connection.Open();
			return connection;
		}
		catch
		{
			connection.Dispose();
			throw;
		}
	}
}
