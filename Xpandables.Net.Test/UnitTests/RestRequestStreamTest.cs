using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Rests;

using static Xpandables.Net.Executions.Rests.Rest;

namespace Xpandables.Net.Test.UnitTests;

public sealed record Monkey(
    string Name,
    string Location,
    string Details,
    string Image,
    int Population,
    double Latitude,
    double Longitude);

[RestGet("monkeys.json")]
public sealed record Query : IRestRequestStream<Monkey>, IRestString;

public sealed class RestRequestStreamTest
{
    [Fact]
    public async Task RequestHttpSender_Should_Return_Valid_Monkeys()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddXRestAttibuteProvider();
        services.AddXRestRequestBuilders();
        services.AddXRestResponseBuilders();
        services.AddXRestRequestHandler();
        services.AddXRestResponseHandler();
        services.AddXRestClient((_, client) =>
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
        var sender = serviceProvider.GetRequiredService<IRestClient>();

        var query = new Query();

        // Act
        using RestResponse<IAsyncEnumerable<Monkey>> response = await sender.SendAsync(query, CancellationToken.None);
        response.IsSuccess.Should().BeTrue();
        var monkeys = await response.Result!.ToListAsync();

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().NotBeNull();
        monkeys.Should().NotBeEmpty();
    }
}
