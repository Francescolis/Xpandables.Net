using System.Net;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Http;
using Xpandables.Net.Http.Builders.Responses;

namespace Xpandables.Net.Test.UnitTests;
public sealed class HttpClientResponseBuilderUnitTest
{
    [Fact]
    public async Task FailureAsyncResult_ShouldReturnFailureResponse()
    {
        // Arrange
        var builder = new RestResponseFailureBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            StatusCode = HttpStatusCode.BadRequest,
            Version = new Version(1, 1),
            ReasonPhrase = "Bad Request",
            Content = new StringContent("Error content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new RestResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<RestResponse<IAsyncEnumerable<string>>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("Bad Request");
        response.Headers.Should().Contain(p => p.Key == "Custom-Header" && p.Values.Contains("HeaderValue"));
        response.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task Failure_ShouldReturnFailureResponse()
    {
        // Arrange
        var builder = new RestResponseFailureBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "Bad Request",
            Content = new StringContent("Error content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new RestResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<RestResponse>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("Bad Request");
        response.Headers.Should().Contain(p => p.Key == "Custom-Header" && p.Values.Contains("HeaderValue"));
        response.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task FailureResult_ShouldReturnFailureResponse()
    {
        // Arrange
        var builder = new RestResponseFailureBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "Bad Request",
            Content = new StringContent("Error content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new RestResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<RestResponse<string>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("Bad Request");
        response.Headers.Should().Contain(p => p.Key == "Custom-Header" && p.Values.Contains("HeaderValue"));
        response.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task SuccessAsyncResult_ShouldReturnSuccessResponse()
    {
        // Arrange
        var builder = new RestResponseSuccessStreamBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "OK",
            Content = new StringContent("[\"Item1\", \"Item2\"]")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new RestResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<RestResponse<IAsyncEnumerable<string>>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Version.Should().Be(new Version(1, 1));
        response.Result.Should().NotBeNull();
        response.ReasonPhrase.Should().Be("OK");
        response.Headers.Should().Contain(p => p.Key == "Custom-Header" && p.Values.Contains("HeaderValue"));
        response.Exception.Should().BeNull();
    }

    [Fact]
    public async Task Success_ShouldReturnSuccessResponse()
    {
        // Arrange
        var builder = new RestResponseSuccessBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "OK",
            Content = new StringContent("Success content")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new RestResponseContext
        {
            Message = responseMessage,
            SerializerOptions = new JsonSerializerOptions()
        };

        // Act
        var response = await builder.BuildAsync<RestResponse>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("OK");
        response.Headers.Should().Contain(p => p.Key == "Custom-Header" && p.Values.Contains("HeaderValue"));
        response.Exception.Should().BeNull();
    }

    record TestResponse(string Key);
    [Fact]
    public async Task SuccessResult_ShouldReturnSuccessResponse()
    {
        // Arrange
        var builder = new RestResponseSuccessResultBuilder();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Version = new Version(1, 1),
            ReasonPhrase = "OK",
            Content = new StringContent("{\"key\":\"value\"}")
        };
        responseMessage.Headers.Add("Custom-Header", "HeaderValue");

        var context = new RestResponseContext
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
        RestResponse<TestResponse> response = await builder.BuildAsync<RestResponse<TestResponse>>(context);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Version.Should().Be(new Version(1, 1));
        response.ReasonPhrase.Should().Be("OK");
        response.Headers.Should().Contain(p => p.Key == "Custom-Header" && p.Values.Contains("HeaderValue"));
        response.Exception.Should().BeNull();
    }
}
