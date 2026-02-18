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

namespace System.Rests.ResponseBuilders;

/// <summary>
/// Composes a failure RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <param name="statusCodeExtension">An optional implementation of <see cref="IHttpStatusCodeExtension"/> used to map
/// HTTP status codes to exceptions. If null, the default <see cref="HttpStatusCodeExtension"/> is used.</param>
public sealed class RestResponseFailureComposer(
    IHttpStatusCodeExtension? statusCodeExtension = null) : IRestResponseComposer
{
    private readonly IHttpStatusCodeExtension _statusCodeExtension =
        statusCodeExtension ?? new HttpStatusCodeExtension();
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return !context.Message.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async ValueTask<RestResponse> ComposeAsync(
        RestResponseContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

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
                Exception = _statusCodeExtension.GetException(response.StatusCode, errorContent)
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
