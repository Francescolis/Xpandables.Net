using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.Interfaces.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestCookieBuilderUnitTest
{
    private readonly HttpClientCookieRequestBuilder _builder;

    public HttpClientRequestCookieBuilderUnitTest() =>
        _builder = new HttpClientCookieRequestBuilder();

    [Fact]
    public void Order_ShouldBeThree()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(3);
    }

    [Fact]
    public void Build_ShouldSetCookieHeader_WhenLocationIsCookie()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.Cookie
            },
            Request = new TestHttpRequestCookie(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Options.Should().ContainKey("cookie1");
        context.Message.Options
            .TryGetValue(new HttpRequestOptionsKey<string>("cookie1"), out string? value1)
            .Should().BeTrue();
        value1.Should().Be("value1");
        context.Message.Options.Should().ContainKey("cookie2");
        context.Message.Options
            .TryGetValue(new HttpRequestOptionsKey<string>("cookie2"), out string? value2)
            .Should().BeTrue();
        value2.Should().Be("value2");
    }

    [Fact]
    public void Build_ShouldNotSetCookieHeader_WhenLocationIsNotCookie()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.Body
            },
            Request = new TestHttpRequestCookie(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Options.Should().BeEmpty();
    }

    private class TestHttpRequestCookie : IHttpClientRequest, ICookieRequest
    {
        public IDictionary<string, object?> GetCookieHeaderValue() =>
            new Dictionary<string, object?>
                {
                    { "cookie1", "value1" },
                    { "cookie2", "value2" }
                };
    }

}
