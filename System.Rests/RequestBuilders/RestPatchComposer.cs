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
using System.Diagnostics.CodeAnalysis;
using System.Rests.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using static System.Rests.Abstractions.RestSettings;

namespace System.Rests.RequestBuilders;

/// <summary>
/// Composes the HTTP request body for a PATCH operation based on the provided context. It serializes patch operations
/// into a StringContent.
/// </summary>
public sealed class RestPatchComposer : IRestRequestComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request is IRestPatch
            && (context.Attribute.Location & Location.Body) == Location.Body
            && context.Attribute.BodyFormat == BodyFormat.String
            && context.Request is IRestPatch;
    }

    /// <inheritdoc/>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (!CanCompose(context))
        {
            throw new InvalidOperationException("The current composer cannot compose the given request context.");
        }

        JsonSerializerOptions options = context.SerializerOptions;

        if (options.TypeInfoResolverChain.FirstOrDefault(
            resolver => resolver == PatchOperationJsonContext.Default) is null)
        {
            options.TypeInfoResolverChain.Add(PatchOperationJsonContext.Default);
        }

        JsonTypeInfo jsonTypeInfo = options.GetTypeInfo(typeof(PatchOperation));

        StringContent content = new(
            JsonSerializer.Serialize(
                ((IRestPatch)context.Request).PatchOperations,
                jsonTypeInfo),
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
        return ValueTask.CompletedTask;
    }
}
