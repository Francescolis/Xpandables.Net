
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Xpandables.Net.Collections;
using Xpandables.Net.Extensions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with <see cref="IHttpClientDispatcher"/> response builder.
/// </summary>
public interface IHttpClientResponseBuilder
{
    /// <summary>
    /// The main method to build an <see cref="HttpClientResponse"/> for response 
    /// that contains an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TResult">The response content type.</typeparam>
    /// <param name="httpResponse">The target HTTP response.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="HttpClientResponse"/>.</returns>
    ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>> BuildHttpResponseAsync<TResult>(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions? serializerOptions = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The main method to build an <see cref="HttpClientResponse{TResult}"/> for response 
    /// that contains a result of <typeparamref name="TResult"/> type.
    /// </summary>
    /// <typeparam name="TResult">The response content type.</typeparam>
    /// <param name="httpResponse">The target HTTP response.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="HttpClientResponse"/>.</returns>
    ValueTask<HttpClientResponse<TResult>> BuildHttpResponse<TResult>(
       HttpResponseMessage httpResponse,
       JsonSerializerOptions? serializerOptions = default,
       CancellationToken cancellationToken = default);

    /// <summary>
    /// The main method to build an <see cref="HttpClientResponse"/> for response.
    /// </summary>
    /// <param name="httpResponse">The target HTTP response.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="HttpClientResponse"/>.</returns>
    ValueTask<HttpClientResponse> BuildHttpResponse(
       HttpResponseMessage httpResponse,
       JsonSerializerOptions? serializerOptions = default,
       CancellationToken cancellationToken = default);
}

internal sealed class HttpClientResponseBuilderInternal : IHttpClientResponseBuilder
{
    public async ValueTask<HttpClientResponse<TResult>> BuildHttpResponse<TResult>(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            return new HttpClientResponse<TResult>(
                httpResponse.StatusCode,
                httpResponse.ReadHttpResponseHeaders(),
                default,
                httpResponse.Version,
                httpResponse.ReasonPhrase,
                await BuildExceptionAsync(httpResponse).ConfigureAwait(false));
        }

        using Stream stream = await httpResponse.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        if (stream is null)
        {
            return new HttpClientResponse<TResult>(
                httpResponse.StatusCode,
                httpResponse.ReadHttpResponseHeaders(),
                default,
                httpResponse.Version,
                httpResponse.ReasonPhrase,
                default);
        }

        TResult? result;

        if (!httpResponse.Content.Headers.ContentDisposition?.FileName?.StartsWith("attachment", StringComparison.InvariantCulture) ?? true)
        {
            result = await JsonSerializer.DeserializeAsync<TResult>(
            stream,
            serializerOptions,
            cancellationToken)
            .ConfigureAwait(false);
        }
        else
        {
            var binary = await DoReadContentAsync(httpResponse, stream, cancellationToken).ConfigureAwait(false);
            result = binary.As<TResult>();
        }

        return new HttpClientResponse<TResult>(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            result,
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            default);

        static async ValueTask<BinaryEntry> DoReadContentAsync(
            HttpResponseMessage httpResponse,
            Stream stream,
            CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            var content = memoryStream.ToArray();
            var fileName = httpResponse.Content.Headers.ContentDisposition!.FileName!;
            var contentType = httpResponse.Content.Headers.ContentType!.MediaType!;
            var extension = Path.GetExtension(fileName).TrimStart('.');

            return new BinaryEntry(fileName, content, contentType, extension);
        }
    }

    public async ValueTask<HttpClientResponse> BuildHttpResponse(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            return new HttpClientResponse(
                httpResponse.StatusCode,
                httpResponse.ReadHttpResponseHeaders(),
                httpResponse.Version,
                httpResponse.ReasonPhrase,
                await BuildExceptionAsync(httpResponse).ConfigureAwait(false));
        }

        return new HttpClientResponse(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            default);
    }

    public async ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>> BuildHttpResponseAsync<TResult>(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                httpResponse.StatusCode,
                httpResponse.ReadHttpResponseHeaders(),
                default,
                httpResponse.Version,
                httpResponse.ReasonPhrase,
                await BuildExceptionAsync(httpResponse).ConfigureAwait(false));
        }

        Stream stream = await httpResponse.Content
             .ReadAsStreamAsync(cancellationToken)
             .ConfigureAwait(false);

        if (stream is null)
        {
            return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                httpResponse.StatusCode,
                httpResponse.ReadHttpResponseHeaders(),
                AsyncEnumerable.EmptyAsync<TResult>(),
                httpResponse.Version,
                httpResponse.ReasonPhrase,
                default);
        }

        IAsyncEnumerable<TResult> results = AsyncEnumerableBuilderAsync(stream, serializerOptions, cancellationToken);

        return new HttpClientResponse<IAsyncEnumerable<TResult>>(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            results,
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            default);

        async static IAsyncEnumerable<TResult> AsyncEnumerableBuilderAsync(
            Stream stream,
            JsonSerializerOptions? serializerOptions = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var blockingCollection = new BlockingCollection<TResult>();
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var blockingCollectionIterator = new AsyncEnumerable<TResult>(
                blockingCollection.GetConsumingEnumerable(cancellationToken))
                .GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            await Task.Run(() =>
                EnumerateStreamElementToBlockingCollection(
                    stream,
                    blockingCollection,
                    serializerOptions,
                    cancellationToken), cancellationToken)
                .ConfigureAwait(false);

            while (await blockingCollectionIterator.MoveNextAsync().ConfigureAwait(false))
                yield return blockingCollectionIterator.Current;
        }

        static void EnumerateStreamElementToBlockingCollection(
            Stream stream,
            BlockingCollection<TResult> resultCollection,
            JsonSerializerOptions? serializerOptions = default,
            CancellationToken cancellationToken = default)
        {
            using var jsonStreamReader = new Utf8JsonStreamReader(stream, 32 * 1024);

            jsonStreamReader.Read();
            while (jsonStreamReader.Read())
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (jsonStreamReader.TokenType != JsonTokenType.StartObject)
                    continue;

                if (jsonStreamReader.Deserialize<TResult>(serializerOptions) is { } result)
                {
                    resultCollection.Add(result, cancellationToken);
                }
            }

            resultCollection.CompleteAdding();
        }
    }

    static async ValueTask<HttpClientException?> BuildExceptionAsync(HttpResponseMessage httpResponse)
    {
        return await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false) switch
        {
            { } content when !string.IsNullOrWhiteSpace(content) => new HttpClientException(content),
            _ => default
        };
    }
}