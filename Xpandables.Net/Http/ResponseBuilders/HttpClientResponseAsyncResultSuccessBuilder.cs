
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

namespace Xpandables.Net.Http.ResponseBuilders;

/// <summary>
/// Builds the success response of <see cref="HttpClientResponse{TResult}"/>
/// type with a specific <see cref="IAsyncEnumerable{T}"/> type result.
/// </summary>
public sealed class HttpClientResponseAsyncResultSuccessBuilder
    : HttpClientResponseBuilder, IHttpClientResponseAsyncResultBuilder
{
    ///<inheritdoc/>
    public override Type Type
        => typeof(IAsyncEnumerable<>);

    ///<inheritdoc/>
    public override bool CanBuild(
        Type targetType,
        HttpStatusCode targetStatusCode)
        => targetType.IsGenericType
            && Type == targetType.GetGenericTypeDefinition()
            && targetStatusCode.IsSuccessStatusCode();

    ///<inheritdoc/>
    public async Task<HttpClientResponse<IAsyncEnumerable<TResult>>> BuildAsync<TResult>(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        Stream stream = await context.ResponseMessage.Content
               .ReadAsStreamAsync(cancellationToken)
               .ConfigureAwait(false);

        if (stream is null)
        {
            return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                context.ResponseMessage.StatusCode,
                context.ResponseMessage.ReadHttpResponseHeaders(),
                AsyncEnumerable.Empty<TResult>(),
                context.ResponseMessage.Version,
                context.ResponseMessage.ReasonPhrase,
                default);
        }

        IAsyncEnumerable<TResult> results
            = AsyncEnumerableBuilderAsync(
                stream,
                context.SerializerOptions,
                cancellationToken);

        return new HttpClientResponse<IAsyncEnumerable<TResult>>(
            context.ResponseMessage.StatusCode,
            context.ResponseMessage.ReadHttpResponseHeaders(),
            results,
            context.ResponseMessage.Version,
            context.ResponseMessage.ReasonPhrase,
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
                = new Primitives.Collections.AsyncEnumerable<TResult>(
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
                    {
                        blockingCollection.Add(result, cancellationToken);
                    }
                }

            }, cancellationToken)
                .ContinueWith(t =>
                {
                    blockingCollection.CompleteAdding();

                    if (t.IsFaulted)
                    {
                        t.Exception.ReThrow();
                    }

                    return Task.CompletedTask;

                }, TaskScheduler.Current)
                .ConfigureAwait(false);

            while (await blockingCollectionIterator.MoveNextAsync()
                .ConfigureAwait(false))
            {
                yield return blockingCollectionIterator.Current;
            }
        }
    }
}
