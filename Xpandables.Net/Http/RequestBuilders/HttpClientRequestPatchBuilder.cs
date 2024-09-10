﻿
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
using System.Text;
using System.Text.Json;

using Xpandables.Net.Http.Requests;

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Build the Patch content for a request.
/// </summary>
public sealed class HttpClientRequestPatchBuilder :
    HttpClientRequestBuilder<IHttpRequestPatch>
{
    /// <inheritdoc/>
    public override int Order => 10;

    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        if (context.Attribute.IsNullable
            || (context.Attribute.Location & Location.Body) != Location.Body
            || context.Attribute.BodyFormat != BodyFormat.String)
        {
            return;
        }

        IHttpRequestPatch request = context
            .Request
            .AsRequired<IHttpRequestPatch>();

        StringContent content = new(
            JsonSerializer.Serialize(
                request.PatchOperations,
                context.SerializerOptions),
            Encoding.UTF8,
            context.Attribute.ContentType);

        if (context.RequestMessage.Content is MultipartFormDataContent multipart)
        {
            multipart.Add(content);
        }
        else
        {
            context.RequestMessage.Content = content;
        }
    }
}