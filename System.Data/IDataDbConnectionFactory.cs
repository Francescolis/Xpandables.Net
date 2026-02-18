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
/// Defines a factory for creating database connections in a provider-agnostic manner.
/// </summary>
/// <remarks>
/// Implementations of this interface should use <see cref="DbProviderFactories"/> to create
/// connections based on the registered provider. The factory is responsible for creating
/// connections but not for managing their lifecycle - that responsibility belongs to
/// <see cref="IDataDbConnectionScope"/>.
/// </remarks>
public interface IDataDbConnectionFactory
{
	/// <summary>
	/// Gets the invariant name of the database provider.
	/// </summary>
	/// <remarks>
	/// This should match one of the provider invariant names defined in <see cref="DbProviders"/>,
	/// such as <see cref="DbProviders.MsSqlServer.InvariantName"/>.
	/// </remarks>
	string ProviderInvariantName { get; }

	/// <summary>
	/// Gets the connection string used to create connections.
	/// </summary>
	string ConnectionString { get; }

	/// <summary>
	/// Creates a new database connection.
	/// </summary>
	/// <remarks>
	/// The returned connection is not opened. The caller is responsible for opening
	/// the connection and disposing of it when done.
	/// </remarks>
	/// <returns>A new <see cref="DbConnection"/> instance configured with the connection string.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the provider is not registered or the connection cannot be created.
	/// </exception>
	DbConnection CreateConnection();

	/// <summary>
	/// Creates a new database connection and opens it asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.
	/// The task result contains an open <see cref="DbConnection"/>.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the provider is not registered or the connection cannot be created.
	/// </exception>
	Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new database connection and opens it synchronously.
	/// </summary>
	/// <returns>An open <see cref="DbConnection"/>.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the provider is not registered or the connection cannot be created.
	/// </exception>
	DbConnection CreateOpenConnection();
}
