using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.Api.Models;
using Xpandables.Net.Api.Requests;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddXOperationResultMinimalApi();
builder.Services.AddXValidators();

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

app.UseXOperationResultMiddleware();
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

app.MapPost("/user", (CreateUserRequest request) =>
{
    User user = new()
    {
        UserName = request.UserName,
        Email = request.Email,
        Password = request.Password
    };

    return OperationResults
        .Created(user)
        .WithLocation("http://localtion.url")
        .Build();
})
.WithName("CreateUser")
.WithDescription("Create a new user.")
.WithXOperationResultMinimalApi()
.Produces<User>(201)
.ProducesValidationProblem()
.ProducesProblem(500)
.WithOpenApi();


app.Run();