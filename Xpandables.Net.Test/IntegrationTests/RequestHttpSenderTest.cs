using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;

using static Xpandables.Net.Http.RequestDefinitions;

namespace Xpandables.Net.Test.IntegrationTests;

public sealed record Monkey(
    string Name,
    string Location,
    string Details,
    string Image,
    int Population,
    double Latitude,
    double Longitude);

[MapGet("monkeys.json")]
public sealed record Query : IRequestHttpAsync<Monkey>;

public sealed class RequestHttpSenderTest
{
    [Fact]
    public async Task RequestHttpSender_Should_Return_Valid_Monkeys()
    {
        // Arrange
        var services = new ServiceCollection();

        services.Configure<RequestOptions>(RequestOptions.Default);
        services.AddXRequestOptions();
        services.AddXRequestFactory();
        services.AddXResponseFactory();
        services.AddXRequestHttpSender((_, client) =>
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
        var sender = serviceProvider.GetRequiredService<IRequestHttpSender>();

        var query = new Query();

        // Act
        var response = await sender.SendAsync(query, CancellationToken.None);
        response.IsSuccessStatusCode.Should().BeTrue();
        var monkeys = await response.Result!.ToListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().NotBeNull();
        monkeys.Should().NotBeEmpty();
    }
}
