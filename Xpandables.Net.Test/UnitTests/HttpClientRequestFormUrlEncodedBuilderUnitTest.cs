using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;

using static Xpandables.Net.Http.Interfaces.HttpClientParameters;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestFormUrlEncodedBuilderUnitTest
{
    private readonly HttpClientFormUrlEncodedRequestBuilder _builder;

    public HttpClientRequestFormUrlEncodedBuilderUnitTest() =>
        _builder = new HttpClientFormUrlEncodedRequestBuilder();

    [Fact]
    public void Order_ShouldBeSeven()
    {
        // Act
        var order = _builder.Order;

        // Assert
        order.Should().Be(7);
    }

    [Fact]
    public void Build_ShouldSetFormUrlEncodedContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
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
    public void Build_ShouldNotSetFormUrlEncodedContent_WhenIsNullableIsTrue()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = true,
                Location = Location.Body,
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
    public void Build_ShouldNotSetFormUrlEncodedContent_WhenLocationIsNotBody()
    {
        // Arrange
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
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
        var context = new HttpClientRequestContext
        {
            Attribute = new HttpClientAttribute
            {
                IsNullable = false,
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
        IHttpClientRequest, IFormUrlEncodedRequest
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
