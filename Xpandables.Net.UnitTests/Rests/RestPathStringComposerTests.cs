using FluentAssertions;

using Xpandables.Net.Rests;
using Xpandables.Net.Rests.Abstractions;
using Xpandables.Net.Rests.RequestBuilders;

namespace Xpandables.Net.UnitTests.Rests;

public class RestPathStringComposerTests
{
    [Fact]
    public void AddPathString_ReplacesPlaceholders_CaseInsensitive()
    {
        // Arrange
        const string path = "/items/{ID}/sub/{name}";
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "42",
            ["NAME"] = "alpha"
        };

        // Act
        var result = RestPathStringComposer<TestPathRequest>.AddPathString(path, parameters);

        // Assert
        result.Should().Be("/items/42/sub/alpha");
    }

    [Fact]
    public void Compose_WithPathLocation_ShouldUpdateRequestUri()
    {
        // Arrange
        var attribute = new RestGetAttribute("/items/{id}")
        {
            Location = RestSettings.Location.Path
        };
        var message = new HttpRequestMessage(HttpMethod.Get, "/items/{id}");
        var request = new TestPathRequest(99);
        var context = new RestRequestContext
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = RestSettings.SerializerOptions
        };

        var composer = new RestPathStringComposer<TestPathRequest>();

        // Act
        composer.Compose(context);

        // Assert
        context.Message.RequestUri!.ToString().Should().Be("/items/99");
    }

    private sealed class TestPathRequest(int id) : IRestPathString
    {
        public IDictionary<string, string> GetPathString() => new Dictionary<string, string> { ["id"] = id.ToString() };
    }
}
