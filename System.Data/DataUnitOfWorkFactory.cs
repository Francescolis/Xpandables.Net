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

namespace System.Data;

/// <summary>
/// Provides a factory for creating <see cref="DataUnitOfWork"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Each call to <see cref="Create"/> or <see cref="CreateAsync"/> produces a new
/// <see cref="DataUnitOfWork"/> that owns its own connection scope. The caller is
/// responsible for disposing the returned unit of work.
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public class DataUnitOfWorkFactory : IDataUnitOfWorkFactory
{
	private readonly IDataConnectionScopeFactory _connectionScopeFactory;
	private readonly IDataSqlServiceAccessorFactory _sqlServiceAccessorFactory;
	private readonly IDataCommandInterceptor _interceptor;
	private readonly string _providerInvariantName;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataUnitOfWorkFactory"/> class.
	/// </summary>
	/// <param name="connectionScopeFactory">The connection scope factory.</param>
	/// <param name="sqlServiceAccessorFactory">The SQL service accessor factory.</param>
	/// <param name="providerInvariantName">The provider invariant name for determining SQL dialect.</param>
	/// <param name="interceptor">The optional command interceptor for logging and telemetry.
	/// When <see langword="null"/>, <see cref="DataCommandInterceptor.Default"/> is used.</param>
	public DataUnitOfWorkFactory(
		IDataConnectionScopeFactory connectionScopeFactory,
		IDataSqlServiceAccessorFactory sqlServiceAccessorFactory,
		string providerInvariantName,
		IDataCommandInterceptor? interceptor = null)
	{
		_connectionScopeFactory = connectionScopeFactory ?? throw new ArgumentNullException(nameof(connectionScopeFactory));
		_sqlServiceAccessorFactory = sqlServiceAccessorFactory ?? throw new ArgumentNullException(nameof(sqlServiceAccessorFactory));
		ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);
		_providerInvariantName = providerInvariantName;
		_interceptor = interceptor ?? DataCommandInterceptor.Default;
	}

	/// <inheritdoc />
	public Task<IDataUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
	{
		// The DataUnitOfWork constructor creates its own connection scope from the factory.
		// Connection pooling makes the synchronous open effectively non-blocking.
		return Task.FromResult(Create());
	}

	/// <inheritdoc />
	public IDataUnitOfWork Create()
	{
		IDataSqlServiceAccessor sqlServiceAccessor = _sqlServiceAccessorFactory.Create(_providerInvariantName);

		return new DataUnitOfWork(_connectionScopeFactory, sqlServiceAccessor, _interceptor);
	}
}
