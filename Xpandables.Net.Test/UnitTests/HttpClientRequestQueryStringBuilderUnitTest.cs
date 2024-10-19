using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestQueryStringBuilderUnitTest
{
    private readonly HttpClientRequestQueryStringBuilder _builder;

    public HttpClientRequestQueryStringBuilderUnitTest() =>
        _builder = new HttpClientRequestQueryStringBuilder();

    [Fact]
    public void Order_ShouldBeTwo()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(2);
    }

    [Fact]
    public void Build_ShouldSetQueryString_WhenLocationIsQuery()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientRequestOptionsAttribute
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
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientRequestOptionsAttribute
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

    private class TestHttpRequestQueryString : IHttpClientRequest, IHttpRequestQueryString
    {
        public IDictionary<string, string?>? GetQueryString() =>
            new Dictionary<string, string?>
                {
                    { "param1", "value1" },
                    { "param2", "value2" }
                };
    }
}
