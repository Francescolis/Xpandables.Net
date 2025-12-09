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
using System.Collections;
using System.Net;
using System.Results;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides functionality to write HTTP response headers for a result in an ASP.NET Core context.
/// </summary>
/// <remarks>This class sets the response content type, appends custom headers, and handles authentication-related
/// headers when the result indicates an unauthorized status. It is typically used to format the HTTP response
/// for GraphQL or similar APIs based on the result.</remarks>
public sealed class ResultHeaderWriter : IResultHeaderWriter
{
    /// <inheritdoc/>
    public async Task WriteAsync(HttpContext context, Result result)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(result);

        context.Response.ContentType ??= context.GetContentType("application/json; charset=utf-8");
        context.Response.StatusCode = (int)result.StatusCode;

        if (result.Location is not null)
        {
            context.Response.Headers.Location =
                new StringValues(result.Location.ToString());
        }

        foreach (ElementEntry header in result.Headers)
        {
            context.Response.Headers.Append(
                header.Key,
                new StringValues([.. header.Values]));
        }

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (context.RequestServices.GetService<IAuthenticationSchemeProvider>()
                is { } schemeProvider)
            {
                IEnumerable<AuthenticationScheme> requestSchemes =
                    await schemeProvider
                        .GetRequestHandlerSchemesAsync()
                        .ConfigureAwait(false);

                AuthenticationScheme? defaultScheme =
                    await schemeProvider
                        .GetDefaultAuthenticateSchemeAsync()
                        .ConfigureAwait(false);

                IEnumerable<AuthenticationScheme> allSchemes =
                    await schemeProvider
                        .GetAllSchemesAsync()
                        .ConfigureAwait(false);

                AuthenticationScheme? scheme =
                     requestSchemes.FirstOrDefault()
                         ?? defaultScheme
                         ?? allSchemes.FirstOrDefault();

                if (scheme is not null)
                {
                    context.Response.Headers.Append(
                        HeaderNames.WWWAuthenticate,
                        scheme.Name);
                }
            }
        }
    }
}
