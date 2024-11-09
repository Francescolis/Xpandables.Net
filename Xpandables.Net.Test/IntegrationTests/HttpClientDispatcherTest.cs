using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Test.IntegrationTests;

public sealed record Monkey(
    string Name,
    string Location,
    string Details,
    string Image,
    int Population,
    double Latitude,
    double Longitude);

[HttpClient(Path = "monkeys.json", Method = Method.GET,
    Location = Location.Body, IsNullable = true, IsSecured = false)]
public sealed record Query : IHttpClientAsyncRequest<Monkey>;

public sealed class HttpClientDispatcherTest
{
    [Fact]
    public async Task HttpClientDispatcher_Should_Return_Valid_Monkeys()
    {
        // Arrange
        var services = new ServiceCollection();

        services.Configure<HttpClientOptions>(HttpClientOptions.Default);
        services.AddXHttpClientOptions();
        services.AddXHttpClientMessageFactory();
        services.AddHttpClient<IHttpClientDispatcher, HttpClientDispatcherDefault>(client =>
        {
            client.BaseAddress = new Uri("https://www.montemagno.com/");
            client.Timeout = new TimeSpan(0, 5, 0);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers
                .MediaTypeWithQualityHeaderValue(ContentType.Json));
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IHttpClientDispatcher>();

        var query = new Query();

        // Act
        var response = await dispatcher.SendAsync(query, CancellationToken.None);
        response.IsValid.Should().BeTrue();
        var monkeys = await response.Result!.ToListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().NotBeNull();
        monkeys.Should().NotBeEmpty();
    }
}
