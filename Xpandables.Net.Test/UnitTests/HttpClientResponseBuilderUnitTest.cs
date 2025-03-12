using System.Net;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.ResponseBuilders;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientResponseBuilderUnitTest
{
    [Fact]
    public async Task FailureAsyncResult_ShouldReturnFailureResponse()
    {
        // Arrange
        var builder = new ResponseHttpFailureStreamBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "Bad Request",
            Content = new StringContent("Error content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new ResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<ResponseHttp<IAsyncEnumerable<string>>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("Bad Request");
        response.Headers["Custom-Header"].Should().Be("HeaderValue");
        response.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task Failure_ShouldReturnFailureResponse()
    {
        // Arrange
        var builder = new ResponseHttpFailureBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "Bad Request",
            Content = new StringContent("Error content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new ResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<ResponseHttp>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("Bad Request");
        response.Headers["Custom-Header"].Should().Be("HeaderValue");
        response.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task FailureResult_ShouldReturnFailureResponse()
    {
        // Arrange
        var builder = new ResponseHttpFailureResultBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "Bad Request",
            Content = new StringContent("Error content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new ResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<ResponseHttp<string>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("Bad Request");
        response.Headers["Custom-Header"].Should().Be("HeaderValue");
        response.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task SuccessAsyncResult_ShouldReturnSuccessResponse()
    {
        // Arrange
        var builder = new ResponseHttpSuccessStreamBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "OK",
            Content = new StringContent("[\"Item1\", \"Item2\"]")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new ResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<ResponseHttp<IAsyncEnumerable<string>>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Version.Should().Be(new Version(1, 1));
        response.Result.Should().NotBeNull();
        response.ReasonPhrase.Should().Be("OK");
        response.Headers["Custom-Header"].Should().Be("HeaderValue");
        response.Exception.Should().BeNull();
    }

    [Fact]
    public async Task Success_ShouldReturnSuccessResponse()
    {
        // Arrange
        var builder = new ResponseHttpSuccessBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "OK",
            Content = new StringContent("Success content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new ResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<ResponseHttp>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("OK");
        response.Headers["Custom-Header"].Should().Be("HeaderValue");
        response.Exception.Should().BeNull();
    }

    record TestResponse(string Key);
    [Fact]
    public async Task SuccessResult_ShouldReturnSuccessResponse()
    {
        // Arrange
        var builder = new ResponseHttpSuccessResultBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "OK",
            Content = new StringContent("{\"key\":\"value\"}")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new ResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null,
                WriteIndented = true
            }
        };

        // Act
        var response = await builder.BuildAsync<ResponseHttp<TestResponse>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("OK");
        response.Headers["Custom-Header"].Should().Be("HeaderValue");
        response.Exception.Should().BeNull();
    }
}
