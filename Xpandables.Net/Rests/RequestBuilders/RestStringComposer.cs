
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Rests.Abstractions;

using static Xpandables.Net.Rests.RestSettings;

namespace Xpandables.Net.Rests.RequestBuilders;

/// <summary>
/// Composes the request content for a REST API call based on the provided context. It serializes string content and adds
/// it to the request message.
/// </summary>
public sealed class RestStringComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestString
{
    /// <inheritdoc/>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public void Compose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        if ((context.Attribute.Location & Location.Body) != Location.Body
            && context.Attribute.BodyFormat != BodyFormat.String)
        {
            return;
        }

        JsonTypeInfo<TRestRequest>? jsonTypeInfo = (JsonTypeInfo<TRestRequest>?)context.SerializerOptions.GetTypeInfo(typeof(TRestRequest));

        StringContent content = new(
            jsonTypeInfo switch
            {
                not null => JsonSerializer.Serialize(
                ((IRestString)context.Request).GetStringContent(), jsonTypeInfo),
                _ => JsonSerializer.Serialize(
                 ((IRestString)context.Request).GetStringContent(), context.SerializerOptions)
            },
            Encoding.UTF8,
            context.Attribute.ContentType);

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
