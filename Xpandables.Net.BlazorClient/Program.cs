using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Xpandables.Net.BlazorClient;
using Xpandables.Net.BlazorClient.Services;
using Xpandables.Net.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001")
});

// Register Xpandables.Net REST client
builder.Services.AddXRestClient<IBankAccountClient>();

await builder.Build().RunAsync();
