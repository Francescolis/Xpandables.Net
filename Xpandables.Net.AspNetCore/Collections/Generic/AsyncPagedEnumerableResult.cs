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

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net;
using Xpandables.Net.Collections.Generic;
using Xpandables.Net.Collections.Generic.Extensions;

namespace Xpandables.Net.Collections.Generic;

/// <summary>
/// Represents an HTTP result that asynchronously writes a paged JSON response containing both page context metadata and
/// a sequence of data items to the response stream.
/// </summary>
/// <remarks>The response is formatted as a JSON object with a 'pagination' property describing pagination
/// metadata and an 'items' array containing the serialized items. The response content type defaults to
/// 'application/json; charset=utf-8' unless overridden by endpoint metadata. The operation observes the request's
/// cancellation token and may be canceled if the client disconnects.</remarks>
/// <typeparam name="TResult">The type of the data items included in the paged response.</typeparam>
public sealed class AsyncPagedEnumerableResult<TResult> : IResult
{
    private readonly IAsyncPagedEnumerable<TResult> _results;
    private readonly JsonTypeInfo<TResult>? _jsonTypeInfo;
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

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
    /// <remarks>The response is written in JSON format with a 'pagination' property and an 'items' array. The
    /// response content type is set to 'application/json; charset=utf-8' unless overridden. The operation observes the
    /// request's cancellation token and may be canceled if the client disconnects.</remarks>
    /// <param name="httpContext">The HTTP context for the current request. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is handled by extension methods with appropriate trimming annotations")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is handled by extension methods with appropriate AOT annotations")]
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType ??= GetContentType(httpContext) ?? "application/json; charset=utf-8";

        CancellationToken cancellationToken = httpContext.RequestAborted;

        // Use Stream directly to avoid disposal issues with PipeWriter.AsStream()
        Stream responseStream = httpContext.Response.BodyWriter.AsStream(leaveOpen: true);

        // Use the appropriate SerializeAsyncPaged overload based on what's available
        if (_jsonTypeInfo is not null)
        {
            await JsonSerializer.SerializeAsyncPaged(
                responseStream,
                _results,
                _jsonTypeInfo,
                cancellationToken).ConfigureAwait(false);
        }
        else if (_jsonSerializerOptions is not null)
        {
            await JsonSerializer.SerializeAsyncPaged(
                responseStream,
                _results,
                _jsonSerializerOptions,
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // Fallback: get options from the HTTP context
            JsonSerializerOptions? options = GetJsonOptions(httpContext)
                ?? throw new InvalidOperationException("Either JsonSerializerOptions or JsonTypeInfo must be provided.");

            await JsonSerializer.SerializeAsyncPaged(
                responseStream,
                _results,
                options,
                cancellationToken).ConfigureAwait(false);
        }
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
