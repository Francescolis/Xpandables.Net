using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.DependencyInjection;

using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;

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
        .UseSqlServer(builder.Configuration.GetConnectionString("EventStoreDb"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .EnableServiceProviderCaching());

builder.Services.AddXOutboxStoreDataContext(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("EventStoreDb"))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        .EnableServiceProviderCaching());

// Register Xpandables.Net services
builder.Services
    .AddXEndpointRoutes()
    .AddXMinimalApi()
    .AddXMediatorWithEventSourcing()
    .AddXRequestHandlers()
    .AddXEventUnitOfWork()
    .AddXAggregateStore()
    .AddXEventStore()
    .AddXOutboxStore();

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

app.UseXExecutionResultMinimalMiddleware();

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


using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<EventStoreDataContext>();
    await dataContext.Database.EnsureCreatedAsync();
}

app.UseXEndpointRoutes();

await app.RunAsync();