using System.Data;
using System.Data.Common;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Scalar.AspNetCore;

using Xpandables.Net.SampleApi.BankAccounts.Accounts;
using Xpandables.Net.SampleApi.ReadStorage;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
	options.AddDefaultPolicy(policy =>
		policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials()));

builder.Services.AddXDataDbConnectionMsSqlServer(builder.Configuration.GetConnectionString("EventStoreDb")!);
DbProviderFactories.RegisterFactory(
	DbProviders.MsSqlServer.InvariantName,
	Microsoft.Data.SqlClient.SqlClientFactory.Instance);

builder.Services.AddXDataContext<BankAccountDataContext>(options =>
	options
		.UseSqlServer(builder.Configuration.GetConnectionString("ReadStoreDb"),
		options => options
			.EnableRetryOnFailure()
			.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
		.EnableDetailedErrors()
		.EnableSensitiveDataLogging()
		.EnableServiceProviderCaching());

// Register Xpandables.Net services
builder.Services
	.AddXMinimalEndpointRoutes()
	.AddXResultEndpointValidator()
	.AddXJsonSerializerOptions()
	.AddXMediatorWithEventSourcingPipelines()
	.AddXRequestHandlers()
	.AddXEventHandlers()
	.AddXEventConverterContext()
	.AddXCompositeValidator()
	.AddXEventPublisher()
	.AddXAggregateStore()
	.AddXDataUnitOfWork()
	.AddXDataSqlMapper()
	.AddXDataMsSqlBuilder()
	.AddXDataDbConnectionScopeFactory()
	.AddXDataDbConnectionScope()
	.AddMemoryCache()
	.AddXEventStores()
	.AddXEventConverterFactory()
	.AddXCacheTypeResolver([typeof(BankAccount).Assembly])
	.AddXResultMiddleware()
	.AddXEventContextAccessor()
	.AddXEventContextMiddleware()
	.AddXDomainEventEnricher()
	.AddXIntegrationEventEnricher()
	.AddXResultProblemDetails()
	.AddXMinimalSupport(options =>
		options.ConfigureEndpoint = builder =>
			builder
				.WithXAsyncPagedFilterSupport()
				.WithXResultSupport());

builder.Services.AddValidation();

builder.Services.Configure<JsonOptions>(options =>
{
	options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
	options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
	options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
	{
		document.Info = new OpenApiInfo
		{
			Title = builder.Configuration["OpenApiOptions:Name"],
			Version = builder.Configuration["OpenApiOptions:Version"],
			Description = builder.Configuration["OpenApiOptions:Description"],
			TermsOfService = new Uri(builder.Configuration["OpenApiOptions:TermsOfService"]!),
			Contact = builder.Configuration.GetSection("OpenApiOptions:Contact").Get<OpenApiContact>(),
			License = builder.Configuration.GetSection("OpenApiOptions:License").Get<OpenApiLicense>()
		};

		return Task.CompletedTask;
	}));

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
	options.WithTitle(builder.Configuration["OpenApiOptions:Name"] ?? "Xpandables .Net");
	options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseXEventContextMiddleware();
app.UseXResultMiddleware();
app.UseXMinimalEndpointRoutes();


await app.RunAsync();
