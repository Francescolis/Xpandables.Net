using System.Collections.Concurrent;
using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be
/// performed on a data context.
/// </summary>
public class UnitOfWork(DataContext context, IServiceProvider serviceProvider) : AsyncDisposable, IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, IRepository> _repositories = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected DataContext Context { get; } = context;

    /// <inheritdoc />
    public bool IsTransactional { get; set; }

    /// <inheritdoc />
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (IsTransactional)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        var _transaction = await Context.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        return new UnitOfWorkTransaction(_transaction, this);
    }

    /// <inheritdoc />
    public async Task<IUnitOfWorkTransaction> UseTransactionAsync(
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (IsTransactional)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        var _transaction = await Context.Database
            .UseTransactionAsync(transaction, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Unable to use the specified transaction");

        return new UnitOfWorkTransaction(_transaction, this);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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
    public TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository
    {
        var repository = _repositories.GetOrAdd(typeof(TRepository), _ =>
        {
            var service = _serviceProvider.GetService<TRepository>()
                ?? throw new InvalidOperationException(
                    $"The repository of type {typeof(TRepository).Name} is not registered.");

            // Inject the ambient DataContext into the repository
            DataContextExtensions.InjectAmbientContext(service, Context);

            return service;
        });

        return (TRepository)repository;
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await Context.DisposeAsync().ConfigureAwait(false);
            foreach (var repository in _repositories.Values)
            {
                await repository.DisposeAsync().ConfigureAwait(false);
            }
            _repositories.Clear();
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
    UnitOfWork(context, serviceProvider)
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected new TDataContext Context { get; } = context;
}

/// <summary>
/// Represents a unit of work for handling events within a specified data context.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context used by this unit of work. 
/// Must inherit from <see cref="DataContext"/>.</typeparam>
/// <param name="context">The data context to be used for this unit of work.</param>
/// <param name="serviceProvider">The service provider to resolve dependencies.</param>
public sealed class EventUnitOfWork<TDataContext>(
    TDataContext context,
    IServiceProvider serviceProvider) : UnitOfWork<TDataContext>(context, serviceProvider)
    where TDataContext : DataContext
{
}

internal sealed class UnitOfWorkTransaction : AsyncDisposable, IUnitOfWorkTransaction
{
    private readonly UnitOfWork _unitOfWork;
    private IDbContextTransaction? _transaction;
    private bool _committed;
    private bool _rollback;
    private bool _exception;

    public UnitOfWorkTransaction(IDbContextTransaction transaction, UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _transaction = transaction;
        _unitOfWork.IsTransactional = true;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }
        try
        {
            await _transaction
                .CommitAsync(cancellationToken)
                .ConfigureAwait(false);

            _committed = true;
        }
        catch
        {
            _exception = true;
            throw;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction
                .RollbackAsync(cancellationToken)
                .ConfigureAwait(false);

            _rollback = true;
        }
        catch
        {
            _exception = true;
            throw;
        }
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            if (_transaction is null)
            {
                return;
            }
            try
            {
                if (_exception && !_rollback)
                {
                    await RollbackTransactionAsync().ConfigureAwait(false);
                }
                else if (!_exception && !_committed)
                {
                    try
                    {
                        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                        await CommitTransactionAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        await RollbackTransactionAsync().ConfigureAwait(false);
                        throw;
                    }
                }
            }
            finally
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
                _unitOfWork.IsTransactional = false;
            }
        }

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}