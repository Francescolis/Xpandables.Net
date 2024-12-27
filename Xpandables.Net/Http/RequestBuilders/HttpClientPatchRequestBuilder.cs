﻿
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
using System.Text;
using System.Text.Json;

using Xpandables.Net.Http.Interfaces;

using static Xpandables.Net.Http.Interfaces.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>
/// A builder class for creating HTTP PATCH requests.
/// </summary>
public sealed class HttpClientPatchRequestBuilder :
    HttpClientRequestBuilder<IPatchRequest>
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

        IPatchRequest request = (IPatchRequest)context.Request;

#pragma warning disable CA2000 // Dispose objects before losing scope
        StringContent content = new(
            JsonSerializer.Serialize(
                request.PatchOperations,
                context.SerializerOptions),
            Encoding.UTF8,
            context.Attribute.ContentType);
#pragma warning restore CA2000 // Dispose objects before losing scope

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