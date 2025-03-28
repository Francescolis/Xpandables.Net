using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.MapRequest;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestByteArrayBuilderUnitTest
{
    private readonly HttpRequestByteArrayBuilder _builder;

    public HttpClientRequestByteArrayBuilderUnitTest() =>
        _builder = new HttpRequestByteArrayBuilder();

    [Fact]
    public void Build_ShouldSetByteArrayContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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
    public void Build_ShouldNotSetByteArrayContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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

    private class TestHttpRequestByteArray : IHttpRequest, IHttpRequestContentByteArray
    {
        public ByteArrayContent GetByteArrayContent() => new([1, 2, 3, 4]);
    }
}