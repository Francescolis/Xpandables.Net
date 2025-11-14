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
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Http.ResponseBuilders;

/// <summary>
/// Composes a stream RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <typeparam name="TResult"> The type of the response payload contained in the stream.</typeparam>
/// <remarks>This implementation returns an <see cref="IAsyncEnumerable{T}"/>.</remarks>
public sealed class RestResponseStreamComposer<TResult> : IRestResponseStreamComposer<TResult>
    where TResult : notnull
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Message.IsSuccessStatusCode
        && context.Request.ResultType is not null
        && context.Request.IsRequestStream;
    }

    /// <inheritdoc/>
    public async ValueTask<RestResponse<IAsyncEnumerable<TResult>>> ComposeAsync(
        RestResponseStreamContext<TResult> context, CancellationToken cancellationToken = default)
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

            JsonTypeInfo<TResult>? typeInfo = (JsonTypeInfo<TResult>?)(options.TypeInfoResolver?.GetTypeInfo(typeof(TResult), options))
                ?? throw new InvalidOperationException(
                    $"{nameof(ComposeAsync)}: The JsonTypeInfo for type {typeof(TResult)} could not be resolved.");

            var asyncPagedResult = response.Content.ReadFromJsonAsAsyncEnumerable(typeInfo, cancellationToken);

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