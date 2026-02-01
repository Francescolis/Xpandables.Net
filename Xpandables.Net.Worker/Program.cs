using System.Data.Common;
using System.Entities.Data;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Worker.ReadStorage;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddXDbConnectionMsSqlServer(builder.Configuration.GetConnectionString("EventStoreDb")!);
DbProviderFactories.RegisterFactory(
    DbProviders.MsSqlServer.InvariantName,
    Microsoft.Data.SqlClient.SqlClientFactory.Instance);

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
    .AddXScheduler()
    .AddXHostedScheduler()
    .AddXEventConverterFactory()
    .AddXCacheTypeResolver([typeof(Program).Assembly])
    .AddXEventHandlers()
    .AddXEventHandlerInboxDecorator()
    .AddXEventPublisher()
    .AddXEventConverterContext()
    .AddXIntegrationEventEnricher()
    .AddXDataUnitOfWork()
    .AddXSlqBuilderMsSqlServer()
    .AddXDbConnectionScopeFactory()
    .AddXDbConnectionScope()
    .AddXEventContextAccessor()
    .AddXEventStores();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.WriteIndented = true;
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var readDb = scope.ServiceProvider.GetRequiredService<BankAccountDataContext>();
    await readDb.Database.MigrateAsync().ConfigureAwait(false);
}

await host.RunAsync();
