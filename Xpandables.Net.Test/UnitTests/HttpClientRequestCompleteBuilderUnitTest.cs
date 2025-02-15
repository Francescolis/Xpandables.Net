﻿using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestCompleteBuilderUnitTest
{
    private readonly RequestHttpCompletionBuilder _builder;

    public HttpClientRequestCompleteBuilderUnitTest() =>
        _builder = new RequestHttpCompletionBuilder();

    [Theory]
    [InlineData(typeof(IRequestCompletion), true)]
    [InlineData(typeof(IRequestHttp), true)]
    [InlineData(typeof(object), false)]
    public void CanBuild_ShouldReturnExpectedResult(Type targetType, bool expectedResult)
    {
        // Act
        var result = _builder.CanBuild(targetType);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Build_ShouldSetContentType_WhenContentIsNotNullAndContentTypeIsNull()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
            {
                ContentType = RequestDefinitions.ContentType.Json
            },
            Request = new TestHttpRequestDefinitionComplete(),
            Message = new HttpRequestMessage
            {
                Content = new StringContent("test content")
            }
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Headers.ContentType.Should().NotBeNull();
        context.Message.Content.Headers.ContentType!.MediaType
            .Should().Be(RequestDefinitions.ContentType.Json);
    }

    [Fact]
    public void Build_ShouldNotSetContentType_WhenContentIsNull()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
            {
                ContentType = RequestDefinitions.ContentType.Json
            },
            Request = new TestHttpRequestDefinitionComplete(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldSetAuthorizationHeader_WhenIsSecuredIsTrueAndAuthorizationHeaderIsNull()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
            {
                IsSecured = true,
                Scheme = "Bearer"
            },
            Request = new TestHttpRequestDefinitionComplete(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Authorization.Should().NotBeNull();
        context.Message.Headers.Authorization!.Scheme.Should().Be("Bearer");
    }

    [Fact]
    public void Build_ShouldNotSetAuthorizationHeader_WhenIsSecuredIsFalse()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new RequestDefinitionAttribute
            {
                IsSecured = false
            },
            Request = new TestHttpRequestDefinitionComplete(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Headers.Authorization.Should().BeNull();
    }

    private class TestHttpRequestDefinitionComplete :
        IRequestHttp, IRequestCompletion
    {
    }
}
