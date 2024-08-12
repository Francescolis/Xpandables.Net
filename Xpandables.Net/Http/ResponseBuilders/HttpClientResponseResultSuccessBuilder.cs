
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
using System.Net;
using System.Text.Json;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Http.ResponseBuilders;

/// <summary>
/// Builds the success response of <see cref="HttpClientResponse{TResult}"/>
/// type with a specific type result.
/// </summary>
public sealed class HttpClientResponseResultSuccessBuilder
    : HttpClientResponseBuilder, IHttpClientResponseResultBuilder
{
    ///<inheritdoc/>
    public override Type Type => typeof(HttpClientResponse<>);

    ///<inheritdoc/>
    public override bool CanBuild(
        Type targetType,
        HttpStatusCode targetStatusCode)
        => targetType.IsGenericType
            && Type == targetType.GetGenericTypeDefinition()
            && targetStatusCode.IsSuccessStatusCode();

    ///<inheritdoc/>
    public async Task<HttpClientResponse<TResult>> BuildAsync<TResult>(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        using Stream stream = await context.ResponseMessage.Content
             .ReadAsStreamAsync(cancellationToken)
             .ConfigureAwait(false);

        if (stream is null)
        {
            return new HttpClientResponse<TResult>(
                context.ResponseMessage.StatusCode,
                context.ResponseMessage.ReadHttpResponseHeaders(),
                default,
                context.ResponseMessage.Version,
                context.ResponseMessage.ReasonPhrase,
                default);
        }

        TResult? result = await JsonSerializer
            .DeserializeAsync<TResult>(
                stream,
                context.SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        return new HttpClientResponse<TResult>(
            context.ResponseMessage.StatusCode,
            context.ResponseMessage.ReadHttpResponseHeaders(),
            result,
            context.ResponseMessage.Version,
            context.ResponseMessage.ReasonPhrase,
            default);
    }
}
