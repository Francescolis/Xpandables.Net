
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work implementation that manages a data context and provides transactional support, repository
/// access, and change tracking for database operations.
/// </summary>
/// <remarks>This class is designed to encapsulate the lifetime of a data context and ensure that all database
/// operations are performed within a consistent transactional boundary. It supports both synchronous and asynchronous
/// operations, including transaction management, saving changes, and accessing repositories.  The <see
/// cref="UnitOfWork"/> is intended to be used in scenarios where a unit of work pattern is required to
/// coordinate changes across multiple repositories or services. It ensures that resources are properly disposed of when
/// the unit of work is no longer needed.</remarks>
/// <param name="context"></param>
/// <param name="serviceProvider"></param>
public class UnitOfWork(DataContext context, IServiceProvider serviceProvider) : UnitOfWorkBase, IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, IRepository> _repositories = [];

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected DataContext Context { get; } = context;

    /// <inheritdoc />
    public override async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var _transaction = await Context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        return new UnitOfWorkTransaction(_transaction.GetDbTransaction());
    }

    /// <inheritdoc />
    public override IUnitOfWorkTransaction BeginTransaction()
    {
        var _transaction = Context.Database.BeginTransaction();

        return new UnitOfWorkTransaction(_transaction.GetDbTransaction());
    }

    /// <inheritdoc />
    public override async Task<IUnitOfWorkTransaction> UseTransactionAsync(
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var _transaction = await Context.Database
            .UseTransactionAsync(transaction, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Unable to use the specified transaction");

        return new UnitOfWorkTransaction(_transaction.GetDbTransaction());
    }

    /// <inheritdoc />
    public override IUnitOfWorkTransaction UseTransaction(DbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var _transaction = Context.Database.UseTransaction(transaction)
            ?? throw new InvalidOperationException("Unable to use the specified transaction");

        return new UnitOfWorkTransaction(_transaction.GetDbTransaction());
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while saving the changes.",
                exception);
        }
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        try
        {
            return Context.SaveChanges();
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while saving the changes.",
                exception);
        }
    }

    /// <inheritdoc />
    public override TRepository GetRepository<TRepository>()
    {
        var repository = _repositories.GetOrAdd(typeof(TRepository), _ =>
        {
            var service = ServiceProvider.GetService<TRepository>()
                ?? throw new InvalidOperationException(
                    $"The store of type {typeof(TRepository).Name} is not registered.");

            // Inject the ambient DataContext into the repository
            DataContextExtensions.InjectAmbientContext(service, Context);

            return service;
        });

        return (TRepository)repository;
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <remarks>This method should be called when the instance is no longer needed to ensure that all
    /// resources  are properly released. If the instance is disposed, it cannot be used again.</remarks>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Context.Dispose();

            _repositories.Clear();

            IsDisposed = true;
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            await Context.DisposeAsync().ConfigureAwait(false);

            foreach (var repository in _repositories.Values)
            {
                await repository.DisposeAsync().ConfigureAwait(false);
            }

            _repositories.Clear();

            IsDisposed = true;
        }

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be
/// performed on a data context of type <typeparamref name="TDataContext" />.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
public class UnitOfWork<TDataContext>(TDataContext context, IServiceProvider serviceProvider) :
    UnitOfWork(context, serviceProvider), IUnitOfWork<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected new TDataContext Context { get; } = context;
}
