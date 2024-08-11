
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
using System.Net.Http.Headers;
using System.Text;

using Xpandables.Net.Http.Requests;

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Build the basic authentication for a request.
/// </summary>
public sealed class HttpClientRequestBasicAuthBuilder :
    HttpClientRequestBuilder<IHttpRequestBasicAuth>
{
    /// <inheritdoc/>
    public override int Order => 5;

    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        if ((context.Attribute.Location & Location.BasicAuth) != Location.BasicAuth)
        {
            return;
        }

        IHttpRequestBasicAuth request = context
            .Request
            .AsRequired<IHttpRequestBasicAuth>();

        string basicContent = request.GetBasicContent();
        byte[] credentials = Encoding.UTF8.GetBytes(basicContent);
        string base64Credentials = Convert.ToBase64String(credentials);

        context.RequestMessage.Headers.Authorization
            = new AuthenticationHeaderValue(
                "Basic",
                base64Credentials);
    }
}
