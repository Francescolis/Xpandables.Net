
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
using Xpandables.Net.Rests;

using static Xpandables.Net.Rests.Rest;

namespace Xpandables.Net.Rests.Requests;

/// <summary>
/// Composes the HTTP request body as a byte array based on the provided context. It handles both multipart and single
/// content types.
/// </summary>
public sealed class RestByteArrayComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestByteArray
{
    /// <inheritdoc/>
    public void Compose(RestRequestContext<TRestRequest> context)
    {
        if ((context.Attribute.Location & Location.Body) != Location.Body
             || context.Attribute.BodyFormat != BodyFormat.ByteArray)
        {
            return;
        }

        ByteArrayContent byteArray = context.Request.GetByteArrayContent();

        if (context.Message.Content is MultipartFormDataContent multipart)
        {
            multipart.Add(byteArray);
        }
        else
        {
            context.Message.Content = byteArray;
        }
    }
}
