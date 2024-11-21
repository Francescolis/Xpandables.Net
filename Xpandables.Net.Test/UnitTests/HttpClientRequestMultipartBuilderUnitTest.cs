using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.Interfaces.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestMultipartBuilderUnitTest
{
    private readonly HttpClientMultipartRequestBuilder _builder;

    public HttpClientRequestMultipartBuilderUnitTest() =>
        _builder = new HttpClientMultipartRequestBuilder();

    [Fact]
    public void Order_ShouldBeEight()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(8);
    }

    [Fact]
    public void Build_ShouldSetMultipartContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.Multipart
            },
            Request = new TestHttpRequestMultipart(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().NotBeNull();
        context.Message.Content.Should().BeOfType<MultipartFormDataContent>();
    }

    [Fact]
    public void Build_ShouldNotSetMultipartContent_WhenIsNullableIsTrue()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = true,
                Location = Location.Body,
                BodyFormat = BodyFormat.Multipart
            },
            Request = new TestHttpRequestMultipart(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetMultipartContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Header,
                BodyFormat = BodyFormat.Multipart
            },
            Request = new TestHttpRequestMultipart(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetMultipartContent_WhenBodyFormatIsNotMultipart()
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
            Request = new TestHttpRequestMultipart(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    private class TestHttpRequestMultipart : IHttpClientRequest, IMultipartRequest
    {
        public MultipartFormDataContent GetMultipartContent()
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent("value1"), "key1" },
                { new StringContent("value2"), "key2" }
            };
            return content;
        }
    }

}
