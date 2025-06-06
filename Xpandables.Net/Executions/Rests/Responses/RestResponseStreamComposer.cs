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

using System.Text.Json;

namespace Xpandables.Net.Executions.Rests.Responses;

/// <summary>
/// Composes a stream RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <typeparam name="TRestRequest"> The type of the REST request.</typeparam> 
public sealed class RestResponseStreamComposer<TRestRequest> : IRestResponseComposer<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext<TRestRequest> context) =>
        context.Message.IsSuccessStatusCode
        && context.Request.ResultType is not null
        && context.Request.IsRequestStream;

    /// <inheritdoc/>
    public async ValueTask<RestResponse> ComposeAsync(
        RestResponseContext<TRestRequest> context, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = context.Message;
        JsonSerializerOptions options = context.SerializerOptions;
        TRestRequest request = context.Request;

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a success. " +
                $"Status code: {response.StatusCode} ({response.ReasonPhrase}).");

        try
        {
            Type streamType = request.ResultType!;

            Stream stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            if (stream.Length == 0)
            {
                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version
                };
            }

            object typedResult = stream.DeserializeAsyncEnumerableAsync(streamType, options, cancellationToken);

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