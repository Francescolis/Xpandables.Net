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
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for accessing JSON serialization options and content type information from an <see
/// cref="HttpContext"/> instance.
/// </summary>
/// <remarks>These extension methods simplify retrieval of request-specific serialization settings and content
/// type metadata within ASP.NET Core applications. Methods in this class can be used to obtain configured <see
/// cref="JsonSerializerOptions"/> for the current request, as well as to determine the content type associated with the
/// current endpoint or response. All methods are intended for use within the scope of an HTTP request and rely on
/// services and metadata available in the <see cref="HttpContext"/>.</remarks>
public static class HttpContextExtensions
{
	extension(HttpContext context)
	{
		/// <summary>
		/// Retrieves the configured <see cref="JsonSerializerOptions"/> for the current request context.
		/// </summary>
		/// <remarks>Use this method to obtain the serialization options applied to JSON operations within
		/// the current request scope. The returned options reflect any customizations made via dependency injection or
		/// application configuration.</remarks>
		/// <returns>The <see cref="JsonSerializerOptions"/> instance associated with the current request.</returns>
		public JsonSerializerOptions GetJsonSerializerOptions()
		{
			IOptions<JsonOptions> optionsProvider = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>();
			return optionsProvider.Value.SerializerOptions;
		}

		/// <summary>
		/// Retrieves the configured Mvc <see cref="JsonSerializerOptions"/> for the current request context.
		/// </summary>
		/// <remarks>Use this method to obtain the serialization options applied to JSON operations within
		/// the current request scope. The returned options reflect any customizations made via dependency injection or
		/// application configuration.</remarks>
		/// <returns>The Mvc <see cref="JsonSerializerOptions"/> instance associated with the current request.</returns>
		public JsonSerializerOptions GetMvcJsonSerializerOptions()
		{
			IOptions<Mvc.JsonOptions> optionsProvider = context.RequestServices.GetRequiredService<IOptions<Mvc.JsonOptions>>();
			return optionsProvider.Value.JsonSerializerOptions;
		}

		/// <summary>
		/// Retrieves the configured <see cref="JsonSerializerOptions"/> for the current request, or returns the default
		/// options if none are configured.
		/// </summary>
		/// <remarks>The returned <see cref="JsonSerializerOptions"/> instance is made read-only to
		/// prevent further modification. This method may require dynamic code and unreferenced code due to its use of
		/// <see cref="JsonSerializerOptions.Default"/>.</remarks>
		/// <returns>A read-only <see cref="JsonSerializerOptions"/> instance representing the serializer options for the current
		/// request. If no options are configured, the default options are returned.</returns>
		[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
		[RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
		public JsonSerializerOptions GetJsonSerializerOptionsOrDefault()
		{
			IOptions<JsonOptions>? optionsProvider = context.RequestServices.GetService<IOptions<JsonOptions>>();
			JsonSerializerOptions options = optionsProvider?.Value?.SerializerOptions ?? JsonSerializerOptions.Default;
			//options.MakeReadOnly(true);
			return options;
		}

		/// <summary>
		/// Retrieves the configured Mvc <see cref="JsonSerializerOptions"/> for the current request, or returns the default
		/// options if none are configured.
		/// </summary>
		/// <remarks>The returned Mvc <see cref="JsonSerializerOptions"/> instance is made read-only to
		/// prevent further modification. This method may require dynamic code and unreferenced code due to its use of
		/// <see cref="JsonSerializerOptions.Default"/>.</remarks>
		/// <returns>A read-only Mvc <see cref="JsonSerializerOptions"/> instance representing the serializer options for the current
		/// request. If no options are configured, the default options are returned.</returns>
		[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
		[RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
		public JsonSerializerOptions GetMvcJsonSerializerOptionsOrDefault()
		{
			IOptions<Mvc.JsonOptions>? optionsProvider = context.RequestServices.GetService<IOptions<Mvc.JsonOptions>>();
			JsonSerializerOptions options = optionsProvider?.Value?.JsonSerializerOptions ?? JsonSerializerOptions.Default;
			//options.MakeReadOnly(true);
			return options;
		}

		/// <summary>
		/// Retrieves the content type associated with the current HTTP request endpoint or response.
		/// </summary>
		/// <remarks>This method first attempts to obtain the content type from the endpoint's metadata.
		/// If no endpoint metadata is present, it returns the content type from the response headers. The returned
		/// value may be null if neither source specifies a content type.</remarks>
		/// <returns>A string representing the content type if available; otherwise, null.</returns>
		public string? GetContentType()
		{
			return context.GetEndpoint() switch
			{
				Endpoint endpoint => endpoint.Metadata
					.GetMetadata<IProducesResponseTypeMetadata>()?.ContentTypes
					.FirstOrDefault(),
				_ => context.Response.GetTypedHeaders().ContentType?.ToString()
			};
		}

		/// <summary>
		/// Retrieves the content type from the current context, or returns the specified default content type if none
		/// is set.
		/// </summary>
		/// <param name="defaultContentType">The content type to return if the context does not specify one. This value should be a valid MIME type
		/// string.</param>
		/// <returns>A string representing the content type from the context, or the specified default content type if the
		/// context does not provide one.</returns>
		public string GetContentType(string defaultContentType)
		{
			return context.GetContentType() ?? defaultContentType;
		}

		/// <summary>
		/// Retrieves the character encoding specified in the current HTTP context's Content-Type header.
		/// </summary>
		/// <remarks>Use this method to determine the appropriate encoding for reading or writing HTTP
		/// request or response bodies based on the Content-Type header. If the header does not specify a charset, UTF-8
		/// is used by default, which is the recommended encoding for web content.</remarks>
		/// <returns>An <see cref="Encoding"/> representing the character encoding from the Content-Type header. Returns <see
		/// cref="Encoding.UTF8"/> if no encoding is specified.</returns>
		public Encoding GetEncoding()
		{
			string? contentType = context.GetContentType();
			return Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType).Encoding ?? Encoding.UTF8;
		}
	}
}
