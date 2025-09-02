
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Provides utility methods for creating and converting asynchronous paged enumerable results.
/// </summary>
/// <remarks>This static class contains methods to wrap or convert instances of <see
/// cref="IAsyncPagedEnumerable{T}"/> into <see cref="IResult"/> objects, enabling their use in asynchronous operations.
/// The methods ensure that the asynchronous paged enumerable is encapsulated in a result object for further
/// processing.</remarks>
public static class AsyncPagedEnumerableResult
{
    /// <summary>
    /// Creates a result object that wraps an asynchronous paged enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the asynchronous paged enumerable.</typeparam>
    /// <param name="value">The asynchronous paged enumerable to wrap. Cannot be null.</param>
    /// <returns>A result object that encapsulates the provided asynchronous paged enumerable.</returns>
    public static IResult Create<T>(IAsyncPagedEnumerable<T> value) => new AsyncPagedEnumerableResult<T>(value);
    /// <summary>
    /// Converts an <see cref="IAsyncPagedEnumerable{T}"/> to an <see cref="IResult"/> for use in asynchronous
    /// operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the asynchronous paged enumerable.</typeparam>
    /// <param name="value">The asynchronous paged enumerable to convert. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IResult"/> representing the asynchronous paged enumerable.</returns>
    public static IResult AsAsyncPagedEnumerableResult<T>(this IAsyncPagedEnumerable<T> value) => Create(value);
}

/// <summary>
/// Represents a result that asynchronously serializes and writes a paginated enumerable of items to the HTTP response
/// in JSON format.
/// </summary>
/// <remarks>This result is designed to handle paginated data efficiently by streaming the items to the response
/// as they are retrieved. The response includes a "pagination" object with metadata about the pagination and a "data"
/// array containing the serialized items.</remarks>
/// <typeparam name="T">The type of the items in the paginated enumerable.</typeparam>
/// <param name="value"></param>
public sealed class AsyncPagedEnumerableResult<T>(IAsyncPagedEnumerable<T> value) : IResult
{
    private readonly IAsyncPagedEnumerable<T> _value = value;

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = GetContentTypeFromEndpoint(httpContext) ?? "application/json; charset=utf-8";

        var jsonOptions = httpContext.RequestServices
            .GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions;

        var ct = httpContext.RequestAborted;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var writer = new Utf8JsonWriter(
            httpContext.Response.BodyWriter.AsStream(),
            new JsonWriterOptions
            {
                Indented = false
            });
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        // Ensure pagination first; in prime mode this primes and prepares the underlying stream.
        var pagination = await _value.GetPaginationAsync(ct).ConfigureAwait(false);

        writer.WriteStartObject();

        writer.WritePropertyName("pagination");
        JsonSerializer.Serialize(writer, pagination, jsonOptions);

        writer.WritePropertyName("data");
        writer.WriteStartArray();

        await foreach (var item in _value.WithCancellation(ct))
        {
            JsonSerializer.Serialize(writer, item, jsonOptions);
            await writer.FlushAsync(ct).ConfigureAwait(false);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        await writer.FlushAsync(ct).ConfigureAwait(false);
    }

    private static string? GetContentTypeFromEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return null;

        var producesMetadata = endpoint.Metadata
            .OfType<ProducesResponseTypeMetadata>()
            .FirstOrDefault();

        if (producesMetadata?.ContentTypes != null && producesMetadata.ContentTypes.Any())
        {
            return producesMetadata.ContentTypes.First();
        }

        var iProducesMetadata = endpoint.Metadata
            .OfType<IProducesResponseTypeMetadata>()
            .FirstOrDefault();

        if (iProducesMetadata?.ContentTypes != null && iProducesMetadata.ContentTypes.Any())
        {
            return iProducesMetadata.ContentTypes.First();
        }

        return null;
    }
}