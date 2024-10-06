/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Diagnostics;
using System.Reflection;

namespace Xpandables.Net.Repositories;
/// <summary>
/// Represents a unit of work that encapsulates a series of operations to be 
/// executed as a single transaction.
/// </summary>
public abstract class UnitOfWork : IUnitOfWork
{
    /// <summary>
    /// When overridden in a derived class, saves all changes made in this unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="InvalidOperationException">All exceptions
    /// related to the operation.</exception>
    public abstract Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public IRepository GetRepository<TRepository>()
        where TRepository : class, IRepository
    {
        TRepository repository = (TRepository)GetRepository(typeof(TRepository));
        return ProxyRepositoryDisposable<TRepository>.Create(repository, this);
    }

    /// <inheritdoc/>
    public IRepositoryRead GetRepositoryRead<TRepository>()
        where TRepository : class, IRepositoryRead
    {
        TRepository repository = (TRepository)GetRepositoryRead(typeof(TRepository));
        return ProxyRepositoryDisposable<TRepository>.Create(repository, this);
    }

    /// <inheritdoc/>
    public IRepositoryWrite GetRepositoryWrite<TRepository>()
        where TRepository : class, IRepositoryWrite
    {
        TRepository repository = (TRepository)GetRepositoryWrite(typeof(TRepository));
        return ProxyRepositoryDisposable<TRepository>.Create(repository, this);
    }

    /// <summary>  
    /// When overridden in a derived class, gets the repository of the specified type.  
    /// </summary>  
    /// <param name="repositoryType">The type of the repository to get.</param>  
    /// <returns>The repository instance of the specified type.</returns>  
    protected abstract IRepository GetRepository(Type repositoryType);

    /// <summary>  
    /// When overridden in a derived class, gets the read-only repository of the specified type.  
    /// </summary>  
    /// <param name="repositoryType">The type of the repository to get.</param>  
    /// <returns>The read-only repository instance of the specified type.</returns>  
    protected abstract IRepositoryRead GetRepositoryRead(Type repositoryType);

    /// <summary>  
    /// When overridden in a derived class, gets the write repository of the specified type.  
    /// </summary>  
    /// <param name="repositoryType">The type of the repository to get.</param>  
    /// <returns>The write repository instance of the specified type.</returns>  
    protected abstract IRepositoryWrite GetRepositoryWrite(Type repositoryType);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, 
    /// or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public abstract ValueTask DisposeAsync();
}


internal sealed class ProxyRepositoryDisposable<TRepository> : DispatchProxy, IAsyncDisposable
    where TRepository : class
{
    private static readonly MethodBase _methodBaseType = typeof(object).GetMethod("GetType")!;
    private TRepository? _instance;
    private IUnitOfWork? _unitOfWork;
    private Exception? _exception;
    private bool _disposed;
    private ProxyRepositoryDisposable() { }
    public static TRepository Create(TRepository instance, IUnitOfWork unitOfWork)
    {
        TRepository proxy = Create<TRepository, ProxyRepositoryDisposable<TRepository>>();

        ((ProxyRepositoryDisposable<TRepository>)(object)proxy)
            .SetParameters(instance, unitOfWork);

        return proxy;
    }

    internal void SetParameters(TRepository instance, IUnitOfWork unitOfWork)
    {
        _instance = instance;
        _unitOfWork = unitOfWork;
    }
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        try
        {
            return ReferenceEquals(targetMethod, _methodBaseType)
                ? Bypass(targetMethod, args)
                : targetMethod.Invoke(_instance, args);
        }
        catch (Exception exception)
        {
            _exception = exception;
            throw;
        }
    }

    [DebuggerStepThrough]
    private object? Bypass(
        MethodInfo targetMethod,
        object?[]? args) => targetMethod.Invoke(_instance, args);

    public async ValueTask DisposeAsync()
    {
        if (_disposed || _exception is not null)
        {
            return;
        }

        try
        {
            if (_unitOfWork is not null)
            {
                _ = await _unitOfWork
                    .SaveChangesAsync()
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            _disposed = true;
            _instance = default;
        }
    }
}
