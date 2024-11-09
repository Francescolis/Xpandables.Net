using FluentAssertions;

using Xpandables.Net.Collections;
using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestHeaderBuilderUnitTest
{
    private readonly HttpClientRequestHeaderBuilder _builder;

    public HttpClientRequestHeaderBuilderUnitTest() =>
        _builder = new HttpClientRequestHeaderBuilder();

    [Fact]
    public void Order_ShouldBeFour()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(4);
    }

    [Fact]
    public void Build_ShouldSetHeaders_WhenLocationIsHeader()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.Header
            },
            Request = new TestHttpRequestHeader(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Should().ContainKey("header1");
        context.Message.Headers.GetValues("header1").Should().Contain("value1");
        context.Message.Headers.Should().ContainKey("header2");
        context.Message.Headers.GetValues("header2").Should().Contain("value2");
    }

    [Fact]
    public void Build_ShouldNotSetHeaders_WhenLocationIsNotHeader()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.Body
            },
            Request = new TestHttpRequestHeader(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Should().BeEmpty();
    }

    [Fact]
    public void Build_ShouldSetModelNameHeader_WhenModelNameIsProvided()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                Location = Location.Header
            },
            Request = new TestHttpRequestHeaderWithModelName(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Should().ContainKey("ModelName");
        context.Message.Headers.GetValues("ModelName")
            .Should().Contain("header1,value1;header2,value2");
    }

    private class TestHttpRequestHeader : IHttpClientRequest, IHttpRequestHeader
    {
        public ElementCollection GetHeaders()
        {
            var headers = new ElementCollection
            {
                { "header1", "value1" },
                { "header2", "value2" }
            };
            return headers;
        }

        public string? GetHeaderModelName() => null;
    }

    private class TestHttpRequestHeaderWithModelName :
        IHttpClientRequest, IHttpRequestHeader
    {
        public ElementCollection GetHeaders()
        {
            var headers = new ElementCollection
            {
                { "header1", "value1" },
                { "header2", "value2" }
            };
            return headers;
        }

        public string? GetHeaderModelName() => "ModelName";
    }

}
