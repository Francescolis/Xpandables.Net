using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.RequestDefinitions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestStreamBuilderUnitTest
{
    private readonly RequestHttpStreamBuilder _builder;

    public HttpClientRequestStreamBuilderUnitTest() =>
        _builder = new RequestHttpStreamBuilder();

    [Fact]
    public void Build_ShouldSetStreamContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
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
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
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
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
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
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
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

    private class TestHttpRequestStream : IRequestHttp, IRequestStream
    {
        public StreamContent GetStreamContent()
        {
            var stream = new System.IO.MemoryStream([1, 2, 3, 4]);
            return new StreamContent(stream);
        }
    }

}
