using System.Data.Common;
using System.Entities.Data;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.SampleApi.BankAccounts.Accounts;
using Xpandables.Net.SampleApi.ReadStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

builder.Services.AddXDbConnectionMsSqlServer(builder.Configuration.GetConnectionString("EventStoreDb")!);
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
    .AddXSlqBuilderMsSqlServer()
    .AddXDbConnectionScopeFactory()
    .AddXDbConnectionScope()
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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(builder.Configuration["SwaggerOptions:Version"], new OpenApiInfo()
    {
        Title = builder.Configuration["SwaggerOptions:Name"],
        Version = builder.Configuration["SwaggerOptions:Version"],
        Description = builder.Configuration["SwaggerOptions:Description"],
        Contact = builder.Configuration.GetSection("SwaggerOptions:Contact").Get<OpenApiContact>(),
        License = builder.Configuration.GetSection("SwaggerOptions:License").Get<OpenApiLicense>(),
        TermsOfService = new(builder.Configuration["SwaggerOptions:TermsOfService"]!)
    });

    options.EnableAnnotations();
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger()
    .UseSwaggerUI(options =>
    {
        string? routePrefix = builder.Configuration["SwaggerOptions:RoutePrefix"];
        string? prefix = builder.Configuration["SwaggerOptions:Prefix"];

        options.SwaggerEndpoint(
            $"{prefix}/swagger/{builder.Configuration["SwaggerOptions:Version"]}/swagger.json",
            builder.Configuration["SwaggerOptions:Description"]);
        options.DocExpansion(DocExpansion.None);
        options.RoutePrefix = routePrefix;
    });


//using (var scope = app.Services.CreateScope())
//{
//    var eventDb = scope.ServiceProvider.GetRequiredService<EventStoreDataContext>();
//    var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxStoreDataContext>();
//    await eventDb.Database.MigrateAsync().ConfigureAwait(false);
//    await outboxDb.Database.MigrateAsync().ConfigureAwait(false);
//}

app.UseXEventContextMiddleware();
app.UseXResultMiddleware();
app.UseXMinimalEndpointRoutes();


await app.RunAsync();
