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

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds a RestResponse asynchronously using the provided RestResponseContext. Supports cancellation through a token.
/// </summary>
/// <typeparam name="TRestRequest"> The type of the REST request.</typeparam> 
public sealed class RestResponseNoContentComposer<TRestRequest> : IRestResponseComposer<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext<TRestRequest> context) =>
        context.Message.IsSuccessStatusCode
        && (context.Message.Content is null || context.Message.Content.Headers.ContentLength == 0);

    /// <inheritdoc/>
    public ValueTask<RestResponse> ComposeAsync(
        RestResponseContext<TRestRequest> context, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = context.Message;

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a success. " +
                $"Status code: {response.StatusCode} ({response.ReasonPhrase}).");

#pragma warning disable CA2000 // Dispose objects before losing scope
        return new(new RestResponse
        {
            StatusCode = response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = response.Headers.ToElementCollection(),
            Version = response.Version
        });
#pragma warning restore CA2000 // Dispose objects before losing scope
    }
}
