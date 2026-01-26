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
using System.Rests.Abstractions;

using static System.Rests.Abstractions.RestSettings;

namespace System.Rests.RequestBuilders;

/// <summary>
/// Composes a multipart HTTP request body if the context specifies a body location and multipart format. It sets the
/// request content accordingly.
/// </summary>
public sealed class RestMultipartComposer : IRestRequestComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request is IRestMultipart
            && (context.Attribute.Location & Location.Body) == Location.Body
            && context.Attribute.BodyFormat == BodyFormat.Multipart;
    }

    /// <inheritdoc/>
    public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (!CanCompose(context))
        {
            throw new InvalidOperationException("The current composer cannot compose the given request context.");
        }

        MultipartFormDataContent content = ((IRestMultipart)context.Request).GetMultipartContent();
        context.Message.Content = content;
        return ValueTask.CompletedTask;
    }
}
