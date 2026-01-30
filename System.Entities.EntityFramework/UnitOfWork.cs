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

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace System.Entities.EntityFramework;

/// <summary>
/// Entity Framework Core implementation of the Unit of Work pattern.
/// </summary>
/// <remarks>This implementation provides transactional support and repository management using Entity Framework Core.
/// It manages the DbContext lifecycle and ensures that all operations within a unit of work are executed within
/// the same transaction context.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="UnitOfWork{TDataContext}"/> class.
/// </remarks>
/// <param name="context">The Entity Framework DbContext to use for database operations.</param>
/// <param name="serviceProvider">service provider for dependency injection of repositories.</param>
/// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
public class UnitOfWork<TContext>(TContext context, IServiceProvider serviceProvider) : IUnitOfWork<TContext>
    where TContext : DataContext
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>")]
    private readonly TContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    /// <remarks>Use this property to determine if the object is no longer usable due to disposal. Accessing
    /// members after disposal may result in exceptions or undefined behavior.</remarks>
    protected bool IsDisposed { get; set; }

    /// <inheritdoc />
    public virtual TRepository GetRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRepository>()
        where TRepository : class, IRepository
    {
        ThrowIfDisposed();

        var repositoryType = typeof(TRepository);

        try
        {
            if (repositoryType.IsInterface)
            {
                var service = ActivatorUtilities.GetServiceOrCreateInstance<TRepository>(serviceProvider);
                service.InjectAmbientContext(_context);
                return service;
            }
            else
            {
                var instance = ActivatorUtilities.CreateInstance<TRepository>(serviceProvider, _context);
                return instance;
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Unable to create repository of type {repositoryType.FullName}. " +
                $"Repository must have a constructor that accepts {typeof(TContext).Name} (or derived type) " +
                $"as a parameter, or have a parameterless constructor with InjectAmbientContext support.",
                ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        return new UnitOfWorkTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual IUnitOfWorkTransaction BeginTransaction()
    {
        ThrowIfDisposed();

        var transaction = _context.Database.BeginTransaction();
        return new UnitOfWorkTransaction(transaction);
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

        return new UnitOfWorkTransaction(efTransaction);
    }

    /// <inheritdoc />
    public virtual IUnitOfWorkTransaction UseTransaction(DbTransaction transaction)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(transaction);

        var efTransaction = _context.Database.UseTransaction(transaction)
            ?? throw new InvalidOperationException("Failed to use transaction.");

        return new UnitOfWorkTransaction(efTransaction);
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        IExecutionStrategy strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Use a transaction if one is not already present
            if (_context.Database.CurrentTransaction == null)
            {
                using var transaction = await _context.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    return result;
                }
                catch (DbUpdateException exception)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    throw new InvalidOperationException("Failed to save changes to the database.", exception);
                }
            }
            else
            {
                try
                {
                    return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (DbUpdateException exception)
                {
                    // An ambient transaction is present, let it handle the rollback
                    throw new InvalidOperationException("Failed to save changes to the database.", exception);
                }
            }
        }).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual int SaveChanges()
    {
        ThrowIfDisposed();

        IExecutionStrategy strategy = _context.Database.CreateExecutionStrategy();

        return strategy.Execute(() =>
        {
            // Use a transaction if one is not already present
            if (_context.Database.CurrentTransaction == null)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    var result = _context.SaveChanges();
                    transaction.Commit();
                    return result;
                }
                catch (DbUpdateException exception)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Failed to save changes to the database.", exception);
                }
            }
            else
            {
                try
                {
                    return _context.SaveChanges();
                }
                catch (DbUpdateException exception)
                {
                    // An ambient transaction is present, let it handle the rollback
                    throw new InvalidOperationException("Failed to save changes to the database.", exception);
                }
            }
        });
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
            // Note: We don't dispose the DbContext here as it should be managed by the dependency injection container
        }

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases the unmanaged resources used by the object and, optionally, releases the managed
    /// resources.
    /// </summary>
    /// <remarks>This method is intended to be called by derived classes to implement asynchronous resource
    /// cleanup. If disposing is true, both managed and unmanaged resources are released; otherwise, only unmanaged
    /// resources are released. Multiple calls to this method have no effect after the object has been
    /// disposed.</remarks>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
            return;

        if (IsDisposed)
            return;

        if (_context != null)
        {
            // Note: We don't dispose the DbContext here as it should be managed by the dependency injection container
        }

        IsDisposed = true;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the unit of work has been disposed.
    /// </summary>
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, context);
}
