/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// Composes the request content for a REST API call based on the provided context. It serializes string content and adds
/// it to the request message.
/// </summary>
public sealed class RestStringComposer : IRestRequestComposer
{
	/// <inheritdoc/>
	public bool CanCompose(RestRequestContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		return context.Request is IRestString
			&& (context.Attribute.Location & Location.Body) == Location.Body
			&& context.Attribute.BodyFormat == BodyFormat.String;
	}
	/// <inheritdoc/>
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
	public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(context);
		cancellationToken.ThrowIfCancellationRequested();

		if (!CanCompose(context))
		{
			throw new InvalidOperationException("The current composer cannot compose the given request context.");
		}

		object contentObject = ((IRestString)context.Request).GetStringContent();
		Type contentType = contentObject.GetType();
		Type requestType = context.Request.GetType();
		string serializedContent;

		if (requestType == contentType)
		{
			// the anonymous type is the request type, so we can serialize it directly
			JsonTypeInfo? jsonTypeInfo = (JsonTypeInfo?)context.SerializerOptions.GetTypeInfo(requestType)
				?? throw new InvalidOperationException(
					$"JsonTypeInfo for type '{requestType}' is not registered in the JsonSerializerOptions.");
			serializedContent = JsonSerializer.Serialize(contentObject, jsonTypeInfo);
		}
		else
		{
			// the anonymous type is not the request type, we serialize it as an object
			serializedContent = JsonSerializer.Serialize(contentObject, context.SerializerOptions);
		}

		StringContent content = new(
			serializedContent,
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
