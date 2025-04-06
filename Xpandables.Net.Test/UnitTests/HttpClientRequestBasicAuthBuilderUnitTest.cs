using System.Net.Http.Headers;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Builders.Requests;

using static Xpandables.Net.Http.MapRest;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestBasicAuthBuilderUnitTest
{
    private readonly RestRequestBasicAuthenticationBuilder _builder;

    public HttpClientRequestBasicAuthBuilderUnitTest() =>
        _builder = new RestRequestBasicAuthenticationBuilder();

    [Fact]
    public void Build_ShouldSetAuthorizationHeader_WhenLocationIsBasicAuth()
    {
        // Arrange
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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

    private class TestHttpRequestBasicAuth : IRestRequest, IRestContentBasicAuthentication
    {
        public AuthenticationHeaderValue GetAuthenticationHeaderValue() =>
            new("Basic", "dGVzdDp0ZXN0");
    }
}
