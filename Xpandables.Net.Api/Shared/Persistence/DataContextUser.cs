using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Shared.Persistence;

public sealed class DataContextUser(DbContextOptions<DataContextUser> options) :
    DataContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        optionsBuilder.UseSqlServer(
            PrimitiveConfiguration.Configuration
            .GetConnectionString(nameof(DataContextUser)))
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .EnableServiceProviderCaching();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.UserName).IsRequired();
            entity.Property(e => e.UserEmail).IsRequired();
            entity.Property(e => e.Password).IsRequired();
            entity.HasMany(e => e.Contacts)
                .WithOne()
                .HasForeignKey(e => e.Id);
        });

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<UserEntity> Users { get; set; } = default!;
}
