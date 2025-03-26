﻿using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.RequestDefinitions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestFormUrlEncodedBuilderUnitTest
{
    private readonly RequestHttpFormUrlEncodedBuilder _builder;

    public HttpClientRequestFormUrlEncodedBuilderUnitTest() =>
        _builder = new RequestHttpFormUrlEncodedBuilder();

    [Fact]
    public void Build_ShouldSetFormUrlEncodedContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
            {
                Location = Location.Body,
                BodyFormat = BodyFormat.FormUrlEncoded
            },
            Request = new TestHttpRequestFormUrlEncoded(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().NotBeNull();
        context.Message.Content.Should().BeOfType<FormUrlEncodedContent>();
    }

    [Fact]
    public void Build_ShouldNotSetFormUrlEncodedContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
            {
                Location = Location.Header,
                BodyFormat = BodyFormat.FormUrlEncoded
            },
            Request = new TestHttpRequestFormUrlEncoded(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    [Fact]
    public void Build_ShouldNotSetFormUrlEncodedContent_WhenBodyFormatIsNotFormUrlEncoded()
    {
        // Arrange
        var context = new RequestContext
        {
            Attribute = new MapRequestAttribute
            {
                Location = Location.Body,
                BodyFormat = BodyFormat.String
            },
            Request = new TestHttpRequestFormUrlEncoded(),
            Message = new HttpRequestMessage()
        };

        // Act
        _builder.Build(context);

        // Assert
        context.Message.Content.Should().BeNull();
    }

    private class TestHttpRequestFormUrlEncoded :
        IRequestHttp, IRequestFormUrlEncoded
    {
        public FormUrlEncodedContent GetFormUrlEncodedContent()
        {
            var data = new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                };
            return new FormUrlEncodedContent(data);
        }
    }

}
