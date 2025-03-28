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
using System.Net;
using System.Reflection;
using System.Text.Json;

using Xpandables.Net.Executions;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating successful HTTP client responses of specific type result.
/// </summary>
public sealed class HttpResponseSuccessResultBuilder : IHttpResponseBuilder
{
    private static readonly MethodInfo _doBuildAsync =
        typeof(HttpResponseSuccessResultBuilder)
        .GetMethod(
            nameof(DoBuildAsync),
            BindingFlags.NonPublic | BindingFlags.Static)!;
    /// <inheritdoc/>
    public Type Type => typeof(ResponseHttp<>);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        return targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == Type
            && statusCode.IsSuccessStatusCode();
    }

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

        Type resultType = typeof(TResponse).GetGenericArguments().First();

        MethodInfo doBuildAsyncInvokable = _doBuildAsync
            .MakeGenericMethod(resultType);

        return (Task<TResponse>)doBuildAsyncInvokable
            .Invoke(null, [context, cancellationToken])!;
    }

    private static async Task<ResponseHttp<TResult>>
       DoBuildAsync<TResult>(
       ResponseContext context,
       CancellationToken cancellationToken)
    {
        using Stream stream = await context.Message.Content
             .ReadAsStreamAsync(cancellationToken)
             .ConfigureAwait(false);

        TResult? result = stream is not null
            ? await JsonSerializer.DeserializeAsync<TResult>(
                stream, context.SerializerOptions, cancellationToken)
                .ConfigureAwait(false)
            : default;

        return new ResponseHttp<TResult>(
            context.Message.StatusCode,
            context.Message.ToNameValueCollection(),
            result,
            null,
            context.Message.Version,
            context.Message.ReasonPhrase);
    }
}
