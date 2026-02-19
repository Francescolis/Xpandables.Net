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

internal static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        internal JsonSerializerOptions GetJsonSerializerOptions()
        {
			IOptions<JsonOptions> optionsProvider = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>();
            return optionsProvider.Value.SerializerOptions;
        }

        internal JsonSerializerOptions GetMvcJsonSerializerOptions()
        {
			IOptions<Mvc.JsonOptions> optionsProvider = context.RequestServices.GetRequiredService<IOptions<Mvc.JsonOptions>>();
            return optionsProvider.Value.JsonSerializerOptions;
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
        internal JsonSerializerOptions GetJsonSerializerOptionsOrDefault()
        {
			IOptions<JsonOptions>? optionsProvider = context.RequestServices.GetService<IOptions<JsonOptions>>();
			JsonSerializerOptions options = optionsProvider?.Value?.SerializerOptions ?? JsonSerializerOptions.Default;
            //options.MakeReadOnly(true);
            return options;
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Default")]
        internal JsonSerializerOptions GetMvcJsonSerializerOptionsOrDefault()
        {
			IOptions<Mvc.JsonOptions>? optionsProvider = context.RequestServices.GetService<IOptions<Mvc.JsonOptions>>();
			JsonSerializerOptions options = optionsProvider?.Value?.JsonSerializerOptions ?? JsonSerializerOptions.Default;
            //options.MakeReadOnly(true);
            return options;
        }

        internal string? GetContentType()
        {
            return context.GetEndpoint() switch
            {
                Endpoint endpoint => endpoint.Metadata
                    .GetMetadata<IProducesResponseTypeMetadata>()?.ContentTypes
                    .FirstOrDefault(),
                _ => context.Response.GetTypedHeaders().ContentType?.ToString()
            };
        }

        internal string GetContentType(string defaultContentType)
        {
            return context.GetContentType() ?? defaultContentType;
        }

        internal Encoding GetEncoding()
        {
			string? contentType = context.GetContentType();
            return Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType).Encoding ?? Encoding.UTF8;
        }
    }
}