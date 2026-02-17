using BankAccounts.Domain;
using BankAccounts.Infrastructure;

using Microsoft.OpenApi;

using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddXMinimalEndpointRoutes()
	.AddXResultEndpointValidator()
	.AddXJsonSerializerOptions()
	.AddXServiceExports(builder.Configuration, typeof(Account).Assembly, typeof(AccountDataContext).Assembly)
	.AddValidation()
	.AddMemoryCache()
	.AddXEventStores()
	.AddXResultMiddleware()
	.AddXEventContextMiddleware()
	.AddXResultProblemDetails()
	.AddXMinimalSupport(options =>
		options.ConfigureEndpoint = builder =>
			builder
				.WithXAsyncPagedFilterSupport()
				.WithXResultSupport());

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

WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
	options.WithTitle(builder.Configuration["OpenApiOptions:Name"] ?? "Kamersoft .Net");
	options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseXEventContextMiddleware();
app.UseXResultMiddleware();
app.UseXMinimalEndpointRoutes();


await app.RunAsync().ConfigureAwait(false);
