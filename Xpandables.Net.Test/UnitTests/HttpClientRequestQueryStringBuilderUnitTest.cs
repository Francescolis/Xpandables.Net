using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.RequestDefinitions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestQueryStringBuilderUnitTest
{
    private readonly RequestHttpQueryStringBuilder _builder;

    public HttpClientRequestQueryStringBuilderUnitTest() =>
        _builder = new RequestHttpQueryStringBuilder();

    [Fact]
    public void Build_ShouldSetQueryString_WhenLocationIsQuery()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
            {
                Location = Location.Query,
                Path = "http://example.com"
            },
            Request = new TestHttpRequestQueryString(),
            Message = new HttpRequestMessage
            {
                RequestUri = new Uri("http://example.com")
            }
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.RequestUri.Should().NotBeNull();
        context.Message.RequestUri.OriginalString
            .Should().Be("http://example.com?param1=value1&param2=value2");
    }

    [Fact]
    public void Build_ShouldNotSetQueryString_WhenLocationIsNotQuery()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
            {
                Location = Location.Body,
                Path = "http://example.com"
            },
            Request = new TestHttpRequestQueryString(),
            Message = new HttpRequestMessage
            {
                RequestUri = new Uri("http://example.com")
            }
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.RequestUri.Should().NotBeNull();
        context.Message.RequestUri.ToString().Should().Be("http://example.com/");
    }

    [Fact]
    public void AddQueryString_ShouldReturnPath_WhenQueryStringIsNull()
    {
        // Arrange
        string path = "http://example.com";

        // Act
        var result = HttpClientRequestQueryStringExtensions
            .AddQueryString(path, null);

        // Assert
        result.Should().Be(path);
    }

    [Fact]
    public void AddQueryString_ShouldAddQueryStringToPath()
    {
        // Arrange
        string path = "http://example.com";
        var queryString = new Dictionary<string, string?>
            {
                { "param1", "value1" },
                { "param2", "value2" }
            };

        // Act
        var result = HttpClientRequestQueryStringExtensions
            .AddQueryString(path, queryString);

        // Assert
        result.Should().Be("http://example.com?param1=value1&param2=value2");
    }

    [Fact]
    public void AddQueryString_ShouldHandleExistingQueryString()
    {
        // Arrange
        string path = "http://example.com?existingParam=existingValue";
        var queryString = new Dictionary<string, string?>
            {
                { "param1", "value1" },
                { "param2", "value2" }
            };

        // Act
        var result = HttpClientRequestQueryStringExtensions
            .AddQueryString(path, queryString);

        // Assert
        result.Should().Be("http://example.com?existingParam=existingValue&param1=value1&param2=value2");
    }

    [Fact]
    public void AddQueryString_ShouldHandleAnchor()
    {
        // Arrange
        string path = "http://example.com#anchor";
        var queryString = new Dictionary<string, string?>
            {
                { "param1", "value1" },
                { "param2", "value2" }
            };

        // Act
        var result = HttpClientRequestQueryStringExtensions
            .AddQueryString(path, queryString);

        // Assert
        result.Should().Be("http://example.com?param1=value1&param2=value2#anchor");
    }

    private class TestHttpRequestQueryString : IRequestHttp, IRequestQueryString
    {
        public IDictionary<string, string?>? GetQueryString() =>
            new Dictionary<string, string?>
                {
                    { "param1", "value1" },
                    { "param2", "value2" }
                };
    }
}
