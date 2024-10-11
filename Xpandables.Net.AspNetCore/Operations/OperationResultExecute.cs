
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
using Xpandables.Net.Operations.Executors;

namespace Xpandables.Net.Operations;

/// <summary>
/// Executes an operation result by setting the appropriate metadata and 
/// invoking the corresponding executor.
/// </summary>
public sealed class OperationResultExecute : IOperationResultExecute
{
    /// <summary>
    /// Executes the operation result asynchronously.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="operationResult">The operation result to execute.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync(
        HttpContext httpContext,
        IOperationResult operationResult)
    {
        await MetadataSetter(httpContext, operationResult)
            .ConfigureAwait(false);

        IOperationResultExecutor executor = httpContext.RequestServices
            .GetServices<IOperationResultExecutor>()
            .FirstOrDefault(executor => executor.CanExecute(operationResult))
            ?? throw new InvalidOperationException(
                "No executor found for the operation result.");

        await executor
            .ExecuteAsync(httpContext, operationResult)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the metadata for the HTTP response based on the operation result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="operationResult">The operation result.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static async Task MetadataSetter(
        HttpContext context,
        IOperationResult operationResult)
    {
        if (operationResult.Location is not null)
        {
            context.Response.Headers.Location =
                new StringValues(operationResult.Location.ToString());
        }

        context.Response.StatusCode = (int)operationResult.StatusCode;

        foreach (ElementEntry header in operationResult.Headers)
        {
            context.Response.Headers.Append(
                header.Key,
                new StringValues([.. header.Values]));
        }

        if (operationResult.StatusCode == HttpStatusCode.Unauthorized)
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
                     requestSchemes.FirstOrDefault() ??
                     defaultScheme ??
                     allSchemes.FirstOrDefault();

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
