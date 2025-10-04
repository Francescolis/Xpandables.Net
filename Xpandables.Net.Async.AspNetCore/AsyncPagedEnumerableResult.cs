/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using System.Net.Async;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Async;

namespace Xpandables.Net.Async;

/// <summary>
/// Represents an HTTP result that asynchronously writes a paged JSON response containing both page context metadata and
/// a sequence of data items to the response stream.
/// </summary>
/// <remarks>The response is formatted as a JSON object with a 'pageContext' property describing pagination
/// metadata and a 'data' array containing the serialized items. The response content type defaults to
/// 'application/json; charset=utf-8' unless overridden by endpoint metadata. The operation observes the request's
/// cancellation token and may be canceled if the client disconnects.</remarks>
/// <typeparam name="TResult">The type of the data items included in the paged response.</typeparam>
public sealed class AsyncPagedEnumerableResult<TResult> : IResult
{
    private readonly IAsyncPagedEnumerable<TResult> _results;
    private JsonTypeInfo<TResult>? _jsonTypeInfo;
    private JsonSerializerOptions? _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the AsyncPagedEnumerableResult class with the specified asynchronous paged
    /// enumerable results.
    /// </summary>
    /// <param name="results">The asynchronous paged enumerable that provides the result items to be wrapped by this instance. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="results"/> is null.</exception>
    /// <remarks>The <see cref="JsonSerializerOptions"/> or <see cref="JsonTypeInfo{T}"/> will be required from the context.</remarks>
    public AsyncPagedEnumerableResult(IAsyncPagedEnumerable<TResult> results)
    {
        _results = results ?? throw new ArgumentNullException(nameof(results));
        _jsonTypeInfo = null;
        _jsonSerializerOptions = null;
    }

    /// <summary>
    /// Initializes a new instance of the AsyncPagedEnumerableResult class with the specified asynchronous paged
    /// enumerable and JSON type information.
    /// </summary>
    /// <param name="results">The asynchronous paged enumerable that provides the sequence of results to be processed.</param>
    /// <param name="jsonTypeInfo">The JSON type information used to serialize and deserialize instances of TResult.</param>
    /// <exception cref="ArgumentNullException">Thrown if either results or jsonTypeInfo is null.</exception>
    public AsyncPagedEnumerableResult(IAsyncPagedEnumerable<TResult> results, JsonTypeInfo<TResult> jsonTypeInfo)
    {
        _results = results ?? throw new ArgumentNullException(nameof(results));
        _jsonTypeInfo = jsonTypeInfo ?? throw new ArgumentNullException(nameof(jsonTypeInfo));
        _jsonSerializerOptions = null;
    }

    /// <summary>
    /// Initializes a new instance of the AsyncPagedEnumerableResult class with the specified asynchronous paged results
    /// and JSON serializer options.
    /// </summary>
    /// <param name="results">An asynchronous paged enumerable that provides the result items to be processed.</param>
    /// <param name="serializerOptions">The options to use for JSON serialization and deserialization of result items.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="results"/> or <paramref name="serializerOptions"/> is <see langword="null"/>.</exception>
    public AsyncPagedEnumerableResult(IAsyncPagedEnumerable<TResult> results, JsonSerializerOptions serializerOptions)
    {
        _results = results ?? throw new ArgumentNullException(nameof(results));
        _jsonTypeInfo = null;
        _jsonSerializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    /// <summary>
    /// Asynchronously writes a JSON response containing the page context and data to the HTTP response stream.
    /// </summary>
    /// <remarks>The response is written in JSON format with a 'pageContext' property and a 'data' array. The
    /// response content type is set to 'application/json; charset=utf-8' unless overridden. The operation observes the
    /// request's cancellation token and may be canceled if the client disconnects.</remarks>
    /// <param name="httpContext">The HTTP context for the current request. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType ??= GetContentType(httpContext) ?? "application/json; charset=utf-8";

        if (_jsonTypeInfo is null)
        {
            _jsonSerializerOptions ??= GetJsonOptions(httpContext)
                ?? throw new InvalidOperationException("Either JsonSerializerOptions or JsonTypeInfo must be provided.");

            _jsonTypeInfo = _jsonSerializerOptions.GetTypeInfo(typeof(TResult)) as JsonTypeInfo<TResult>
                ?? throw new InvalidOperationException($"JsonTypeInfo for type '{typeof(TResult)}' could not be obtained from the provided JsonSerializerOptions.");
        }

        CancellationToken cancellationToken = httpContext.RequestAborted;

        using Utf8JsonWriter writer = new(
            httpContext.Response.BodyWriter.AsStream(),
            new JsonWriterOptions
            {
                Indented = _jsonTypeInfo.Options.WriteIndented,
                Encoder = _jsonTypeInfo.Options.Encoder
            });

        var pageContext = await _results.GetPageContextAsync(cancellationToken).ConfigureAwait(false);

        writer.WriteStartObject();
        writer.WritePropertyName("pageContext");
        JsonSerializer.Serialize(writer, pageContext, PageContextSourceGenerationContext.Default.PageContext);

        writer.WritePropertyName("data");
        writer.WriteStartArray();

        await foreach (TResult item in _results.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            JsonSerializer.Serialize(writer, item, _jsonTypeInfo);

            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    static string? GetContentType(HttpContext context)
    {
        if (context.GetEndpoint() is not Endpoint endpoint)
            return context.Response.GetTypedHeaders().ContentType?.ToString();

        return endpoint.Metadata
            .GetMetadata<IProducesResponseTypeMetadata>()?.ContentTypes
            .FirstOrDefault();
    }

    static JsonSerializerOptions? GetJsonOptions(HttpContext context)
    {
        JsonSerializerOptions? options = context.RequestServices
            .GetService<IOptions<JsonOptions>>()
            ?.Value?.SerializerOptions;

        return options;
    }
}
