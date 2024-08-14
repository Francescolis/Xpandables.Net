using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using Xpandables.Net.Aggregates.Events;
using Xpandables.Net.Api;
using Xpandables.Net.Api.I18n;
using Xpandables.Net.Api.Persons.Persistence;
using Xpandables.Net.Api.Persons.Repositories;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddLogging()
    .Configure<EventOptions>(options =>
    {
        EventOptions.Default(options);
        options.DisposeEventEntityAfterPersistence = false;
    })
    .AddXRequestAggregateHandlers()
    .AddXAllRequestHandlers(options =>
        options
        .UseOperationFinalizer()
        .UseValidator())
    .AddXEventHandlers()
    .AddXDistributor()
    .AddXValidatorGenerics()
    .AddXValidators()
    .AddXAggregateAccessor()
    .AddXRepositoryEvent<RepositoryPerson>()
    .AddXEventPublisher()
    .AddXEventIntegrationScheduler()
    .AddXEventDuplicateDecorator()
    .AddXEventStore()
    .AddXRequestAggregateHandlerDecorator()
    .AddXEndpointRoutes()
    .AddXOperationResultFinalizer()
    .AddXOperationResultSerializationConfigureOptions()
    .AddXOperationResultRequestValidator()
    .AddXOperationResultResponseBuilder()
    .AddXOperationResultMiddleware()
    .AddHttpContextAccessor()
    .AddScoped<DatabasePerson>()
    .AddScoped<IPersonExistChecker, PersonExistChecker>()
    .AddRouting(options =>
    {
        options.ConstraintMap.Add("string", typeof(StringConstraintMap));
        options.LowercaseQueryStrings = true;
        options.LowercaseUrls = true;
    })
    .AddSingleton(provider
        => provider.GetRequiredService<IOptions<JsonOptions>>()
            .Value
            .SerializerOptions)
    .AddLocalization()
    .AddRequestLocalization(options =>
    {
        IEnumerable<System.Globalization.CultureInfo> cultures =
        ICultureResourcesProvider.GetCultures();

        System.Globalization.CultureInfo defaultCulture = cultures.
        First(c => c.Name == ICultureResourcesProvider.DefaultCulture);
        string[] namedCultures = cultures.Select(c => c.Name).ToArray();

        options.SetDefaultCulture(defaultCulture.Name);
        options.ApplyCurrentCultureToResponseHeaders = true;
        options.AddSupportedCultures(namedCultures);
        options.AddSupportedUICultures(namedCultures);
        options.RequestCultureProviders =
        [
            new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
        ];
    })
    .AddSwaggerGen(options =>
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

        options.MapType<DateOnly>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "date"
        });

        options.MapType<TimeOnly>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "time"
        });

        options.EnableAnnotations();
        options.SchemaFilter<EnumSchemaFilter>();
    });

var app = builder.Build();

app.UseXOperationResultMiddleware()
    .UseSwagger()
    .UseSwaggerUI(options =>
    {
        string? routePrefix = builder.Configuration["SwaggerOptions:RoutePrefix"];
        string? prefix = builder.Configuration["SwaggerOptions:Prefix"];

        options.SwaggerEndpoint(
            $"{prefix}/swagger/{builder.Configuration["SwaggerOptions:Version"]}/swagger.json",
            builder.Configuration["SwaggerOptions:Description"]);
        options.DocExpansion(DocExpansion.None);
        options.RoutePrefix = routePrefix;
    })
    .UseStaticFiles()
    .UseRequestLocalization();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseXEndpointRoutes();

app.Run();