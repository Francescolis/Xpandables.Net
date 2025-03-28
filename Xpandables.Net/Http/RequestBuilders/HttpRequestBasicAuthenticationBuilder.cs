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
using System.Net.Http.Headers;

using static Xpandables.Net.Http.MapRequest;

namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>  
/// Builder for HTTP client requests that require Basic Authentication.  
/// </summary>  
public sealed class HttpRequestBasicAuthenticationBuilder : HttpRequestBuilder<IHttpRequestContentBasicAuthentication>
{
    /// <inheritdoc/>         
    public override void Build(RequestContext context)
    {
        if ((context.Attribute.Location & Location.BasicAuth) != Location.BasicAuth)
        {
            return;
        }

        IHttpRequestContentBasicAuthentication request = (IHttpRequestContentBasicAuthentication)context.Request;

        AuthenticationHeaderValue value = request.GetAuthenticationHeaderValue();

        context.Message.Headers.Authorization = value;
    }
}
