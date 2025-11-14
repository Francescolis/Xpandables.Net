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
using static Xpandables.Net.Http.RestSettings;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Composes the HTTP request body as URL-encoded form data based on the request context. It adds the content to the
/// message or a multipart form.
/// </summary>
public sealed class RestFormUrlEncodedComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestFormUrlEncoded
{
    /// <inheritdoc/>
    public void Compose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        if ((context.Attribute.Location & Location.Body) != Location.Body
            || context.Attribute.BodyFormat != BodyFormat.FormUrlEncoded)
        {
            return;
        }

        FormUrlEncodedContent content = ((IRestFormUrlEncoded)context.Request).GetFormUrlEncodedContent();

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
