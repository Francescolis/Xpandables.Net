using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.UnitTests.Repositories;

public sealed class SqliteTestFixture : IAsyncLifetime
{
    private readonly string _dbDirectory;
    private readonly string _dbPath;
    private readonly string _connectionString;

    public ServiceProvider ServiceProvider { get; private set; } = default!;

    public SqliteTestFixture()
    {
        _dbDirectory = Path.Combine(AppContext.BaseDirectory, "TestData");
        Directory.CreateDirectory(_dbDirectory);
        _dbPath = Path.Combine(_dbDirectory, "repositories.tests.sqlite");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TestDataContext>(o =>
        {
            o.UseSqlite(_connectionString);
            o.EnableSensitiveDataLogging();
        }, optionsLifetime: ServiceLifetime.Scoped);

        services.AddXEntityFrameworkRepositories<TestDataContext>();

        ServiceProvider = services.BuildServiceProvider(validateScopes: true);

        // Ensure DB and schema
        await using var scope = ServiceProvider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<TestDataContext>();
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await ServiceProvider.DisposeAsync();
    }

    public TestDataContext CreateContext() => ServiceProvider.GetRequiredService<TestDataContext>();
    public IUnitOfWork CreateUow() => ServiceProvider.GetRequiredService<IUnitOfWork>();
    public IRepository CreateRepo() => ServiceProvider.GetRequiredService<IRepository>();
    public IRepository<TestDataContext> CreateTypedRepo() => ServiceProvider.GetRequiredService<IRepository<TestDataContext>>();
}

[CollectionDefinition("sqlite-shared-db")]
public sealed class SqliteSharedDbCollection : ICollectionFixture<SqliteTestFixture> { }
