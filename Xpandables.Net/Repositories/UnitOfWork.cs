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
/// <remarks>The implementation uses a proxy that allows use of
/// <see langword="using"/> statement.
/// <code>
/// IUnitOfWork unitOfWork = new UnitOfWork();
/// await (using IRepository repository = unitOfWork.GetRepository())
/// {
///     await repository.InsertAsync(entities, ct);
/// } 
/// // SaveChangesAsync is called automatically if no exception is thrown,
/// // and the methods InsertAsync, UpdateAsync, DeleteAsync are called.
/// </code>
/// </remarks>
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
    public TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository
    {
        TRepository repository = (TRepository)GetRepositoryCore(typeof(TRepository));
        return RepositoryProxy<TRepository>.CreateProxy(this, repository);
    }

    /// <summary>  
    /// When overridden in a derived class, gets the real instance of the repository.  
    /// </summary>
    /// <param name="repositoryType">The type of the repository.</param>
    /// <returns>The repository instance.</returns>  
    protected abstract IRepository GetRepositoryCore(Type repositoryType);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, 
    /// or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public abstract ValueTask DisposeAsync();
}

internal class RepositoryProxy<TRepository> : DispatchProxy
    where TRepository : class, IRepository
{
    private static readonly MethodBase _methodBaseType =
        typeof(object).GetMethod("GetType")!;
    private IUnitOfWork _unitOfWork = default!;
    private TRepository _repositoryInstance = default!;
    private Exception? _exception;
    private bool _writeOperation;
    private bool _disposed;

    // Method to create an instance of the proxy, with both IUnitOfWork and concrete IRepository
    public static TRepository CreateProxy(
        IUnitOfWork unitOfWork,
        TRepository repositoryInstance)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(repositoryInstance);

        object proxy = Create<TRepository, RepositoryProxy<TRepository>>();

        ((RepositoryProxy<TRepository>)proxy).SetParameters(unitOfWork, repositoryInstance);

        return (TRepository)proxy;
    }

    internal void SetParameters(
        IUnitOfWork unitOfWork,
        TRepository repositoryInstance)
    {
        _unitOfWork = unitOfWork;
        _repositoryInstance = repositoryInstance;
    }

    // Overriding the Invoke method to handle method calls dynamically
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        // Forward the method calls to the provided IRepository instance
        try
        {
            return ReferenceEquals(targetMethod, _methodBaseType)
                ? Bypass(targetMethod, args)
                : DoInvoke(targetMethod, args);
        }
        catch (Exception exception)
        {
            _exception = exception;
            throw;
        }
    }

    private object? DoInvoke(MethodInfo targetMethod, object?[]? args)
    {
        if (targetMethod.Name == nameof(DisposeAsync))
        {
            return DisposeAsync();
        }

        if (targetMethod.Name is (nameof(IRepository.InsertAsync)) or
            (nameof(IRepository.UpdateAsync)) or
            (nameof(IRepository.DeleteAsync)))
        {
            _writeOperation = true;
        }

        return targetMethod.Invoke(_repositoryInstance, args);
    }

    // DisposeAsync implementation to ensure SaveChangesAsync is called when disposing
    public async ValueTask DisposeAsync()
    {
        if (_disposed || _exception is not null)
        {
            return;
        }

        if (_writeOperation)
        {
            _ = await _unitOfWork
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        _disposed = true;
    }

    [DebuggerStepThrough]
    private object? Bypass(
        MethodInfo targetMethod,
        object?[]? args) => targetMethod.Invoke(_repositoryInstance, args);
}