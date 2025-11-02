namespace Xpandables.Net.SampleApi.EventStorage;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Xpandables.Net.Events.Repositories;
using Xpandables.Net.Repositories.Converters;

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