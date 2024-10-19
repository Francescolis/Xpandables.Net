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
using Xpandables.Net.Collections;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>
/// A builder class for constructing HTTP client request headers.
/// </summary>
public sealed class HttpClientRequestHeaderBuilder :
    HttpClientRequestBuilder<IHttpRequestHeader>
{
    /// <inheritdoc/>
    public override int Order => 4;

    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        if ((context.Attribute.Location & Location.Header) != Location.Header)
        {
            return;
        }

        IHttpRequestHeader request = (IHttpRequestHeader)context.Request;

        ElementCollection headerSource = request.GetHeaders();

        if (request.GetHeaderModelName() is string modelName)
        {
            string headerValue = string.Join(
                ";",
                headerSource
                    .Select(x => $"{x.Key},{string.Join(",", x.Values)}"));

            context.Message
                .Headers
                .Add(modelName, headerValue);
        }
        else
        {
            foreach (ElementEntry parameter in headerSource)
            {
                _ = context.Message
                        .Headers
                        .Remove(parameter.Key);

                context.Message
                    .Headers
                    .Add(parameter.Key, parameter.Values);
            }
        }
    }
}
