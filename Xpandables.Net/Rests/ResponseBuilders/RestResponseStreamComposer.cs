
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

using Xpandables.Net.Collections;
using Xpandables.Net.Collections.Generic;
using Xpandables.Net.Collections.Generic.Extensions;
using Xpandables.Net.Rests.Abstractions;

namespace Xpandables.Net.Rests.ResponseBuilders;

/// <summary>
/// Composes a stream RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <typeparam name="TResult"> The type of the response payload contained in the stream.</typeparam>
/// <remarks>This implementation returns an <see cref="IAsyncPagedEnumerable{T}"/>.</remarks>
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
    public async ValueTask<RestResponse<IAsyncPagedEnumerable<TResult>>> ComposeAsync(
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

            var asyncPagedResult = response.Content.ReadFromJsonAsAsyncPagedEnumerable<TResult>(options, cancellationToken);

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