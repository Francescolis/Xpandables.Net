/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Collections;
using System.Rests.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Rests.ResponseBuilders;

/// <summary>
/// Composes a result RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
public sealed class RestResponseResultComposer : IRestResponseComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Message.IsSuccessStatusCode
            && context.Request.ResultType is not null
            && context.Message.Content is not null
            && context.Request is IRestRequestResult;
    }

    /// <inheritdoc/>
    public async ValueTask<RestResponse> ComposeAsync(
        RestResponseContext context, CancellationToken cancellationToken = default)
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

            Type type = ((IRestRequestResult)context.Request).ResultType;

            JsonTypeInfo? jsonTypeInfo = (JsonTypeInfo?)options.GetTypeInfo(type)
                ?? throw new InvalidOperationException(
                    $"{nameof(ComposeAsync)}: The JsonTypeInfo for type {type.Name} could not be found.");

            object? typedResult = JsonSerializer.Deserialize(stringContent, jsonTypeInfo);

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
            when (exception is not ArgumentNullException
                and not OperationCanceledException
                and not InvalidOperationException)
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
