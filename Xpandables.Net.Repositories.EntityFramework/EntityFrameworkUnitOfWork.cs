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

using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Entity Framework Core implementation of the Unit of Work pattern.
/// </summary>
/// <remarks>This implementation provides transactional support and repository management using Entity Framework Core.
/// It manages the DbContext lifecycle and ensures that all operations within a unit of work are executed within
/// the same transaction context.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="EntityFrameworkUnitOfWork"/> class.
/// </remarks>
/// <param name="context">The Entity Framework DbContext to use for database operations.</param>
/// <param name="serviceProvider">service provider for dependency injection of repositories.</param>
/// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
public class EntityFrameworkUnitOfWork(DataContext context, IServiceProvider serviceProvider) :
    DisposableAsync, IUnitOfWork
{
    private readonly DataContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ConcurrentDictionary<Type, IRepository> _repositories = [];

    /// <inheritdoc />
    public virtual TRepository GetRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRepository>()
        where TRepository : class, IRepository
    {
        ThrowIfDisposed();

        var repositoryType = typeof(TRepository);

        var repository = _repositories.GetOrAdd(typeof(TRepository), _ =>
        {
            IRepository? service;
            if (repositoryType == typeof(IRepository))
            {
                service = new EntityFrameworkRepository(context);
                return service;
            }

            service = serviceProvider.GetService<TRepository>();
            if (service is not null)
            {
                service.InjectAmbientContext(context);
                return service;
            }

            var constructor = repositoryType.GetConstructor([typeof(DataContext)]);
            if (constructor is not null)
            {
                service = (TRepository)constructor.Invoke([context]);
                return service;
            }

            constructor = repositoryType.GetConstructor(Type.EmptyTypes);
            if (constructor is not null)
            {
                service = (TRepository)constructor.Invoke([]);
                service.InjectAmbientContext(context);
                return service;
            }

            throw new InvalidOperationException(
                $"Unable to create repository of type {repositoryType.Name}" +
                $". Repository must be registered in the service provider, have a constructor that accepts DataContext, " +
                $"or have a parameterless constructor.");
        });

        return (TRepository)repository;
    }

    /// <inheritdoc />
    public virtual async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        return new EntityFrameworkUnitOfWorkTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual IUnitOfWorkTransaction BeginTransaction()
    {
        ThrowIfDisposed();

        var transaction = _context.Database.BeginTransaction();
        return new EntityFrameworkUnitOfWorkTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual async Task<IUnitOfWorkTransaction> UseTransactionAsync(
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(transaction);

        var efTransaction = await _context.Database
            .UseTransactionAsync(transaction, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Failed to use transaction.");

        return new EntityFrameworkUnitOfWorkTransaction(efTransaction);
    }

    /// <inheritdoc />
    public virtual IUnitOfWorkTransaction UseTransaction(DbTransaction transaction)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(transaction);

        var efTransaction = _context.Database.UseTransaction(transaction)
            ?? throw new InvalidOperationException("Failed to use transaction.");

        return new EntityFrameworkUnitOfWorkTransaction(efTransaction);
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("Failed to save changes to the database.", exception);
        }
    }

    /// <inheritdoc />
    public virtual int SaveChanges()
    {
        ThrowIfDisposed();

        try
        {
            return _context.SaveChanges();
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("Failed to save changes to the database.", exception);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// When overridden in derived classes, this method get 
    /// called when the instance will be disposed.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to 
    /// release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// <list type="bulleted">
    /// <see cref="Dispose(bool)"/> executes in two distinct scenarios.
    /// <item>If <paramref name="disposing"/> equals <c>true</c>, 
    /// the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged 
    /// resources can be disposed.</item>
    /// <item>If <paramref name="disposing"/> equals <c>false</c>, 
    /// the method has been called
    /// by the runtime from inside the finalizer and you should 
    /// not reference other objects.
    /// Only unmanaged resources can be disposed.</item></list>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var repository in _repositories.Values)
            {
                repository?.Dispose();
            }

            _repositories.Clear();
            _context?.Dispose();
        }

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
            return;

        if (IsDisposed)
            return;

        foreach (var repository in _repositories.Values)
        {
            await repository.DisposeAsync().ConfigureAwait(false);
        }

        _repositories.Clear();

        if (_context != null)
        {
            await _context.DisposeAsync().ConfigureAwait(false);
        }

        IsDisposed = true;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the unit of work has been disposed.
    /// </summary>
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, context);
}

/// <summary>
/// Generic Entity Framework Core implementation of the Unit of Work pattern with typed context.
/// </summary>
/// <typeparam name="TDataContext">The type of the DbContext.</typeparam>
/// <remarks>This implementation provides the same functionality as EntityFrameworkUnitOfWork but with
/// strong typing for the specific DbContext type.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="EntityFrameworkUnitOfWork{TDataContext}"/> class.
/// </remarks>
/// <param name="context">The strongly-typed Entity Framework DbContext.</param>
/// <param name="serviceProvider">Optional service provider for dependency injection of repositories.</param>
/// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
public class EntityFrameworkUnitOfWork<TDataContext>(TDataContext context, IServiceProvider serviceProvider) :
    EntityFrameworkUnitOfWork(context, serviceProvider), IUnitOfWork<TDataContext>
    where TDataContext : DataContext
{
    /// <inheritdoc />
    public override TRepository GetRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRepository>() =>
        base.GetRepository<TRepository>();
}