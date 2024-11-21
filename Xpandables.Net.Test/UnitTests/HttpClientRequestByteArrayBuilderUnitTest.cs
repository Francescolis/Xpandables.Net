using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.Interfaces.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestByteArrayBuilderUnitTest
{
    private readonly HttpClientByteArrayRequestBuilder _builder;

    public HttpClientRequestByteArrayBuilderUnitTest() =>
        _builder = new HttpClientByteArrayRequestBuilder();

    [Fact]
    public void Order_ShouldBeSix()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(6);
    }

    [Fact]
    public void Build_ShouldSetByteArrayContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.ByteArray
            },
            Request = new TestHttpRequestByteArray(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().NotBeNull();
        context.Message.Content.Should().BeOfType<ByteArrayContent>();
    }

    [Fact]
    public void Build_ShouldNotSetByteArrayContent_WhenIsNullableIsTrue()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = true,
                Location = Location.Body,
                BodyFormat = BodyFormat.ByteArray
            },
            Request = new TestHttpRequestByteArray(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetByteArrayContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
                Location = Location.Header,
                BodyFormat = BodyFormat.ByteArray
            },
            Request = new TestHttpRequestByteArray(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetByteArrayContent_WhenBodyFormatIsNotByteArray()
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
            Request = new TestHttpRequestByteArray(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    private class TestHttpRequestByteArray : IHttpClientRequest, IByteArrayRequest
    {
        public ByteArrayContent GetByteArrayContent() => new([1, 2, 3, 4]);
    }
}