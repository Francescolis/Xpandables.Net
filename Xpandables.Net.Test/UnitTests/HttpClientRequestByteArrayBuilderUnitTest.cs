using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Builders.Requests;

using static Xpandables.Net.Http.MapRest;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestByteArrayBuilderUnitTest
{
    private readonly RestRequestByteArrayBuilder _builder;

    public HttpClientRequestByteArrayBuilderUnitTest() =>
        _builder = new RestRequestByteArrayBuilder();

    [Fact]
    public void Build_ShouldSetByteArrayContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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

    private class TestHttpRequestByteArray : IRestRequest, IRestContentByteArray
    {
        public ByteArrayContent GetByteArrayContent() => new([1, 2, 3, 4]);
    }
}