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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace System.Data;

/// <summary>
/// Provides an ADO.NET implementation of <see cref="IDataUnitOfWork"/>.
/// </summary>
/// <remarks>
/// <para>
/// This unit of work manages a single database connection scope and provides
/// repository access and transaction management for ADO.NET operations.
/// </para>
/// <para>
/// Repositories created through this unit of work share the same connection
/// and transaction context, ensuring consistency across operations.
/// </para>
/// </remarks>
#pragma warning disable CA1063 // Implement IDisposable correctly - simplified for sealed-like behavior
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public class DataUnitOfWork : IDataUnitOfWork
{
	private readonly IDataDbConnectionScope _connectionScope;
	private readonly IDataSqlBuilder _sqlBuilder;
	private readonly IDataSqlMapper _sqlMapper;
	private readonly IDataCommandInterceptor _interceptor;
	private readonly ConcurrentDictionary<Type, object> _repositories = new();
	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataUnitOfWork"/> class.
	/// The connection scope is created internally from the supplied factory,
	/// making this unit of work the sole owner of the connection lifecycle.
	/// </summary>
	/// <param name="connectionScopeFactory">The factory used to create the connection scope. Must not be null.</param>
	/// <param name="sqlBuilder">The SQL builder for generating queries.</param>
	/// <param name="sqlMapper">The SQL mapper for mapping data.</param>
	/// <param name="interceptor">The optional command interceptor for logging and telemetry.
	/// When <see langword="null"/>, <see cref="DataCommandInterceptor.Default"/> is used.</param>
	public DataUnitOfWork(
		IDataDbConnectionScopeFactory connectionScopeFactory,
		IDataSqlBuilder sqlBuilder,
		IDataSqlMapper sqlMapper,
		IDataCommandInterceptor? interceptor = null)
	{
		ArgumentNullException.ThrowIfNull(connectionScopeFactory);
		_connectionScope = connectionScopeFactory.CreateScope();
		_sqlBuilder = sqlBuilder ?? throw new ArgumentNullException(nameof(sqlBuilder));
		_sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
		_interceptor = interceptor ?? DataCommandInterceptor.Default;
	}

	/// <inheritdoc />
	public IDataDbConnectionScope ConnectionScope
	{
		get
		{
			ThrowIfDisposed();
			return _connectionScope;
		}
	}

	/// <inheritdoc />
	public IDataTransaction? CurrentTransaction
	{
		get
		{
			ThrowIfDisposed();
			return _connectionScope.CurrentTransaction;
		}
	}

	/// <inheritdoc />
	public bool HasActiveTransaction
	{
		get
		{
			ThrowIfDisposed();
			return _connectionScope.HasActiveTransaction;
		}
	}

	/// <inheritdoc />
	public virtual IDataRepository<TEntity> GetRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
		where TEntity : class
	{
		ThrowIfDisposed();

		Type key = typeof(TEntity);

		// Fast path: cached instance is still alive
		if (_repositories.TryGetValue(key, out object? cached) && cached is IDataRepository<TEntity> repo && !IsDisposed(repo))
		{
			return repo;
		}

		// Slow path: evict the stale entry (if any) and create a fresh repository
		IDataRepository<TEntity> fresh = CreateRepository<TEntity>();
		_repositories[key] = fresh;
		return fresh;
	}

	/// <summary>
	/// Creates a new repository instance for the specified entity type.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <returns>A new repository instance.</returns>
	protected virtual IDataRepository<TEntity> CreateRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
		where TEntity : class
	{
		return new DataRepository<TEntity>(_connectionScope, _sqlBuilder, _sqlMapper, _interceptor);
	}

	/// <inheritdoc />
	public virtual async Task<IDataTransaction> BeginTransactionAsync(
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
		CancellationToken cancellationToken = default)
	{
		ThrowIfDisposed();
		return await _connectionScope.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public virtual IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
	{
		ThrowIfDisposed();
		return _connectionScope.BeginTransaction(isolationLevel);
	}

	/// <summary>
	/// Throws if the unit of work has been disposed.
	/// </summary>
	protected void ThrowIfDisposed()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
	}

	/// <summary>
	/// Determines whether the specified repository instance has been disposed.
	/// </summary>
	/// <param name="repository">The repository to check.</param>
	/// <returns><see langword="true"/> if the repository is disposed; otherwise, <see langword="false"/>.</returns>
	private static bool IsDisposed(object repository)
	{
		try
		{
			// Attempt a lightweight operation that throws ObjectDisposedException if disposed.
			// CountAsync/QueryAsync all call ThrowIfDisposed() internally, but we use
			// a direct property access on the connection scope via the repository's interface
			// to avoid side effects.
			if (repository is IDisposableCheck check)
			{
				return check.IsDisposed;
			}

			return false;
		}
		catch (ObjectDisposedException)
		{
			return true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		_isDisposed = true;

		// Dispose all cached repositories
		foreach (object repo in _repositories.Values)
		{
			if (repo is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		_repositories.Clear();
		_connectionScope.Dispose();

		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (_isDisposed)
		{
			return;
		}

		_isDisposed = true;

		// Dispose all cached repositories
		foreach (object repo in _repositories.Values)
		{
			if (repo is IAsyncDisposable asyncDisposable)
			{
				await asyncDisposable.DisposeAsync().ConfigureAwait(false);
			}
			else if (repo is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		_repositories.Clear();
		await _connectionScope.DisposeAsync().ConfigureAwait(false);

		GC.SuppressFinalize(this);
	}
}
