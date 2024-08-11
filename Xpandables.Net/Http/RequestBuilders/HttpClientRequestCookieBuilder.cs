
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using Xpandables.Net.Http.Requests;

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Build the cookie for a request.
/// </summary>
public sealed class HttpClientRequestCookieBuilder :
    HttpClientRequestBuilder<IHttpRequestCookie>
{
    /// <inheritdoc/>
    public override int Order => 3;

    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        if ((context.Attribute.Location & Location.Cookie) != Location.Cookie)
        {
            return;
        }

        IHttpRequestCookie request = context
            .Request
            .AsRequired<IHttpRequestCookie>();

        IDictionary<string, object?> cookieSource
             = request.GetCookieSource();

        foreach (KeyValuePair<string, object?> parameter in cookieSource)
        {
            _ = context.RequestMessage.Options
                .TryAdd(parameter.Key, parameter.Value);
        }
    }
}
