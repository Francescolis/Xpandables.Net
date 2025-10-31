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
using System.Net;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.ExecutionResults.Collections;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Provides an abstract base for writing execution results to an HTTP response.
/// </summary>
/// <remarks>Implement this class to customize how execution results are written to HTTP responses, such as
/// setting headers, status codes, or response bodies. Derived classes should override the CanWrite and WriteAsync
/// methods to define supported result types and response writing behavior.</remarks>
public abstract class ExecutionResultResponseWriter : IExecutionResultResponseWriter
{
    /// <summary>
    /// When overridden in a derived class, determines whether the specified execution result can be written by this instance.
    /// </summary>
    /// <param name="executionResult">The execution result to evaluate for writability. Cannot be null.</param>
    /// <returns>true if the execution result can be written; otherwise, false.</returns>
    public abstract bool CanWrite(ExecutionResult executionResult);

    /// <summary>
    /// When overridden in a derived class, writes the specified execution result to the HTTP response.
    /// </summary>
    /// <remarks>The default implementation do this : If the execution result indicates an unauthorized status code, the method attempts to set the
    /// 'WWW-Authenticate' header using the available authentication schemes from the request's service provider. This
    /// method does not write a response body.</remarks>
    /// <param name="context">The HTTP context for the current request. Provides access to the response where headers and status code will be
    /// set. Cannot be null.</param>
    /// <param name="executionResult">The result of the execution containing status code, headers, and optional location information to be written to
    /// the response. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public virtual async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(executionResult);

        if (executionResult.Location is not null)
        {
            context.Response.Headers.Location =
                new StringValues(executionResult.Location.ToString());
        }

        foreach (ElementEntry header in executionResult.Headers)
        {
            context.Response.Headers.Append(
                header.Key,
                new StringValues([.. header.Values]));
        }

        if (executionResult.StatusCode == HttpStatusCode.Unauthorized)
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
