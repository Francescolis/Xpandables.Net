using System.Events.Data;
using System.Events.Data.Configurations;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Xpandables.Net.Worker.ReadStorage;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddXEventDataContext(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("EventStoreDb"),
        options => options
            .EnableRetryOnFailure()
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            .MigrationsHistoryTable("__EventStoreMigrations")
            .MigrationsAssembly("Xpandables.Net.Worker"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .EnableServiceProviderCaching()
        .ReplaceService<IModelCustomizer, EventStoreSqlServerModelCustomizer>());

builder.Services.AddXDataContext<BankAccountDataContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("ReadStoreDb"),
        options => options
            .EnableRetryOnFailure()
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            .MigrationsHistoryTable("_ReadStoreMigrations")
            .MigrationsAssembly("Xpandables.Net.Worker"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .EnableServiceProviderCaching());

builder.Services
    .AddXJsonSerializerOptions()
    .AddXEventHandlers()
    .AddXEventHandlerInboxDecorator()
    .AddXEventConverterContext()
    .AddXEventPublisher()
    .AddXEventStore()
    .AddXOutboxStore()
    .AddXInboxStore()
    .AddXEventRepositories()
    .AddXUnitOfWork<EventDataContext>()
    .AddXEventConverterFactory()
    .AddXCacheTypeResolver([typeof(Program).Assembly])
    .AddXScheduler()
    .AddXHostedScheduler()
    .AddXEventContextAccessor()
    .AddXIntegrationEventEnricher();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.WriteIndented = true;
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var outboxDb = scope.ServiceProvider.GetRequiredService<EventDataContext>();
    var readDb = scope.ServiceProvider.GetRequiredService<BankAccountDataContext>();
    await outboxDb.Database.MigrateAsync().ConfigureAwait(false);
    await readDb.Database.MigrateAsync().ConfigureAwait(false);
}

await host.RunAsync();
