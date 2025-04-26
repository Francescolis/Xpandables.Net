using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Domains.Converters;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Accounts.Persistence;

// To manage JsonDocument conversion for the event data.
public static class DataContextEventSqlServerBuilder
{
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
            .Property(p => p.EventData)
            .HasJsonDocumentConversion()
            .IsRequired();

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(p => p.EventData)
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
            .Property(p => p.EventData)
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