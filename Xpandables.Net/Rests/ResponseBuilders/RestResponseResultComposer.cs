
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
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Collections;
using Xpandables.Net.Rests.Abstractions;

namespace Xpandables.Net.Rests.ResponseBuilders;

/// <summary>
/// Composes a result RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <typeparam name="TResult"> The type of the response payload.</typeparam>
public sealed class RestResponseResultComposer<TResult> : IRestResponseResultComposer<TResult>
    where TResult : notnull
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Message.IsSuccessStatusCode
            && context.Request.ResultType is not null
            && context.Message.Content is not null
            && !context.Request.IsRequestStream;
    }

    /// <inheritdoc/>
    public async ValueTask<RestResponse<TResult>> ComposeAsync(
        RestResponseContext<TResult> context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        HttpResponseMessage response = context.Message;
        JsonSerializerOptions options = context.SerializerOptions;

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a success. " +
                $"Status code: {response.StatusCode} ({response.ReasonPhrase}).");

        try
        {
            string stringContent = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(stringContent))
            {
                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version
                };
            }

            JsonTypeInfo<TResult>? jsonTypeInfo = (JsonTypeInfo<TResult>?)options.GetTypeInfo(typeof(TResult));
            TResult? typedResult = jsonTypeInfo is not null ? JsonSerializer.Deserialize(stringContent, jsonTypeInfo) :
                JsonSerializer.Deserialize<TResult>(stringContent, options);

            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Result = typedResult
            };
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Exception = exception
            };
        }
    }
}
