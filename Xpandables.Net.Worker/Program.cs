using System.Entities;
using System.Entities.EntityFramework;
using System.Events.Data;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Xpandables.Net.Worker.ReadStorage;


var builder = Host.CreateApplicationBuilder(args);

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
