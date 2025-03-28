﻿/*******************************************************************************
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
using System.Net;
using System.Reflection;
using System.Text.Json;

using Xpandables.Net.Executions;

using AsyncEnumerable = Xpandables.Net.Collections.AsyncEnumerable;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating successful async HTTP client responses of 
/// stream result.
/// </summary>
public sealed class HttpResponseSuccessStreamBuilder : IHttpResponseBuilder
{
    private static readonly MethodInfo _doBuildAsync =
        typeof(HttpResponseSuccessStreamBuilder)
        .GetMethod(
            nameof(DoBuildAsync),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <inheritdoc/>
    public Type Type => typeof(ResponseHttp<>);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == Type
            && targetType.GetGenericArguments()[0].IsGenericType
            && targetType.GetGenericArguments()[0].IsInterface
            && targetType.GetGenericArguments()[0]
                .GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)
            && statusCode.IsSuccessStatusCode();

    /// <inheritdoc/>
    public Task<TResponse> BuildAsync<TResponse>(
        ResponseContext context,
        CancellationToken cancellationToken = default)
        where TResponse : HttpResponse
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanBuild(typeof(TResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        Type resultType = typeof(TResponse)
            .GetGenericArguments()[0]
            .GetGenericArguments()[0];

        MethodInfo doBuildAsyncInvokable = _doBuildAsync
            .MakeGenericMethod(resultType);

        return (Task<TResponse>)doBuildAsyncInvokable
            .Invoke(null, [context, cancellationToken])!;
    }

    private static async Task<ResponseHttp<IAsyncEnumerable<TResult>>>
        DoBuildAsync<TResult>(
        ResponseContext context,
        CancellationToken cancellationToken)
    {
        using Stream stream = await context.Message.Content
             .ReadAsStreamAsync(cancellationToken)
             .ConfigureAwait(false);

        IAsyncEnumerable<TResult> results = stream is not null
            ? await JsonSerializer.DeserializeAsync<IAsyncEnumerable<TResult>>(
                stream,
                context.SerializerOptions,
                cancellationToken)
                .ConfigureAwait(false) ?? AsyncEnumerable.Empty<TResult>()
            : AsyncEnumerable.Empty<TResult>();

        return new ResponseHttp<IAsyncEnumerable<TResult>>
        {
            StatusCode = context.Message.StatusCode,
            Headers = context.Message.ToNameValueCollection(),
            Result = results,
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase
        };
    }
}
