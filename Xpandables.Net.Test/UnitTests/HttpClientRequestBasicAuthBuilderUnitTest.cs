using System.Net.Http.Headers;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestBasicAuthBuilderUnitTest
{
    private readonly HttpClientRequestBasicAuthBuilder _builder;

    public HttpClientRequestBasicAuthBuilderUnitTest() =>
        _builder = new HttpClientRequestBasicAuthBuilder();

    [Fact]
    public void Order_ShouldBeFive()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(5);
    }

    [Fact]
    public void Build_ShouldSetAuthorizationHeader_WhenLocationIsBasicAuth()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.BasicAuth
            },
            Request = new TestHttpRequestBasicAuth(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Authorization.Should().NotBeNull();
        context.Message.Headers.Authorization!.Scheme.Should().Be("Basic");
        context.Message.Headers.Authorization.Parameter.Should().Be("dGVzdDp0ZXN0");
    }

    [Fact]
    public void Build_ShouldNotSetAuthorizationHeader_WhenLocationIsNotBasicAuth()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.Body
            },
            Request = new TestHttpRequestBasicAuth(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Authorization.Should().BeNull();
    }

    private class TestHttpRequestBasicAuth : IHttpClientRequest, IHttpRequestBasicAuth
    {
        public AuthenticationHeaderValue GetAuthenticationHeaderValue() =>
            new("Basic", "dGVzdDp0ZXN0");
    }
}
