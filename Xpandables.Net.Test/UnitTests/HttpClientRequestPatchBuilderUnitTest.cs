﻿using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.MapRequest;
using static Xpandables.Net.Http.MapRequest.Patch;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestPatchBuilderUnitTest
{
    private readonly HttpRequestPatchBuilder _builder;

    public HttpClientRequestPatchBuilderUnitTest() =>
        _builder = new HttpRequestPatchBuilder();

    [Fact]
    public void Build_ShouldSetStringContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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
    public void Build_ShouldNotSetStringContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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
        var context = new RequestContext
        {
            Attribute = new MapHttpAttribute
            {
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
        HttpRequestPatch<TestHttpRequestPatch>, IHttpRequest
    {
    }
}
