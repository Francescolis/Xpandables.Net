using System.Net.Optionals;
using System.Net.Repositories;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Repositories;
namespace Xpandables.Net.UnitTests.Repositories;

[Collection("sqlite-shared-db")]
public sealed class RepositoryEfCoreTests(SqliteTestFixture fx)
{
    private readonly SqliteTestFixture _fx = fx;

    [Fact]
    public async Task Add_Fetch_Update_Delete_without_uow_should_persist_immediately()
    {
        await using var scope = _fx.ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
        repo.IsUnitOfWorkEnabled = false;

        // ensure state: person with KeyId = 1 does not already exist
        var preCount = await repo
            .FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 1).Select(_ => 1))
            .CountAsync();
        if (preCount > 0)
        {
            await repo.DeleteAsync<TestPerson>(q => q.Where(x => x.KeyId == 1));
        }

        var p = new TestPerson { KeyId = 1, Name = "Ann", Age = 20 };
        await repo.AddAsync(default, p);

        // fetch using projection
        var results = repo.FetchAsync<TestPerson, string>(q => q.Where(x => x.KeyId == 1).Select(x => x.Name));
        var names = await results.ToListAsync();
        names.Should().ContainSingle().Which.Should().Be("Ann");

        // update with action
        await repo.UpdateAsync<TestPerson>(q => q.Where(x => x.KeyId == 1), e => e.Age = 21);

        // verify updated
        var ages = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 1).Select(x => x.Age)).ToListAsync();
        ages.Should().ContainSingle().Which.Should().Be(21);

        // update with expression
        await repo.UpdateAsync<TestPerson>(q => q.Where(x => x.KeyId == 1), x => new TestPerson { KeyId = x.KeyId, Name = x.Name, Age = 22, Status = x.Status, CreatedOn = x.CreatedOn, UpdatedOn = DateTime.UtcNow, DeletedOn = x.DeletedOn });
        var ages2 = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 1).Select(x => x.Age)).ToListAsync();
        ages2.Should().ContainSingle().Which.Should().Be(22);

        // bulk update with updater
        var updater = EntityUpdater.SetProperty((TestPerson e) => e.Age, 23);
        await repo.UpdateAsync(q => q.Where(x => x.KeyId == 1), updater);
        var ages3 = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 1).Select(x => x.Age)).ToListAsync();
        ages3.Should().ContainSingle().Which.Should().Be(23);

        // delete
        await repo.DeleteAsync<TestPerson>(q => q.Where(x => x.KeyId == 1));
        var count = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 1).Select(x => 1)).CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Add_Update_Delete_with_uow_should_defer_until_SaveChanges()
    {
        await using var scope = _fx.ServiceProvider.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<IRepository>();
        repo.IsUnitOfWorkEnabled = true;

        await repo.AddAsync(default, new TestPerson { KeyId = 2, Name = "Bob", Age = 30 });
        // not yet persisted
        var existsBefore = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 2).Select(x => 1)).CountAsync();
        existsBefore.Should().Be(0);

        await uow.SaveChangesAsync();
        var existsAfter = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 2).Select(x => 1)).CountAsync();
        existsAfter.Should().Be(1);

        await repo.UpdateAsync<TestPerson>(q => q.Where(x => x.KeyId == 2), e => e.Age = 31);
        await uow.SaveChangesAsync();
        var age = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 2).Select(x => x.Age)).SingleAsync();
        age.Should().Be(31);

        await repo.DeleteAsync<TestPerson>(q => q.Where(x => x.KeyId == 2));
        await uow.SaveChangesAsync();
        var existsAfterDelete = await repo.FetchAsync<TestPerson, int>(q => q.Where(x => x.KeyId == 2).Select(x => 1)).CountAsync();
        existsAfterDelete.Should().Be(0);
    }
}
