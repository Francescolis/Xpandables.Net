using FluentAssertions;

using Xpandables.Net.Rests;
using Xpandables.Net.Rests.RequestBuilders;

namespace Xpandables.Net.UnitTests.Rests;

public class RestQueryStringComposerTests
{
    [Fact]
    public void Compose_WithQueryLocation_ShouldAppendParameters()
    {
        // Arrange
        var attribute = new RestGetAttribute("/api/items")
        {
            Location = RestSettings.Location.Query
        };
        var message = new HttpRequestMessage(HttpMethod.Get, "/api/items");
        var request = new TestQueryRequest(new Dictionary<string, string?>
        {
            ["q"] = "test",
            ["page"] = "2",
            ["filter"] = null
        });
        var context = new RestRequestContext
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = RestSettings.SerializerOptions
        };

        var composer = new RestQueryStringComposer<TestQueryRequest>();

        // Act
        composer.Compose(context);

        // Assert
        context.Message.RequestUri!.ToString().Should().Contain("?q=test");
        context.Message.RequestUri!.ToString().Should().Contain("page=2");
        context.Message.RequestUri!.ToString().Should().Contain("filter=");
    }

    private sealed class TestQueryRequest(IDictionary<string, string?> qs) : IRestQueryString
    {
        public IDictionary<string, string?>? GetQueryString() => qs;
    }
}
