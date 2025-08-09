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

using Xpandables.Net.Executions;


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

using Xpandables.Net.Rests;

namespace Xpandables.Net.Rests.Responses;

/// <summary>
/// Composes a failure RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <typeparam name="TRestRequest"> The type of the REST request.</typeparam> 
public sealed class RestResponseFailureComposer<TRestRequest> : IRestResponseComposer<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <inheritdoc/>>
    public bool CanCompose(RestResponseContext<TRestRequest> context) =>
        context.Message.IsSuccessStatusCode == false;

    /// <inheritdoc/>
    public async ValueTask<RestResponse> ComposeAsync(
        RestResponseContext<TRestRequest> context, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = context.Message;

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a failure. " +
                $"Status code: {response.StatusCode} ({response.ReasonPhrase}).");

        try
        {
            string? errorContent = default;
            if (response.Content is not null)
            {
                errorContent = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            errorContent = $"Response status code does not indicate success: " +
                $"{(int)response.StatusCode} ({response.ReasonPhrase}). {errorContent}";

            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Exception = response.StatusCode.GetAppropriateException(errorContent)
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
