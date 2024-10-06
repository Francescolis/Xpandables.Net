using System.Linq.Expressions;

using FluentAssertions;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Test.UnitTests;

public sealed class RepositoryUnitTest
{
    [Fact]
    public async Task UnitOfWork_Should_SaveChanges_OnExit()
    {
        UnitOfWorkTest unitOfWork = new();
        await using (IRepository repository = unitOfWork.GetRepository())
        {
            await repository.InsertAsync(new[] { new EntityTest() }, default);
        }

        unitOfWork.Result.Should().Be(1);
    }
}

internal sealed class EntityTest : IEntity<Guid>
{
    public Guid Id { get; set; }
}

internal sealed class UnitOfWorkTest : UnitOfWork
{
    public int Result { get; private set; }
    protected override IRepository GetRepositoryCore() =>
        new RepositoryTest();
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        Result = 1;
        return Task.FromResult(Result);
    }
    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
internal sealed class RepositoryTest : IRepository
{
    public Task DeleteAsync<TEntity>(
        IEntityFilter<TEntity> entityFilter,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity =>
        throw new InvalidOperationException();
    public IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        IEntityFilter<TEntity, TResult> entityFilter,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity =>
        AsyncEnumerable.Empty<TResult>();
    public Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity =>
        Task.CompletedTask;
    public Task UpdateAsync<TEntity>(
        IEntityFilter<TEntity> entityFilter,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity =>
        Task.CompletedTask;
}