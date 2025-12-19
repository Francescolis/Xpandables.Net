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
/// Composes a a NoContent RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
public sealed class RestResponseNoContentComposer : IRestResponseComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Message.IsSuccessStatusCode
        && (context.Message.Content is null || context.Message.Content.Headers.ContentLength == 0);
    }

    /// <inheritdoc/>
    [Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public ValueTask<RestResponse> ComposeAsync(
        RestResponseContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a success. " +
                $"Status code: {context.Message.StatusCode} ({context.Message.ReasonPhrase}).");

        return new(new RestResponse
        {
            StatusCode = context.Message.StatusCode,
            ReasonPhrase = context.Message.ReasonPhrase,
            Headers = context.Message.Headers.ToElementCollection(),
            Version = context.Message.Version
        });
    }
}
