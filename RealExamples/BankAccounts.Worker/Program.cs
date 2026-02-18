using System.Data;
using System.Data.Common;
using System.Events.Integration;

using BankAccounts.Domain;
using BankAccounts.Infrastructure;

using Microsoft.AspNetCore.Http.Json;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<JsonOptions>(options =>
{
	options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
	options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
	options.SerializerOptions.WriteIndented = true;
});

builder.AddSqlServerClient("accountDB");
builder.AddSqlServerClient("eventDB");

builder.Services.AddXDataDbConnectionMsSqlServer(builder.Configuration.GetConnectionString("eventDB")!);
DbProviderFactories.RegisterFactory(
	DbProviders.MsSqlServer.InvariantName,
	Microsoft.Data.SqlClient.SqlClientFactory.Instance);

builder.Services
	.AddXJsonSerializerOptions()
	.AddXServiceExports(builder.Configuration, typeof(AccountDataContext).Assembly)
	.AddXScheduler()
	.AddXHostedScheduler()
	.AddXEventContextAccessor()
	.AddXEventConverterContext()
	.AddXEventConverterFactory()
	.AddXCacheTypeResolver(type => typeof(IIntegrationEvent).IsAssignableFrom(type), typeof(Account).Assembly)
	.AddXEventHandlers()
	.AddXEventHandlerInboxDecorator()
	.AddXEventPublisher()
	.AddXEventStores()
	.AddXIntegrationEventEnricher()
	.AddXDataUnitOfWork()
	.AddXDataSqlMapper()
	.AddXDataMsSqlBuilder()
	.AddXDataDbConnectionScopeFactory()
	.AddXDataDbConnectionScope();

IHost host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
