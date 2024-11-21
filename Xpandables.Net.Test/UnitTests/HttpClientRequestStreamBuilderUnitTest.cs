using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.Interfaces.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestStreamBuilderUnitTest
{
    private readonly HttpClientStreamRequestBuilder _builder;

    public HttpClientRequestStreamBuilderUnitTest() =>
        _builder = new HttpClientStreamRequestBuilder();

    [Fact]
    public void Order_ShouldBeNine()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(9);
    }

    [Fact]
    public void Build_ShouldSetStreamContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.Stream
            },
            Request = new TestHttpRequestStream(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().NotBeNull();
        context.Message.Content.Should().BeOfType<StreamContent>();
    }

    [Fact]
    public void Build_ShouldNotSetStreamContent_WhenIsNullableIsTrue()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = true,
                Location = Location.Body,
                BodyFormat = BodyFormat.Stream
            },
            Request = new TestHttpRequestStream(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetStreamContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Header,
                BodyFormat = BodyFormat.Stream
            },
            Request = new TestHttpRequestStream(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetStreamContent_WhenBodyFormatIsNotStream()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.String
            },
            Request = new TestHttpRequestStream(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldAddStreamContentToMultipart_WhenContentIsMultipart()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.Stream
            },
            Request = new TestHttpRequestStream(),
            Message = new HttpRequestMessage
            {
                Content = new MultipartFormDataContent()
            }
        };

        // Act
        _builder.Build(context);

        // Assert
        var multipart = context.Message.Content as MultipartFormDataContent;
        multipart.Should().NotBeNull();
        multipart!.Should().ContainSingle();
    }

    private class TestHttpRequestStream : IHttpClientRequest, IStreamRequest
    {
        public StreamContent GetStreamContent()
        {
            var stream = new System.IO.MemoryStream([1, 2, 3, 4]);
            return new StreamContent(stream);
        }
    }

}
