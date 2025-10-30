using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Entities;

namespace Xpandables.Net.UnitTests.Repositories;

[Collection("sqlite-shared-db")]
public sealed class UnitOfWorkEfCoreTests(SqliteTestFixture fx)
{
    private readonly SqliteTestFixture _fx = fx;

    [Fact]
    public async Task BeginTransaction_commit_should_persist()
    {
        await using var scope = _fx.ServiceProvider.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<IRepository>();
        repo.IsUnitOfWorkEnabled = true;

        var preCount = await repo
                 .FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 100).Select(_ => 1))
                 .CountAsync();
        if (preCount > 0)
        {
            repo.IsUnitOfWorkEnabled = false;
            await repo.DeleteAsync<TestPerson>(q => q.Where(x => x.KeyId == 100));
            repo.IsUnitOfWorkEnabled = true;
        }

        await using var tx = await uow.BeginTransactionAsync();
        await repo.AddAsync(default, new TestPerson { KeyId = 100, Name = "Tx", Age = 1 });
        await uow.SaveChangesAsync();
        tx.CommitTransaction();

        var exists = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 100).Select(x => 1)).CountAsync();
        exists.Should().Be(1);
    }

    [Fact]
    public async Task BeginTransaction_rollback_should_revert()
    {
        await using var scope = _fx.ServiceProvider.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<IRepository>();
        repo.IsUnitOfWorkEnabled = true;

        var preCount = await repo
                 .FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 101).Select(_ => 1))
                 .CountAsync();
        if (preCount > 0)
        {
            repo.IsUnitOfWorkEnabled = false;
            await repo.DeleteAsync<TestPerson>(q => q.Where(x => x.KeyId == 101));
            repo.IsUnitOfWorkEnabled = true;
        }

        await using var tx = await uow.BeginTransactionAsync();
        await repo.AddAsync(default, new TestPerson { KeyId = 101, Name = "Tx2", Age = 1 });
        await uow.SaveChangesAsync();
        await tx.RollbackTransactionAsync();

        var exists = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 101).Select(x => 1)).CountAsync();
        exists.Should().Be(0);
    }

    [Fact]
    public async Task UseExistingTransaction_should_be_supported()
    {
        await using var scope = _fx.ServiceProvider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDataContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<IRepository>();
        repo.IsUnitOfWorkEnabled = true;

        var preCount = await repo
                 .FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 102).Select(_ => 1))
                 .CountAsync();
        if (preCount > 0)
        {
            repo.IsUnitOfWorkEnabled = false;
            await repo.DeleteAsync<TestPerson>(q => q.Where(x => x.KeyId == 102));
            repo.IsUnitOfWorkEnabled = true;
        }

        await using var efTx = await ctx.Database.BeginTransactionAsync();
        await using var uowTx = await uow.UseTransactionAsync(efTx.GetDbTransaction());

        await repo.AddAsync(default, new TestPerson { KeyId = 102, Name = "Tx3", Age = 1 });
        await uow.SaveChangesAsync();
        await uowTx.CommitTransactionAsync();

        var exists = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 102).Select(x => 1)).CountAsync();
        exists.Should().Be(1);
    }
}
