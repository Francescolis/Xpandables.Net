using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Builders.Requests;

using static Xpandables.Net.Http.MapRest;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientRequestFormUrlEncodedBuilderUnitTest
{
    private readonly RestRequestFormUrlEncodedBuilder _builder;

    public HttpClientRequestFormUrlEncodedBuilderUnitTest() =>
        _builder = new RestRequestFormUrlEncodedBuilder();

    [Fact]
    public void Build_ShouldSetFormUrlEncodedContent_WhenConditionsAreMet()
    {
        // Arrange
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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
        var context = new RestRequestContext
        {
            Attribute = new MapRestAttribute
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
        IRestRequest, IRestContentFormUrlEncoded
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
