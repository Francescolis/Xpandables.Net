using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Domains.Converters;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Accounts.Persistence;

// To manage JsonDocument conversion for the event data.
public static class DataContextEventSqlServerBuilder
{
    public static IModel CreateModel()
    {
        ConventionSet conventions
            = NpgsqlConventionSetBuilder.Build();

        ModelBuilder modelBuilder = new(conventions);
        modelBuilder.HasDefaultSchema("Event");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContextEvent).Assembly);

        modelBuilder.Entity<EntityDomainEvent>()
            .ToTable(nameof(DataContextEvent.Domains))
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        modelBuilder.Entity<EntityDomainEvent>()
            .Property(p => p.Data)
            .IsRequired();

        modelBuilder.Entity<EntityDomainEvent>()
            .Property(p => p.Sequence)
            .UseIdentityAlwaysColumn();

        modelBuilder.Entity<EntityDomainEvent>()
            .Property<uint>("ConcurrencyToken")
            .IsRowVersion()
            .IsRequired();

        modelBuilder.Entity<EntityIntegrationEvent>()
            .ToTable(nameof(DataContextEvent.Integrations))
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(p => p.Data)
                .IsRequired();

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(p => p.Sequence)
            .UseIdentityAlwaysColumn();

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property<uint>("ConcurrencyToken")
            .IsRowVersion()
            .IsRequired();

        modelBuilder.Entity<EntitySnapshotEvent>()
            .ToTable(nameof(DataContextEvent.Snapshots))
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property(p => p.Data)
            .IsRequired();

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property(p => p.Sequence)
            .UseIdentityAlwaysColumn();

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property<uint>("ConcurrencyToken")
            .IsRowVersion()
            .IsRequired();

        return (IModel)modelBuilder.Model;
    }
    public static IServiceCollection AddDataContextEventForSqlServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConventionSet conventionSet = SqlServerConventionSetBuilder.Build();

        ModelBuilder modelBuilder = new(conventionSet);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContextEvent).Assembly);

        modelBuilder.Entity<EntityDomainEvent>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EntityDomainEvent>()
            .Property(p => p.Data)
            .HasJsonDocumentConversion()
            .IsRequired();

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(p => p.Data)
            .HasJsonDocumentConversion()
            .IsRequired();


        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property<byte[]>("ConcurrencyToken")
            .IsRowVersion()
            .IsRequired();

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property(p => p.Data)
            .HasJsonDocumentConversion()
            .IsRequired();


        return services.AddXDataContextEvent(options =>
            options.UseSqlServer(
                    configuration.GetConnectionString(nameof(DataContextEvent)))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .EnableServiceProviderCaching()
                .UseModel((IModel)modelBuilder.Model));
    }
}