namespace Xpandables.Net.SampleApi.EventStorage;

using System.Data.Repositories.Converters;
using System.Events.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class EventStoreModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property(e => e.EventData)
            .HasConversion<JsonDocumentValueConverter>();

        modelBuilder.Entity<EntityDomainEvent>()
            .Property(e => e.EventData)
            .HasConversion<JsonDocumentValueConverter>();

        base.Customize(modelBuilder, context);
    }
}