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
using System.Net.Http.Headers;
using System.Rests.Abstractions;

using static System.Rests.Abstractions.RestSettings;

namespace System.Rests.RequestBuilders;

/// <summary>
/// Composes the authorization header for basic authentication in a REST request. It checks the context for basic
/// authentication location.
/// </summary>
public sealed class RestBasicAuthComposer : IRestRequestComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request is IRestBasicAuthentication
            && (context.Attribute.Location & Location.BasicAuth) == Location.BasicAuth;
    }

    /// <inheritdoc/>
    public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (!CanCompose(context))
            throw new InvalidOperationException("The current composer cannot compose the given request context.");

        AuthenticationHeaderValue value = ((IRestBasicAuthentication)context.Request).GetAuthenticationHeaderValue();

        context.Message.Headers.Authorization = value;
        return ValueTask.CompletedTask;
    }
}
