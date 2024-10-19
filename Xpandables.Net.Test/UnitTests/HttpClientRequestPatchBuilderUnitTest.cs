using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.HttpClientParameters;
using static Xpandables.Net.Http.HttpClientParameters.Patch;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestPatchBuilderUnitTest
{
    private readonly HttpClientRequestPatchBuilder _builder;

    public HttpClientRequestPatchBuilderUnitTest() =>
        _builder = new HttpClientRequestPatchBuilder();

    [Fact]
    public void Order_ShouldBeTen()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(10);
    }

    [Fact]
    public void Build_ShouldSetStringContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientRequestOptionsAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.String,
                ContentType = ContentType.Json
            },
            Request = new TestHttpRequestPatch()
            {
                PatchOperationsBuilder = req =>
                [
                    Replace("/name", "newName")
                ]
            },
            Message = new HttpRequestMessage(),
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().NotBeNull();
        context.Message.Content.Should().BeOfType<StringContent>();
        var content = (StringContent)context.Message.Content!;
        content.Headers.ContentType!.MediaType.Should().Be(ContentType.Json);
    }

    [Fact]
    public void Build_ShouldNotSetStringContent_WhenIsNullableIsTrue()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientRequestOptionsAttribute
            {
                IsNullable = true,
                Location = Location.Body,
                BodyFormat = BodyFormat.String,
                ContentType = ContentType.Json
            },
            Request = new TestHttpRequestPatch()
            {
                PatchOperationsBuilder = req =>
                [
                    Replace("/name", "newName")
                ]
            },
            Message = new HttpRequestMessage()
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
            Attribute = new HttpClientRequestOptionsAttribute
            {
                IsNullable = false,
                Location = Location.Header,
                BodyFormat = BodyFormat.String,
                ContentType = ContentType.Json
            },
            Request = new TestHttpRequestPatch()
            {
                PatchOperationsBuilder = req =>
                [
                    Replace("/name", "newName")
                ]
            },
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
            Attribute = new HttpClientRequestOptionsAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.Stream,
                ContentType = ContentType.Json
            },
            Request = new TestHttpRequestPatch()
            {
                PatchOperationsBuilder = req =>
                [
                    Replace("/name", "newName")
                ]
            },
            Message = new HttpRequestMessage()
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
            Attribute = new HttpClientRequestOptionsAttribute
            {
                IsNullable = false,
                Location = Location.Body,
                BodyFormat = BodyFormat.String,
                ContentType = ContentType.Json
            },
            Request = new TestHttpRequestPatch()
            {
                PatchOperationsBuilder = req =>
                [
                    Replace("/name", "newName")
                ]
            },
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

    private record TestHttpRequestPatch :
        HttpRequestPatch<TestHttpRequestPatch>, IHttpClientRequest
    {
    }
}
