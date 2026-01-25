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
/// Composes cookies from the request context if the location is set to Cookie. 
/// It adds each cookie to the message options.
/// </summary>
public sealed class RestCookieComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestCookie
{
    /// <inheritdoc/>
    public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if ((context.Attribute.Location & Location.Cookie) != Location.Cookie)
        {
            return ValueTask.CompletedTask;
        }

        IDictionary<string, object?> cookieSource
             = ((IRestCookie)context.Request).GetCookieHeaderValue();

        foreach (KeyValuePair<string, object?> parameter in cookieSource)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = context.Message.Options
                .TryAdd(parameter.Key, parameter.Value);
        }
        return ValueTask.CompletedTask;
    }
}
