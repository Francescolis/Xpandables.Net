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
    private readonly IDbConnectionScope _connectionScope;
    private readonly IDataSqlBuilder _sqlBuilder;
    private readonly IDataSqlMapper _sqlMapper;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataUnitOfWork"/> class.
    /// </summary>
    /// <param name="connectionScope">The connection scope to use.</param>
    /// <param name="sqlBuilder">The SQL builder for generating queries.</param>
    /// <param name="sqlMapper">The SQL mapper for mapping data.</param>
    public DataUnitOfWork(IDbConnectionScope connectionScope, IDataSqlBuilder sqlBuilder, IDataSqlMapper sqlMapper)
    {
        _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
        _sqlBuilder = sqlBuilder ?? throw new ArgumentNullException(nameof(sqlBuilder));
        _sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
    }

    /// <inheritdoc />
    public IDbConnectionScope ConnectionScope
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

        return (IDataRepository<TEntity>)_repositories.GetOrAdd(
            typeof(TEntity),
            _ => CreateRepository<TEntity>());
    }

    /// <summary>
    /// Creates a new repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A new repository instance.</returns>
    protected virtual IDataRepository<TEntity> CreateRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        return new DataRepository<TEntity>(_connectionScope, _sqlBuilder, _sqlMapper);
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

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // Dispose all cached repositories
        foreach (var repo in _repositories.Values)
        {
            if (repo is IDisposable disposable)
                disposable.Dispose();
        }

        _repositories.Clear();
        _connectionScope.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // Dispose all cached repositories
        foreach (var repo in _repositories.Values)
        {
            if (repo is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else if (repo is IDisposable disposable)
                disposable.Dispose();
        }

        _repositories.Clear();
        await _connectionScope.DisposeAsync().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Provides a factory for creating <see cref="DataUnitOfWork"/> instances.
/// </summary>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public class DataUnitOfWorkFactory : IDataUnitOfWorkFactory
{
    private readonly IDbConnectionScopeFactory _connectionScopeFactory;
    private readonly IDataSqlBuilderFactory _sqlBuilderFactory;
    private readonly IDataSqlMapper _sqlMapper;
    private readonly string _providerInvariantName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataUnitOfWorkFactory"/> class.
    /// </summary>
    /// <param name="connectionScopeFactory">The connection scope factory.</param>
    /// <param name="sqlBuilderFactory">The SQL builder factory.</param>
    /// <param name="sqlMapper">The SQL mapper.</param>
    /// <param name="providerInvariantName">The provider invariant name for determining SQL dialect.</param>
    public DataUnitOfWorkFactory(
        IDbConnectionScopeFactory connectionScopeFactory,
        IDataSqlBuilderFactory sqlBuilderFactory,
        IDataSqlMapper sqlMapper,
        string providerInvariantName)
    {
        _connectionScopeFactory = connectionScopeFactory ?? throw new ArgumentNullException(nameof(connectionScopeFactory));
        _sqlBuilderFactory = sqlBuilderFactory ?? throw new ArgumentNullException(nameof(sqlBuilderFactory));
        ArgumentException.ThrowIfNullOrWhiteSpace(providerInvariantName);
        _providerInvariantName = providerInvariantName;
        _sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
    }

    /// <inheritdoc />
    public async Task<IDataUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
    {
        var connectionScope = await _connectionScopeFactory
            .CreateScopeAsync(cancellationToken)
            .ConfigureAwait(false);

        var sqlBuilder = _sqlBuilderFactory.Create(_providerInvariantName);

        return new DataUnitOfWork(connectionScope, sqlBuilder, _sqlMapper);
    }

    /// <inheritdoc />
    public IDataUnitOfWork Create()
    {
        var connectionScope = _connectionScopeFactory.CreateScope();
        var sqlBuilder = _sqlBuilderFactory.Create(_providerInvariantName);

        return new DataUnitOfWork(connectionScope, sqlBuilder, _sqlMapper);
    }
}
