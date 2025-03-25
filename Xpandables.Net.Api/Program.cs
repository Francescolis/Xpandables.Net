using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.Api.Accounts.Persistence;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.Configure<EventOptions>(EventOptions.Default);
builder.Services.AddXEndpointRoutes();
builder.Services.AddXMinimalApi();
builder.Services.AddXDispatcher();
builder.Services.AddXHandlers();
builder.Services.AddXAggregateStore();
builder.Services.AddXEventStore();
builder.Services.AddXEventUnitOfWork();
builder.Services.AddXEventPublisher();
builder.Services.AddXAggregateDependencyProvider();
builder.Services.AddXDeciderDependencyManager();
builder.Services.AddXPipelineUnitOfWorkDecorator();
builder.Services.AddXPipelineAggregateDecorator();
builder.Services.AddXPipelineDeciderDecorator();

builder.Services.AddDataContextEventForSqlServer(builder.Configuration);

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

// ensure the database is created and migrated
using var scope = app.Services.CreateScope();
var eventContext = scope.ServiceProvider.GetRequiredService<DataContextEvent>();
await eventContext.Database.EnsureDeletedAsync();
await eventContext.Database.EnsureCreatedAsync();

app.Run();