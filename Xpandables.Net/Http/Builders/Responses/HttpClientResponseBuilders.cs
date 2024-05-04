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

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds the success response of <see cref="HttpClientResponse"/> type.
/// </summary>
public sealed class SuccessHttpClientResponseBuilder :
    HttpClientResponseBuilderBase, IHttpClientResponseBuilder
{
    ///<inheritdoc/>
    public override Type? Type => null;

    ///<inheritdoc/>
    public override bool CanBuild(
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
        => targetStatusCode.IsSuccessStatusCode()
            && resultType is null;

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

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
/// Builds the failure response of <see cref="HttpClientResponse"/> type.
/// </summary>
public sealed class FailureHttpClientResponseBuilder :
    HttpClientResponseBuilderBase, IHttpClientResponseBuilder
{
    ///<inheritdoc/>
    public override Type? Type => null;

    ///<inheritdoc/>
    public override bool CanBuild(
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
        => targetStatusCode.IsFailureStatusCode()
            && resultType is null;

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

        return new HttpClientResponse(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            await httpResponse.BuildExceptionAsync()
            .ConfigureAwait(false));
    }
}

/// <summary>
/// Builds the success response of <see cref="HttpClientResponse{TResult}"/>
/// type with a specific type result.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public sealed class SuccessHttpClientResponseResultBuilder<TResult>
    : HttpClientResponseBuilderBase, IHttpClientResponseResultBuilder<TResult>
    where TResult : notnull
{
    ///<inheritdoc/>
    public override Type? Type => typeof(TResult);

    ///<inheritdoc/>
    public override bool CanBuild(
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
        => targetStatusCode.IsSuccessStatusCode()
            && resultType is not null
            && !resultType.IsValueType
            && !resultType.IsInterface
            && !resultType.IsAsyncEnumerable();

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse<TResult>> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

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
/// Builds the failure response of <see cref="HttpClientResponse{TResult}"/>
/// type with a specific type result.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public sealed class FailureHttpClientResponseResultBuilder<TResult>
    : HttpClientResponseBuilderBase, IHttpClientResponseResultBuilder<TResult>
    where TResult : notnull
{
    ///<inheritdoc/>
    public override Type? Type => typeof(TResult);

    ///<inheritdoc/>
    public override bool CanBuild(
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
        => targetStatusCode.IsFailureStatusCode()
            && resultType is not null
            && !resultType.IsValueType
            && !resultType.IsInterface
            && !resultType.IsAsyncEnumerable();

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse
        <TResult>> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

        return new HttpClientResponse<TResult>(
            httpResponse.StatusCode,
            httpResponse.ReadHttpResponseHeaders(),
            default,
            httpResponse.Version,
            httpResponse.ReasonPhrase,
            await httpResponse.BuildExceptionAsync()
            .ConfigureAwait(false));
    }
}

/// <summary>
/// Builds the success response of <see cref="HttpClientResponse{TResult}"/>
/// type with a specific <see cref="IAsyncEnumerable{T}"/> type result.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public sealed class SuccessHttpClientResponseAsyncResultBuilder<TResult>
    : HttpClientResponseBuilderBase, IHttpClientResponseIAsyncResultBuilder<TResult>
{
    ///<inheritdoc/>
    public override Type? Type => typeof(IAsyncEnumerable<TResult>);

    ///<inheritdoc/>
    public override bool CanBuild(
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
        => targetStatusCode.IsSuccessStatusCode()
            && resultType is not null
            && !resultType.IsValueType
            && resultType.IsInterface
            && resultType.IsAsyncEnumerable();

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse
        <IAsyncEnumerable<TResult>>> BuildAsync(
        HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

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
/// <typeparam name="TResult">Type of the result.</typeparam>
public sealed class FailureHttpClientResponseAsyncResultBuilder<TResult>
     : HttpClientResponseBuilderBase, IHttpClientResponseIAsyncResultBuilder<TResult>
{
    ///<inheritdoc/>
    public override Type? Type => typeof(IAsyncEnumerable<TResult>);

    ///<inheritdoc/>
    public override bool CanBuild(
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
        => targetStatusCode.IsFailureStatusCode()
            && resultType is not null
            && !resultType.IsValueType
            && resultType.IsInterface
            && resultType.IsAsyncEnumerable();

    ///<inheritdoc/>
    public async ValueTask<HttpClientResponse
        <IAsyncEnumerable<TResult>>>
        BuildAsync(HttpResponseMessage httpResponse,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

        return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                httpResponse.StatusCode,
                httpResponse.ReadHttpResponseHeaders(),
                default,
                httpResponse.Version,
                httpResponse.ReasonPhrase,
                await httpResponse.BuildExceptionAsync()
                .ConfigureAwait(false));
    }
}
