using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Builders.Requests;

using static Xpandables.Net.Http.MapRest;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestStreamBuilderUnitTest
{
    private readonly RestRequestStreamBuilder _builder;

    public HttpClientRequestStreamBuilderUnitTest() =>
        _builder = new RestRequestStreamBuilder();

    [Fact]
    public void Build_ShouldSetStreamContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
            {
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
    public void Build_ShouldNotSetStreamContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
            {
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
            {
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
            {
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

    private class TestHttpRequestStream : IRestRequest, IRestContentStream
    {
        public StreamContent GetStreamContent()
        {
            var stream = new System.IO.MemoryStream([1, 2, 3, 4]);
            return new StreamContent(stream);
        }
    }

}
