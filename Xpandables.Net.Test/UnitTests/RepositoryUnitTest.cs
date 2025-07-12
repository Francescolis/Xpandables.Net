using System.Linq.Expressions;

using FluentAssertions;

using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Test.UnitTests;

public sealed class RepositoryUnitTest
{
    private readonly InMemoryRepository _repository;
    public RepositoryUnitTest()
    {
        _repository = new InMemoryRepository();

        // Arrange
        _repository.InsertAsync(new List<TestEntity>
        {
            new() { KeyId = 1, Name = "Test1" },
            new() { KeyId = 2, Name = "Test2" }
        }).Wait();
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnEntities()
    {
        // Arrange
        var filter = new EntityFilter<TestEntity, TestEntity>
        {
            Where = e => e.KeyId > 0,
            Selector = e => e
        };

        // Act
        var result = await _repository.FetchAsync(filter).ToListAsync();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
            {
                new() { KeyId = 1,  Name = "Test1" },
                new() { KeyId = 2, Name = "Test2" }
            };

        // Act
        await _repository.InsertAsync(entities);

        // Assert
        var filter = new EntityFilter<TestEntity>
        {
            Where = e => e.KeyId > 0,
            Selector = e => e
        };
        var result = await _repository.FetchAsync(filter).ToListAsync();
        result.Should().Contain(entities);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntities()
    {
        // Arrange
        var filter = new EntityFilter<TestEntity>
        {
            Where = e => e.KeyId == 1
        };

        Expression<Func<TestEntity, TestEntity>> updateExpression =
            e => new TestEntity { KeyId = e.KeyId, Name = "Updated" };

        // Act
        await _repository.UpdateAsync(filter, updateExpression);

        // Assert
        var result = await _repository.FetchAsync(filter).ToListAsync();
        result.First(e => e.KeyId == 1).Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntities()
    {
        // Arrange
        var filter = new EntityFilter<TestEntity>
        {
            Where = e => e.KeyId == 1
        };

        // Act
        await _repository.DeleteAsync(filter);

        // Assert
        var result = await _repository.FetchAsync(filter).ToListAsync();
        result.Should().NotContain(e => e.KeyId == 1);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var _unitOfWork = new InMemoryUnitOfWork();

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void GetRepository_ShouldReturnRepository()
    {
        // Arrange
        var _unitOfWork = new InMemoryUnitOfWork();

        // Act
        var repository = _unitOfWork.GetRepository<IRepository>();

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task UsingStatement_ShouldCallSaveChangesAsyncOnDispose()
    {
        // Arrange
        var unitOfWork = new InMemoryUnitOfWork();

        // Act
        await using (var repository = unitOfWork.GetRepository<IRepository>())
        {
            await repository.InsertAsync(new List<TestEntity>
            { new() { KeyId = 1,  Name = "Test" } });
        }

        // Assert
        // No need to call SaveChangesAsync explicitly, it should be called automatically
        var result = await unitOfWork.GetChangesCountAsync();
        result.Should().BePositive();
    }
}

// Assuming InMemoryUnitOfWork is a real implementation of UnitOfWork
public class InMemoryUnitOfWork : UnitOfWorkCore
{
    private int _changesCount = 0;

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // Simulate saving changes
        _changesCount++;
        return Task.FromResult(_changesCount);
    }

    protected override IRepository GetRepositoryCore(Type repositoryType) =>
        // Simulate getting a repository
        new InMemoryRepository();

    public Task<int> GetChangesCountAsync() => Task.FromResult(_changesCount);
}

public class InMemoryRepository : IRepository
{
    private readonly List<IEntity> _entities = [];

    public IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        IEntityFilter<TEntity, TResult> entityFilter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        var query = _entities.OfType<TEntity>().AsQueryable();
        var filteredQuery = entityFilter.Apply(query);
        return filteredQuery.OfType<TResult>().ToAsyncEnumerable();
    }

    public Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        _entities.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync<TEntity>(
        IEntityFilter<TEntity> entityFilter,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        var query = _entities.OfType<TEntity>().AsQueryable();
        var filteredEntities = entityFilter.Apply(query).OfType<TEntity>().ToList();

        foreach (var entity in filteredEntities)
        {
            var updatedEntity = updateExpression.Compile().Invoke(entity);
            _entities.Remove(entity);
            _entities.Add(updatedEntity);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync<TEntity>(
        IEntityFilter<TEntity> entityFilter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        var query = _entities.OfType<TEntity>().AsQueryable();
        var filteredEntities = entityFilter.Apply(query).OfType<TEntity>().ToList();

        foreach (var entity in filteredEntities)
        {
            _entities.Remove(entity);
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // Simulate dispose
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

// Assuming TestEntity is a real implementation of IEntity
public class TestEntity : Entity<int>
{
    public required string Name { get; init; }
}