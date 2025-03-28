
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
using static Xpandables.Net.Http.MapRequest;

namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>  
/// Builds an HTTP client request with form URL encoded content.  
/// </summary>  
public sealed class HttpRequestFormUrlEncodedBuilder : HttpRequestBuilder<IHttpRequestContentFormUrlEncoded>
{
    ///<inheritdoc/>  
    public override void Build(RequestContext context)
    {
        if ((context.Attribute.Location & Location.Body) == Location.Body
            && context.Attribute.BodyFormat == BodyFormat.FormUrlEncoded)
        {
            IHttpRequestContentFormUrlEncoded request =
                (IHttpRequestContentFormUrlEncoded)context.Request;

            FormUrlEncodedContent content = request.GetFormUrlEncodedContent();

            if (context.Message.Content is MultipartFormDataContent multipart)
            {
                multipart.Add(content);
            }
            else
            {
                context.Message.Content = content;
            }
        }
    }
}
