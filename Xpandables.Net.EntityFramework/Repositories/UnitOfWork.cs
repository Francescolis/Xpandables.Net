namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be 
/// performed on a data context.
/// </summary>
public class UnitOfWork(DataContext context, IServiceProvider serviceProvider) :
    UnitOfWorkCore
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected DataContext Context { get; } = context;

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while saving the changes.",
                exception);
        }
    }

    /// <inheritdoc/>
    protected override IRepository GetRepositoryCore(Type repositoryType) =>
        _serviceProvider.GetService(repositoryType) as IRepository
            ?? throw new InvalidOperationException(
                $"The repository of type {repositoryType.Name} is not registered.");

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await Context.DisposeAsync().ConfigureAwait(false);
        }

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be 
/// performed on a data context of type <typeparamref name="TDataContext"/>.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
public class UnitOfWork<TDataContext>(
    TDataContext context, IServiceProvider serviceProvider) :
    UnitOfWork(context, serviceProvider),
    IUnitOfWork<TDataContext>
    where TDataContext : DataContext
{
}