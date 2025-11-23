namespace Xpandables.Net.SampleApi.EventStorage;

using System.Data.Repositories.Converters;
using System.Events.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class OutboxStoreModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(e => e.EventData)
            .HasConversion<JsonDocumentValueConverter>();

        base.Customize(modelBuilder, context);
    }
}