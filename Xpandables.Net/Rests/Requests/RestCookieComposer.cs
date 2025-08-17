
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
using static Xpandables.Net.Rests.Rest;

namespace Xpandables.Net.Rests.Requests;

/// <summary>
/// Composes cookies from the request context if the location is set to Cookie. 
/// It adds each cookie to the message options.
/// </summary>
public sealed class RestCookieComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestCookie
{
    /// <inheritdoc/>
    public void Compose(RestRequestContext<TRestRequest> context)
    {
        if ((context.Attribute.Location & Location.Cookie) != Location.Cookie)
        {
            return;
        }

        IDictionary<string, object?> cookieSource
             = context.Request.GetCookieHeaderValue();

        foreach (KeyValuePair<string, object?> parameter in cookieSource)
        {
            _ = context.Message.Options
                .TryAdd(parameter.Key, parameter.Value);
        }
    }
}
