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
using System.Text.Json;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating successful HTTP client responses of specific type result.
/// </summary>
public sealed class HttpClientResponseSuccessResultBuilder : IHttpClientResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(HttpClientResponse<>);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == Type
            && (int)statusCode is >= 200 and <= 299;

    /// <inheritdoc/>
    public async Task<TResponse> BuildAsync<TResponse>(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
        where TResponse : HttpClientResponse
    {
        if (!CanBuild(typeof(TResponse), context.ResponseMessage.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        Type resultType = typeof(TResponse).GetGenericArguments().First();

        using Stream stream = await context.ResponseMessage.Content
             .ReadAsStreamAsync(cancellationToken)
             .ConfigureAwait(false);

        object? result = stream is not null
            ? await JsonSerializer.DeserializeAsync(
                stream, resultType, context.SerializerOptions, cancellationToken)
                .ConfigureAwait(false)
            : null;

        return (TResponse)Activator.CreateInstance(
            typeof(TResponse),
            context.ResponseMessage.StatusCode,
            context.ResponseMessage.ToNameValueCollection(),
            result,
            context.ResponseMessage.Version,
            context.ResponseMessage.ReasonPhrase)!;
    }
}
