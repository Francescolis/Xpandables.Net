using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.DependencyInjection;

using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.SampleApi.EventStorage;

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

// Configure SqlServer database for event sourcing
builder.Services.AddXEventStoreDataContext(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("EventStoreDb"),
        options => options
            .EnableRetryOnFailure()
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            .MigrationsHistoryTable("__EventStoreMigrations")
            .MigrationsAssembly("Xpandables.Net.SampleApi"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .EnableServiceProviderCaching()
        .ReplaceService<IModelCustomizer, EventStoreModelCustomizer>());

builder.Services.AddXOutboxStoreDataContext(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("EventStoreDb"),
        options => options
            .EnableRetryOnFailure()
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            .MigrationsHistoryTable("__OutboxStoreMigrations")
            .MigrationsAssembly("Xpandables.Net.SampleApi"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .EnableServiceProviderCaching()
        .ReplaceService<IModelCustomizer, OutboxStoreModelCustomizer>());

// Register Xpandables.Net services
builder.Services
    .AddXEndpointRoutes()
    .AddXMinimalApi()
    .AddXMediatorWithEventSourcing()
    .AddXRequestHandlers()
    .AddXEventUnitOfWork()
    .AddXPublisher()
    .AddXAggregateStore()
    .AddXAggregateStoreFor()
    .AddXEventStore()
    .AddXOutboxStore()
    .AddMemoryCache()
    .AddXCacheTypeResolver();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(builder.Configuration["SwaggerOptions:Version"], new OpenApiInfo()
    {
        Title = builder.Configuration["SwaggerOptions:Name"],
        Version = builder.Configuration["SwaggerOptions:Version"],
        Description = builder.Configuration["SwaggerOptions:Description"],
        Contact = builder.Configuration.GetSection("SwaggerOptions:Contact")
            .Get<OpenApiContact>(),
        License = builder.Configuration.GetSection("SwaggerOptions:License")
            .Get<OpenApiLicense>(),
        TermsOfService = new(builder.Configuration["SwaggerOptions:TermsOfService"]!)
    });

    options.EnableAnnotations();
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

app.UseHttpsRedirection();
app.UseXExecutionResultMinimalMiddleware();
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


using (var scope = app.Services.CreateScope())
{
    var eventDb = scope.ServiceProvider.GetRequiredService<EventStoreDataContext>();
    var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxStoreDataContext>();
    await eventDb.Database.MigrateAsync().ConfigureAwait(false);
    await outboxDb.Database.MigrateAsync().ConfigureAwait(false);
}

app.UseXEndpointRoutes();

await app.RunAsync();