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
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating successful async HTTP client responses of 
/// specific type result.
/// </summary>
public sealed class HttpClientResponseSuccessAsyncResultBuilder : IHttpClientResponseBuilder
{
    private readonly MethodInfo _builderResultAsyncMethod =
        typeof(HttpClientResponseSuccessAsyncResultBuilder)
        .GetMethod(
            nameof(BuilderResultAsync),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <inheritdoc/>
    public Type Type => typeof(HttpClientResponse<>);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == Type
            && targetType.GetGenericArguments()[0].IsGenericType
            && targetType.GetGenericArguments()[0].IsInterface
            && targetType.GetGenericArguments()[0]
                .GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)
            && (int)statusCode is >= 200 and <= 299;

    /// <inheritdoc/>
    public async Task<TResponse> BuildAsync<TResponse>(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
        where TResponse : HttpClientResponse
    {
        if (!CanBuild(typeof(TResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        Type resultType = typeof(TResponse)
            .GetGenericArguments()[0]
            .GetGenericArguments()[0];

        using Stream stream = await context.Message.Content
             .ReadAsStreamAsync(cancellationToken)
             .ConfigureAwait(false);

        MethodInfo builderMethod = _builderResultAsyncMethod
            .MakeGenericMethod(resultType);

        MethodInfo asyncEmpty = ElementCollectionExtensions
            .AsyncArrayEmptyMethod
            .MakeGenericMethod(resultType);

        object? results = stream is not null
            ? builderMethod.Invoke(
                null, [stream, context.SerializerOptions, cancellationToken])
            : asyncEmpty.Invoke(null, null);

        return (TResponse)Activator.CreateInstance(
            typeof(TResponse),
            context.Message.StatusCode,
            context.Message.ToNameValueCollection(),
            results,
            null,
            context.Message.Version,
            context.Message.ReasonPhrase)!;
    }

    private static async IAsyncEnumerable<TResult> BuilderResultAsync<TResult>(
        Stream stream,
        JsonSerializerOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using BlockingCollection<TResult> blockingCollection = [];

        await using IAsyncEnumerator<TResult> blockingCollectionIterator
            = new AsyncEnumerable<TResult>(
            blockingCollection.GetConsumingEnumerable(cancellationToken))
            .GetAsyncEnumerator(cancellationToken);

        _ = await Task.Run(async () =>
        {
            await foreach (TResult? element in JsonSerializer
                .DeserializeAsyncEnumerable<TResult>(
                stream,
                options,
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
                throw t.Exception;
            }

            return Task.CompletedTask;

        }, TaskScheduler.Current)
        .ConfigureAwait(false);

        while (await blockingCollectionIterator
            .MoveNextAsync()
            .ConfigureAwait(false))
        {
            yield return blockingCollectionIterator.Current;
        }

    }
}
