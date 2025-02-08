using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events.Converters;
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

        modelBuilder.Entity<EventEntityDomain>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EventEntityDomain>()
            .Property(p => p.EventData)
            .HasColumnType("VARBINARY(MAX)")
            .HasReadOnlyMemoryToByteArrayConversion()
            .IsRequired();

        modelBuilder.Entity<EventEntityIntegration>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EventEntityIntegration>()
            .Property(p => p.EventData)
            .HasReadOnlyMemoryToByteArrayConversion()
            .HasColumnType("VARBINARY(MAX)")
            .IsRequired();


        modelBuilder.Entity<EventEntityIntegration>()
            .Property<byte[]>("ConcurrencyToken")
            .IsRowVersion()
            .IsRequired();

        modelBuilder.Entity<EventEntitySnapshot>()
            .Property(p => p.KeyId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        modelBuilder.Entity<EventEntitySnapshot>()
            .Property(p => p.EventData)
            .HasColumnType("VARBINARY(MAX)")
            .HasReadOnlyMemoryToByteArrayConversion()
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
