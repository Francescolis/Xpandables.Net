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
using System.Data.Common;

namespace System.Data;

/// <summary>
/// Provides a default implementation of <see cref="IDataConnectionScopeFactory"/> that creates 
/// <see cref="DataConnectionScope"/> instances using an <see cref="IDataConnectionFactory"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CreateScopeAsync"/> opens the connection eagerly for backward compatibility.
/// <see cref="CreateScope"/> creates a scope with a <strong>closed</strong> connection that
/// opens lazily on first use via <see cref="IDataConnectionScope.EnsureOpenAsync"/>,
/// avoiding synchronous thread-pool blocking during connection establishment.
/// </para>
/// </remarks>
/// <param name="connectionFactory">The factory used to create database connections.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionFactory"/> is null.</exception>
public sealed class DataConnectionScopeFactory(IDataConnectionFactory connectionFactory) : IDataConnectionScopeFactory
{
	private readonly IDataConnectionFactory _connectionFactory = connectionFactory
		?? throw new ArgumentNullException(nameof(connectionFactory));

	/// <inheritdoc />
	public async Task<IDataConnectionScope> CreateScopeAsync(CancellationToken cancellationToken = default)
	{
		DbConnection connection = await _connectionFactory
			.CreateConnectionAsync(cancellationToken)
			.ConfigureAwait(false);

		return new DataConnectionScope(connection);
	}

	/// <inheritdoc/>
	public async Task<IDataConnectionScope> CreateOpenScopeAsync(CancellationToken cancellationToken = default)
	{
		DbConnection connection = await _connectionFactory
			.CreateOpenConnectionAsync(cancellationToken)
			.ConfigureAwait(false);

		return new DataConnectionScope(connection);
	}

	/// <inheritdoc />
	/// <remarks>
	/// Creates a scope with a closed connection. The connection will be opened
	/// asynchronously on first use via <see cref="IDataConnectionScope.EnsureOpenAsync"/>.
	/// This avoids blocking a thread-pool thread during connection establishment.
	/// </remarks>
	public IDataConnectionScope CreateScope()
	{
		DbConnection connection = _connectionFactory.CreateConnection();
		return new DataConnectionScope(connection);
	}

	/// <inheritdoc />
	public IDataConnectionScope CreateOpenScope()
	{
		DbConnection connection = _connectionFactory.CreateOpenConnection();
		return new DataConnectionScope(connection);
	}
}
