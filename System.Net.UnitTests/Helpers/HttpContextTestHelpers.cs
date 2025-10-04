/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.IO.Pipelines;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace System.Net.UnitTests.Helpers;

/// <summary>
/// Test helpers for ASP.NET Core HTTP context mocking and testing.
/// </summary>
internal static class HttpContextTestHelpers
{
    /// <summary>
    /// Creates a mock HttpContext with the necessary features for testing AsyncPagedEnumerableResult.
    /// </summary>
    /// <param name="configureServices">Optional service configuration action.</param>
    /// <returns>A configured HttpContext for testing.</returns>
    public static HttpContext CreateTestHttpContext(Action<IServiceCollection>? configureServices = null)
    {
        var httpContext = new DefaultHttpContext();

        // Set up response body feature with a memory stream
        var memoryStream = new MemoryStream();
        httpContext.Features.Set<IHttpResponseBodyFeature>(new TestStreamResponseBodyFeature(memoryStream));

        // Set up response feature
        httpContext.Features.Set<IHttpResponseFeature>(new HttpResponseFeature());

        // Set up request feature
        httpContext.Features.Set<IHttpRequestFeature>(new HttpRequestFeature());

        // Always configure services, even if empty
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        httpContext.RequestServices = services.BuildServiceProvider();

        return httpContext;
    }

    /// <summary>
    /// Extracts the response body as a string from the HttpContext (synchronous version).
    /// </summary>
    /// <param name="httpContext">The HttpContext to extract the response from.</param>
    /// <returns>The response body as a UTF-8 string.</returns>
    public static string GetResponseBodyAsString(HttpContext httpContext)
    {
        var responseBodyFeature = httpContext.Features.Get<IHttpResponseBodyFeature>();
        if (responseBodyFeature is TestStreamResponseBodyFeature testFeature)
        {
            return testFeature.GetBodyAsString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Extracts the response body as a string from the HttpContext using a specific encoding.
    /// </summary>
    /// <param name="httpContext">The HttpContext to extract the response from.</param>
    /// <param name="encoding">The encoding to use to decode the response body.</param>
    /// <returns>The response body as a string decoded with the provided encoding.</returns>
    public static string GetResponseBodyAsString(HttpContext httpContext, Encoding encoding)
    {
        var responseBodyFeature = httpContext.Features.Get<IHttpResponseBodyFeature>();
        if (responseBodyFeature is TestStreamResponseBodyFeature testFeature)
        {
            return testFeature.GetBodyAsString(encoding);
        }

        return string.Empty;
    }

    /// <summary>
    /// Creates a mock HttpContext with endpoint metadata for testing content type resolution.
    /// </summary>
    /// <param name="contentTypes">The content types to include in the endpoint metadata.</param>
    /// <returns>A configured HttpContext with endpoint metadata.</returns>
    public static HttpContext CreateTestHttpContextWithEndpoint(params string[] contentTypes)
    {
        var httpContext = CreateTestHttpContext();
        var metadata = new EndpointMetadataCollection();
        var endpoint = new Endpoint(_ => Task.CompletedTask, metadata, "test-endpoint");

        httpContext.SetEndpoint(endpoint);
        return httpContext;
    }
}

/// <summary>
/// Custom stream response body feature for testing.
/// </summary>
internal sealed class TestStreamResponseBodyFeature(MemoryStream memoryStream) : IHttpResponseBodyFeature
{
    private readonly MemoryStream _memoryStream = memoryStream;

    public Stream Stream => _memoryStream;

    // Create a PipeWriter that writes to our memory stream
    public PipeWriter Writer => PipeWriter.Create(_memoryStream);

    public Task CompleteAsync() => Task.CompletedTask;
    public void DisableBuffering() { }
    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Gets the written content as a string using UTF-8.
    /// </summary>
    /// <returns>The content written to the body as a UTF-8 string.</returns>
    public string GetBodyAsString()
    {
        return GetBodyAsString(Encoding.UTF8);
    }

    /// <summary>
    /// Gets the written content as a string using a specific encoding.
    /// </summary>
    /// <param name="encoding">Encoding to use to decode the memory stream.</param>
    /// <returns>The content written to the body as a string.</returns>
    public string GetBodyAsString(Encoding encoding)
    {
        // Reset position and read content
        var originalPosition = _memoryStream.Position;
        _memoryStream.Position = 0;
        using var reader = new StreamReader(_memoryStream, encoding, leaveOpen: true);
        var result = reader.ReadToEnd();
        _memoryStream.Position = originalPosition;
        return result;
    }
}