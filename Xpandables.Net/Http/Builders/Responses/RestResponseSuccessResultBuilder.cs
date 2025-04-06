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
using System.Text.Json;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds a successful REST response asynchronously from a given context. 
/// Validates response type and status code before deserialization.
/// </summary>
public sealed class RestResponseSuccessResultBuilder : IRestResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(RestResponse<>);

    /// <inheritdoc/>
    public async Task<TRestResponse> BuildAsync<TRestResponse>(
        RestResponseContext context, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract
    {
        if (!CanBuild(typeof(TRestResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        Type resultType = typeof(TRestResponse).GetGenericArguments()[0];

        using Stream stream = await context.Message.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await JsonSerializer
            .DeserializeAsync(stream, resultType, context.SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        var response = new RestResponse
        {
            StatusCode = context.Message.StatusCode,
            Headers = context.Message.Headers.ToElementCollection(),
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase,
            Result = result
        };

        return (TRestResponse)response.ToRestResponse(resultType);
    }

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        return targetType.IsGenericType
            && targetType.GetGenericTypeDefinition() == Type
            && !targetType.GenericTypeArguments[0].IsAsyncEnumerable()
            && statusCode.IsSuccessStatusCode();
    }
}
