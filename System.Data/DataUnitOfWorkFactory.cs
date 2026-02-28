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
	private readonly IDataDbConnectionScopeFactory _connectionScopeFactory;
	private readonly IDataSqlBuilderFactory _sqlBuilderFactory;
	private readonly IDataSqlMapper _sqlMapper;
	private readonly IDataCommandInterceptor _interceptor;
	private readonly string _providerInvariantName;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataUnitOfWorkFactory"/> class.
	/// </summary>
	/// <param name="connectionScopeFactory">The connection scope factory.</param>
	/// <param name="sqlBuilderFactory">The SQL builder factory.</param>
	/// <param name="sqlMapper">The SQL mapper.</param>
	/// <param name="providerInvariantName">The provider invariant name for determining SQL dialect.</param>
	/// <param name="interceptor">The optional command interceptor for logging and telemetry.
	/// When <see langword="null"/>, <see cref="DataCommandInterceptor.Default"/> is used.</param>
	public DataUnitOfWorkFactory(
		IDataDbConnectionScopeFactory connectionScopeFactory,
		IDataSqlBuilderFactory sqlBuilderFactory,
		IDataSqlMapper sqlMapper,
		string providerInvariantName,
		IDataCommandInterceptor? interceptor = null)
	{
		_connectionScopeFactory = connectionScopeFactory ?? throw new ArgumentNullException(nameof(connectionScopeFactory));
		_sqlBuilderFactory = sqlBuilderFactory ?? throw new ArgumentNullException(nameof(sqlBuilderFactory));
		ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);
		_providerInvariantName = providerInvariantName;
		_sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
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
		IDataSqlBuilder sqlBuilder = _sqlBuilderFactory.Create(_providerInvariantName);

		return new DataUnitOfWork(_connectionScopeFactory, sqlBuilder, _sqlMapper, _interceptor);
	}
}
