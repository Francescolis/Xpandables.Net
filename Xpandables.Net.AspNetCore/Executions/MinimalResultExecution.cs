
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
using System.Net;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Defines a base class for executing minimal results in ASP.NET Core applications.
/// </summary>
public abstract class MinimalResultExecution : IMinimalResultExecution
{
    /// <summary>
    /// When overridden in a derived class, determines whether the execution result can be executed by this instance.
    /// </summary>
    /// <param name="executionResult">The execution result to check.</param>
    /// <returns>True if the execution result can be executed; otherwise, false.</returns>
    public abstract bool CanExecute(ExecutionResult executionResult);

    /// <summary>
    /// When overridden in a derived class, writes the execution result to the HTTP response asynchronously.
    /// </summary>
    /// <remarks>The default implementation sets the response status code, headers, and location if provided.</remarks>
    /// <param name="context">The HTTP context that contains the request and response information.</param>
    /// <param name="executionResult">The execution result that will be written to the response.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual async Task ExecuteAsync(HttpContext context, ExecutionResult executionResult)
    {
        if (executionResult.Location is not null)
        {
            context.Response.Headers.Location =
                new StringValues(executionResult.Location.ToString());
        }

        context.Response.StatusCode = (int)executionResult.StatusCode;

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
