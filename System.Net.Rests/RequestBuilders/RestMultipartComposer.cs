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
using static System.Net.Rests.RestSettings;

namespace System.Net.Rests.RequestBuilders;

/// <summary>
/// Composes a multipart HTTP request body if the context specifies a body location and multipart format. It sets the
/// request content accordingly.
/// </summary>
public sealed class RestMultipartComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestMultipart
{
    /// <inheritdoc/>
    public void Compose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        if ((context.Attribute.Location & Location.Body) != Location.Body
            || context.Attribute.BodyFormat != BodyFormat.Multipart)
        {
            return;
        }

        MultipartFormDataContent content = ((IRestMultipart)context.Request).GetMultipartContent();
        context.Message.Content = content;
    }
}
