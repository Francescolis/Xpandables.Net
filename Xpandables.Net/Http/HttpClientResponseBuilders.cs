/*******************************************************************************
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
********************************************************************************/
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.Http;

/// <summary>
/// Builds the success response from the <see cref="HttpRequestMessage"/>.
/// </summary>
public sealed class SuccessHttpClientResponseBuilder :
    HttpClientResponseBuilder
{
    ///<inheritdoc/>
    public override bool CanBuild(HttpStatusCode statusCode)
        => statusCode.IsSuccessStatusCode();

    ///<inheritdoc/>
    public override async ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        if (httpResponse.Content.Headers.ContentDisposition is not null)
            if (httpResponse
                .Content
                .Headers
                .ContentDisposition
                .DispositionType
                .StartsWith("attachment", StringComparison.InvariantCulture))
            {
                string fileName = httpResponse
                    .Content
                    .Headers
                    .ContentDisposition
                    .FileName!
                    .Trim('"');

                Uri requestUri = httpResponse.RequestMessage!.RequestUri!;
                string baseUrl = requestUri.GetLeftPart(UriPartial.Authority);

                string fileUrl = $"{baseUrl}/{Uri.EscapeDataString(fileName)}";

                System.Collections.Specialized.NameValueCollection headers
                    = httpResponse.ReadHttpResponseHeaders();
                headers.Add("Location", fileUrl);

                return new HttpClientResponse(
                    httpResponse.StatusCode,
                    headers,
                    httpResponse.Version,
                    httpResponse.ReasonPhrase);
            }

        return new HttpClientResponse(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            default);
    }
}

/// <summary>
/// Builds the failure response from the <see cref="HttpRequestMessage"/>.
/// </summary>
public sealed class FailureHttpClientResponseBuilder :
    HttpClientResponseBuilder
{
    ///<inheritdoc/>
    public override bool CanBuild(HttpStatusCode statusCode)
        => statusCode.IsFailureStatusCode();

    ///<inheritdoc/>
    public async override ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
        => new HttpClientResponse(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            await httpResponse.BuildExceptionAsync()
            .ConfigureAwait(false));
}

/// <summary>
/// Builds the success response from the <see cref="HttpRequestMessage"/>
/// of a specific type result.
/// </summary>
public sealed class SucessHttpClientResponseBuilder<TResult>
    : HttpClientResponseBuilder<TResult>
{
    ///<inheritdoc/>
    public override bool CanBuild(HttpStatusCode statusCode)
        => statusCode.IsSuccessStatusCode();
    ///<inheritdoc/>
    public async override ValueTask<HttpClientResponse<TResult>> BuildAsync(HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
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

        TResult? result = await JsonSerializer
            .DeserializeAsync<TResult>(
                stream,
                options,
                cancellationToken)
            .ConfigureAwait(false);

        return new HttpClientResponse<TResult>(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            result,
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            default);
    }
}

/// <summary>
/// Builds the failure response from the <see cref="HttpRequestMessage"/>
/// of a specific type result.
/// </summary>
public sealed class FailureHttpClientResponseBuilder<TResult>
    : HttpClientResponseBuilder<TResult>
{
    ///<inheritdoc/>
    public override bool CanBuild(HttpStatusCode statusCode)
        => statusCode.IsFailureStatusCode();
    ///<inheritdoc/>
    public async override ValueTask<HttpClientResponse<TResult>> BuildAsync(HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
        => new HttpClientResponse<TResult>(
              httpResponse.StatusCode,
              httpResponse.ReadHttpResponseHeaders(),
              default,
              httpResponse.Version,
              httpResponse.ReasonPhrase,
              await httpResponse.BuildExceptionAsync()
              .ConfigureAwait(false));
}

/// <summary>
/// Builds the success response from the <see cref="HttpRequestMessage"/>
/// of an <see cref="IAsyncEnumerable{T}"/> of specific type.
/// </summary>
public sealed class SucessHttpClientResponseAsyncBuilder<TResult>
    : HttpClientResponseAsyncBuilder<TResult>
{
    ///<inheritdoc/>
    public override bool CanBuild(HttpStatusCode statusCode)
        => statusCode.IsSuccessStatusCode();
    ///<inheritdoc/>
    public async override ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>>
        BuildAsync(HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
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

        IAsyncEnumerable<TResult> results
            = AsyncEnumerableBuilderAsync(
                stream,
                options,
                cancellationToken);

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
            using BlockingCollection<TResult> blockingCollection = [];
#pragma warning disable CA2007 // Consider calling ConfigureAwait 
            //on the awaited task
            await using IAsyncEnumerator<TResult> blockingCollectionIterator
                = new AsyncEnumerable<TResult>(
                blockingCollection.GetConsumingEnumerable(cancellationToken))
                .GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait 
            // on the awaited task

            _ = await Task.Run(async () =>
            {
                await foreach (TResult? element in JsonSerializer
                    .DeserializeAsyncEnumerable<TResult>(
                    stream,
                    serializerOptions,
                    cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (element is { } result)
                        blockingCollection.Add(result, cancellationToken);
                }

            }, cancellationToken)
                .ContinueWith(t =>
                {
                    blockingCollection.CompleteAdding();

                    if (t.IsFaulted)
                        t.Exception.ReThrow();

                    return Task.CompletedTask;

                }, TaskScheduler.Current)
                .ConfigureAwait(false);

            while (await blockingCollectionIterator.MoveNextAsync()
                .ConfigureAwait(false))
                yield return blockingCollectionIterator.Current;
        }
    }
}

/// <summary>
/// Builds the failure response from the <see cref="HttpRequestMessage"/>
/// of an <see cref="IAsyncEnumerable{T}"/> of specific type.
/// </summary>
public sealed class FailureHttpClientResponseAsyncBuilder<TResult>
    : HttpClientResponseAsyncBuilder<TResult>
{
    ///<inheritdoc/>
    public override bool CanBuild(HttpStatusCode statusCode)
        => statusCode.IsFailureStatusCode();
    ///<inheritdoc/>
    public async override ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>>
        BuildAsync(HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
        => new HttpClientResponse<IAsyncEnumerable<TResult>>(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            default,
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            await httpResponse.BuildExceptionAsync()
            .ConfigureAwait(false));
}
