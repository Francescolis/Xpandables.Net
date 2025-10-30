using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Entities;

namespace Xpandables.Net.UnitTests.Repositories;

public sealed class TestDataContext(DbContextOptions options) : DataContext(options)
{
    public DbSet<TestPerson> People => Set<TestPerson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestPerson>(b =>
        {
            b.ToTable("repo_TestPeople");
            b.HasKey(x => x.KeyId);
            b.Property(x => x.Name).IsRequired();
            b.Property(x => x.Age);
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.CreatedOn).IsRequired();
            b.Property(x => x.UpdatedOn);
            b.Property(x => x.DeletedOn);
        });
    }
}

public sealed class TestPerson : Entity<int>
{
    public required string Name { get; set; }
    public int Age { get; set; }
}
