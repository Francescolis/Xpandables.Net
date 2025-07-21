using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

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
            InjectAmbientContext(service);

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

    /// <summary>
    /// Injects the ambient DataContext into the resolved repository.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <param name="repository">The repository instance to inject the context into.</param>
    /// <exception cref="InvalidOperationException">Thrown when the repository doesn't have a DataContext property or field, 
    /// or when injection fails.</exception>
    protected void InjectAmbientContext<TRepository>(TRepository repository)
         where TRepository : class, IRepository
    {
        var repositoryType = repository.GetType();

        // Try to find a writable property of DataContext type
        var contextProperty = FindDataContextProperty(repositoryType);
        if (contextProperty != null)
        {
            try
            {
                contextProperty.SetValue(repository, Context);
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to inject DataContext into property '{contextProperty.Name}' of repository type '{repositoryType.Name}'. " +
                    $"Ensure the property has a public setter and is compatible with the ambient DataContext type.", ex);
            }
        }

        // Try to find a field of DataContext type
        var contextField = FindDataContextField(repositoryType);
        if (contextField != null)
        {
            try
            {
                contextField.SetValue(repository, Context);
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to inject DataContext into field '{contextField.Name}' of repository type '{repositoryType.Name}'. " +
                    $"Ensure the field is accessible and compatible with the ambient DataContext type.", ex);
            }
        }

        // If neither property nor field found, throw exception
        throw new InvalidOperationException(
            $"Repository type '{repositoryType.Name}' does not contain a writable property or accessible field of type '{typeof(DataContext).Name}' " +
            $"or its derived types. The repository must have a way to receive the ambient DataContext.");
    }

    private static PropertyInfo? FindDataContextProperty(Type repositoryType) =>
        // Look for properties that are assignable from DataContext and are writable
        repositoryType
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(prop =>
                typeof(DataContext).IsAssignableFrom(prop.PropertyType) &&
                prop.CanWrite);
    private static FieldInfo? FindDataContextField(Type repositoryType) =>
        // Look for fields that are assignable from DataContext
        repositoryType
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(field =>
                typeof(DataContext).IsAssignableFrom(field.FieldType) &&
                !field.IsInitOnly);
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