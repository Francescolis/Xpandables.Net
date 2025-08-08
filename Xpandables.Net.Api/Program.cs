using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.Api.Accounts;
using Xpandables.Net.Api.Accounts.Persistence;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Domains.Converters;
using Xpandables.Net.Executions.Handlers;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddXEndpointRoutes();
builder.Services.AddXMinimalApi();
builder.Services.AddXMediator();
builder.Services.AddXHandlers(typeof(Account).Assembly);
builder.Services.AddScoped(typeof(IRequestPostHandler<>), typeof(AggregateRequestPostHandler<>));
builder.Services.AddXDependencyProvider<AggregateDependencyProvider>();

builder.Services.AddXUnitOfWorkEvent();
builder.Services.AddXAggregateStore();
builder.Services.AddXEventStore();
builder.Services.AddXDataContextEvent(options =>
    options
       .UseNpgsql(builder.Configuration.GetConnectionString(nameof(DataContextEvent)))
       .EnableSensitiveDataLogging()
       .EnableDetailedErrors()
       .UseModel(DataContextEventSqlServerBuilder.CreateModel()));
builder.Services.AddXPublisher();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(builder.Configuration["SwaggerOptions:Version"], new()
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseXMinimalMiddleware();
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

app.UseXEndpointRoutes();

app.Run();

// Method to create and configure the model for in-memory database
#pragma warning disable CS8321 // Local function is declared but never used
static Microsoft.EntityFrameworkCore.Metadata.IModel CreateInMemoryModel()
{
    var modelBuilder = new ModelBuilder();

    // Apply all configurations from the assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContextEvent).Assembly);

    // Explicitly configure JsonDocument properties with converters
    modelBuilder.Entity<EntityDomainEvent>()
        .Property(e => e.Data)
        .HasConversion<JsonDocumentValueConverter>();

    modelBuilder.Entity<EntityIntegrationEvent>()
        .Property(e => e.Data)
        .HasConversion<JsonDocumentValueConverter>();

    modelBuilder.Entity<EntitySnapshotEvent>()
        .Property(e => e.Data)
        .HasConversion<JsonDocumentValueConverter>();

    return modelBuilder.FinalizeModel();
}
#pragma warning restore CS8321 // Local function is declared but never used
public partial class Program
{
}
