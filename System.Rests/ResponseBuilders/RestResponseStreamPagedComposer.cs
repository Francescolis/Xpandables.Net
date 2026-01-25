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
using System.Net.Http.Json;
using System.Rests.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Rests.ResponseBuilders;

/// <summary>
/// Composes a paged stream RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <remarks>
/// This implementation returns an <see cref="IAsyncPagedEnumerable{T}"/>.
/// For AOT compatibility, the request should implement <see cref="IRestStreamPagedDeserializer"/>.
/// </remarks>
public sealed class RestResponseStreamPagedComposer : IRestResponseComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Message.IsSuccessStatusCode
        && context.Request.ResultType is not null
        && context.Request is IRestRequestStreamPaged;
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
            await Task.Yield();

            object? asyncPagedResult;

            // Use AOT-compatible deserializer if available
            if (context.Request is IRestStreamPagedDeserializer deserializer)
            {
                asyncPagedResult = deserializer.DeserializeAsAsyncPagedEnumerable(
                    response.Content,
                    options,
                    cancellationToken);
            }
            else
            {
                // Fallback for non-AOT scenarios using JsonTypeInfo
                Type type = ((IRestRequestStreamPaged)context.Request).ResultType;
                JsonTypeInfo? typeInfo = options.TypeInfoResolver?.GetTypeInfo(type, options)
                    ?? throw new InvalidOperationException(
                        $"{nameof(ComposeAsync)}: The JsonTypeInfo for type {type.Name} could not be resolved. " +
                        $"For AOT compatibility, implement {nameof(IRestStreamPagedDeserializer)} on your request type.");

                asyncPagedResult = ReadFromJsonAsyncPagedEnumerableInternal(response.Content, typeInfo, cancellationToken);
            }

            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Result = asyncPagedResult
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

#pragma warning disable IL2026 // Suppress trimming warning for fallback scenario
#pragma warning disable IL3050 // Suppress AOT warning for fallback scenario
    private static object? ReadFromJsonAsyncPagedEnumerableInternal(
        HttpContent content,
        JsonTypeInfo jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        var method = typeof(HttpContentExtensions)
            .GetMethod(nameof(HttpContentExtensions.ReadFromJsonAsAsyncPagedEnumerable),
            [typeof(HttpContent), typeof(JsonSerializerOptions), typeof(PaginationStrategy), typeof(CancellationToken)])
            ?? throw new InvalidOperationException(
                $"Could not find method {nameof(HttpContentExtensions.ReadFromJsonAsAsyncPagedEnumerable)}. " +
                $"For AOT compatibility, implement {nameof(IRestStreamPagedDeserializer)} on your request type.");

        var genericMethod = method.MakeGenericMethod(jsonTypeInfo.Type);
        return genericMethod.Invoke(null, [content, jsonTypeInfo.Options, PaginationStrategy.None, cancellationToken]);
    }
#pragma warning restore IL3050
#pragma warning restore IL2026
}