using System.Net.DependencyInjection;
using System.Net.Repositories;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace System.Net.UnitTests.Repositories;

public interface ICustomRepo : IRepository { }
public sealed class CustomRepo(TestDataContext ctx) : EntityFrameworkRepository<TestDataContext>(ctx), ICustomRepo { }

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddXEntityFrameworkRepositories_registers_defaults()
    {
        var sc = new ServiceCollection();
        sc.AddDbContext<TestDataContext>(o => o.UseSqlite("DataSource=:memory:"));
        sc.AddXEntityFrameworkRepositories<TestDataContext>();
        var sp = sc.BuildServiceProvider();

        sp.GetRequiredService<IRepository>().Should().NotBeNull();
        sp.GetRequiredService<IRepository<TestDataContext>>().Should().NotBeNull();
        sp.GetRequiredService<IUnitOfWork>().Should().NotBeNull();
        sp.GetRequiredService<IUnitOfWork<TestDataContext>>().Should().NotBeNull();
    }

    [Fact]
    public void AddXRepository_registers_custom_repo()
    {
        var sc = new ServiceCollection();
        sc.AddDbContext<TestDataContext>(o => o.UseSqlite("DataSource=:memory:"));
        sc.AddXEntityFrameworkRepositories<TestDataContext>();
        sc.AddXRepository<ICustomRepo, CustomRepo>();
        var sp = sc.BuildServiceProvider();

        sp.GetRequiredService<ICustomRepo>().Should().BeOfType<CustomRepo>();
    }

    [Fact]
    public void AddXRepositories_registers_multiple()
    {
        var sc = new ServiceCollection();
        sc.AddDbContext<TestDataContext>(o => o.UseSqlite("DataSource=:memory:"));
        sc.AddXEntityFrameworkRepositories<TestDataContext>();
        sc.AddXRepositories(
            ServiceLifetime.Scoped,
            [(typeof(ICustomRepo), typeof(CustomRepo))]
        );
        var sp = sc.BuildServiceProvider();

        sp.GetRequiredService<ICustomRepo>().Should().BeOfType<CustomRepo>();
    }
}
