using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestStringBuilderUnitTest
{
    private readonly HttpClientRequestStringBuilder _builder;

    public HttpClientRequestStringBuilderUnitTest() =>
        _builder = new HttpClientRequestStringBuilder();

    [Fact]
    public void Order_ShouldBeEleven()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(11);
    }

    [Fact]
    public void Build_ShouldSetStringContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.String,
                ContentType = "application/json"
            },
            Request = new TestHttpRequestString(),
            Message = new HttpRequestMessage(),
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().NotBeNull();
        context.Message.Content.Should().BeOfType<StringContent>();
        var content = context.Message.Content as StringContent;
        content!.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public void Build_ShouldNotSetStringContent_WhenIsNullableIsTrue()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = true,
                Location = Location.Body,
                BodyFormat = BodyFormat.String,
                ContentType = "application/json"
            },
            Request = new TestHttpRequestString(),
            Message = new HttpRequestMessage(),
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetStringContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Header,
                BodyFormat = BodyFormat.String,
                ContentType = "application/json"
            },
            Request = new TestHttpRequestString(),
            Message = new HttpRequestMessage(),
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetStringContent_WhenBodyFormatIsNotString()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.Multipart,
                ContentType = "application/json"
            },
            Request = new TestHttpRequestString(),
            Message = new HttpRequestMessage(),
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldAddStringContentToMultipart_WhenContentIsMultipart()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.String,
                ContentType = "application/json"
            },
            Request = new TestHttpRequestString(),
            Message = new HttpRequestMessage
            {
                Content = new MultipartFormDataContent()
            },
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        _builder.Build(context);

        // Assert
        var multipart = context.Message.Content as MultipartFormDataContent;
        multipart.Should().NotBeNull();
        multipart!.Should().ContainSingle();
    }

    private class TestHttpRequestString : IHttpClientRequest, IHttpRequestString
    {
        public object GetStringContent() => new { Key = "value" };
    }

}
