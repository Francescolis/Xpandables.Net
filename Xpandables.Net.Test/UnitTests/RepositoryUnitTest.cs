using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Test.UnitTests;

public sealed class RepositoryUnitTest
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    public RepositoryUnitTest()
    {
        // Arrange
        _serviceProvider = new ServiceCollection()
            .AddXUnitOfWork<UnitOfWork<TestDbContext>>()
            .AddXRepositoryDefault<TestDbContext>()
            .AddXDataContext<TestDbContext>(options =>
                options
                .UseSqlServer(@"Data Source=(localdb)\ProjectModels;Initial Catalog=XpandablesDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False")
                .UseSeeding((ctx, _) =>
                {
                    if (ctx.Set<TestEntity>().Any())
                        ctx.Set<TestEntity>().ExecuteDelete();
                    ctx.Set<TestEntity>().AddRange(
                        new TestEntity { KeyId = 1, Name = "Test1" },
                        new TestEntity { KeyId = 2, Name = "Test2" }
                    );
                    ctx.SaveChanges();
                }))
            .BuildServiceProvider();

        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        using var scope = _serviceProvider.CreateScope();
        // Initialize the database and insert initial data
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task FetchAsync_ShouldReturnEntities()
    {
        // Arrange
        Func<IQueryable<TestEntity>, IQueryable<TestEntity>> filter = query => query.Where(e => e.KeyId > 0);

        await using var repository = _unitOfWork.GetRepository<IRepository>();
        // Act
        var result = await repository.FetchAsync(filter).ToListAsync();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FetchAsyncAnonymous_ShouldReturnAnonymousTypes()
    {
        // Arrange
        await using var repository = _unitOfWork.GetRepository<IRepository>();
        // Act
        var result = await repository
            .FetchAsync((IQueryable<TestEntity> query) =>
                query.Where(e => e.KeyId > 0).Select(e => new { e.KeyId, e.Name }))
            .ToListAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(e => e.GetType().GetProperty("KeyId") != null &&
                                         e.GetType().GetProperty("Name") != null);
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
            {
                new() { KeyId = 3,  Name = "Test3" },
                new() { KeyId = 4, Name = "Test4" }
            };

        // Act
        await using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            await using var repository = _unitOfWork.GetRepository<IRepository>();
            await repository.AddOrUpdateAsync(entities);
        }

        // Assert
        Func<IQueryable<TestEntity>, IQueryable<TestEntity>> filter = query => query.Where(e => e.KeyId > 2);

        await using var repository1 = _unitOfWork.GetRepository<IRepository>();
        var result = await repository1.FetchAsync(filter).ToListAsync();
        result.Should().Contain(entities);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntities()
    {
        // Arrange
        Func<IQueryable<TestEntity>, IQueryable<TestEntity>> filter = query => query.Where(e => e.KeyId == 1);

        await using var repository = _unitOfWork.GetRepository<IRepository>();

        // Act
        await repository.DeleteAsync(filter);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await repository.FetchAsync(filter).ToListAsync();
        result.Should().NotContain(e => e.KeyId == 1);
    }

    [Fact]
    public void GetRepository_ShouldReturnRepository()
    {
        // Arrange

        // Act
        var repository = _unitOfWork.GetRepository<IRepository>();

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task UsingStatement_ShouldCallSaveChangesAsyncOnDispose()
    {
        // Arrange // Act
        await using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            await using var repository = _unitOfWork.GetRepository<IRepository>();
            await repository.AddOrUpdateAsync(new List<TestEntity>
            { new() { KeyId = 5,  Name = "Test5" } });
            // No need to call SaveChangesAsync explicitly, it should be called automatically
        }

        // Assert
        await using var _repository = _unitOfWork.GetRepository<IRepository>();
        var name = await _repository.FetchAsync(
            (IQueryable<TestEntity> query) => query.Where(e => e.KeyId == 5)
            .Select(e => e.Name))
            .FirstAsync();
        name.Should().Be("Test5");
    }
}

public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DataContext(options)
{
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<TestEntity>().HasNoKey();
        modelBuilder.Entity<TestEntity>().Property<Guid>("Id").ValueGeneratedOnAdd();
        modelBuilder.Entity<TestEntity>().HasKey("Id");
        modelBuilder.Entity<TestEntity>().Property(e => e.KeyId).IsRequired();
        modelBuilder.Entity<TestEntity>().Property(e => e.Name).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<TestEntity>().ToTable("TestEntities");

        base.OnModelCreating(modelBuilder);
    }
}

// Assuming TestEntity is a real implementation of IEntity
public class TestEntity : Entity<int>
{
    public required string Name { get; init; }
}