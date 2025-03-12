using System.Net.Http.Headers;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.RequestDefinitions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestBasicAuthBuilderUnitTest
{
    private readonly RequestHttpBasicAuthenticationBuilder _builder;

    public HttpClientRequestBasicAuthBuilderUnitTest() =>
        _builder = new RequestHttpBasicAuthenticationBuilder();

    [Fact]
    public void Build_ShouldSetAuthorizationHeader_WhenLocationIsBasicAuth()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
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
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
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

    private class TestHttpRequestBasicAuth : IRequestHttp, IRequestBasicAuthentication
    {
        public AuthenticationHeaderValue GetAuthenticationHeaderValue() =>
            new("Basic", "dGVzdDp0ZXN0");
    }
}
