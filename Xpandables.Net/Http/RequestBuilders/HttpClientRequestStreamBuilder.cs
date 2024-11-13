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
using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>
/// A builder class for creating HTTP client requests with stream content.
/// </summary>
public sealed class HttpClientRequestStreamBuilder :
    HttpClientRequestBuilder<IStreamRequest>
{
    /// <inheritdoc/>
    public override int Order => 9;

    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        if (context.Attribute.IsNullable
            || (context.Attribute.Location & Location.Body) != Location.Body
            || context.Attribute.BodyFormat != BodyFormat.Stream)
        {
            return;
        }

        IStreamRequest request = (IStreamRequest)context.Request;

        StreamContent streamContent = request.GetStreamContent();

        if (context.Message.Content is MultipartFormDataContent content)
        {
            if (context.Request is IHttpRequestMultipart)
            {
                content.Add(streamContent);
            }
            else
            {
                content.Add(streamContent);
                context.Message.Content = content;
            }
        }
        else
        {
            context.Message.Content = streamContent;
        }

    }
}
