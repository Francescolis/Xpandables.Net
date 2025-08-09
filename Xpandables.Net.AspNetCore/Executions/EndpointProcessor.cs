
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Processes the execution result asynchronously, setting metadata for the HTTP response based on the execution
/// result.
/// </summary>
public sealed class EndpointProcessor : IEndpointProcessor
{
    /// <inheritdoc/>
    public async Task ProcessAsync(HttpContext httpContext, ExecutionResult executionResult)
    {
        await MetadataSetter(httpContext, executionResult)
            .ConfigureAwait(false);

        if (executionResult.IsSuccessStatusCode)
        {
            await SuccessHandleAsync(httpContext, executionResult)
                .ConfigureAwait(false);

            return;
        }

        await FailureHandleAsync(httpContext, executionResult)
            .ConfigureAwait(false);
    }

    private static async Task MetadataSetter(HttpContext context, ExecutionResult executionResult)
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

    private static Task FailureHandleAsync(HttpContext context, ExecutionResult executionResult)
    {
        context.Response.StatusCode = (int)executionResult.StatusCode;

        bool isDevelopment = context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        ProblemDetails problemDetails = executionResult.StatusCode.IsValidationProblem()
            ? new ValidationProblemDetails(executionResult.ToModelStateDictionary())
            {
                Title = executionResult.Title ?? executionResult.StatusCode.GetAppropriateTitle(),
                Detail = executionResult.Detail ?? executionResult.StatusCode.GetAppropriateDetail(),
                Status = (int)executionResult.StatusCode,
                Instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}",
                Type = isDevelopment ? executionResult.GetType().Name : null,
                Extensions = executionResult.Extensions.ToDictionary()
            }
            : new ProblemDetails()
            {
                Title = executionResult.Title ?? executionResult.StatusCode.GetAppropriateTitle(),
                Detail = executionResult.Detail ?? executionResult.StatusCode.GetAppropriateDetail(),
                Status = (int)executionResult.StatusCode,
                Instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}",
                Type = isDevelopment ? executionResult.GetType().Name : null,
                Extensions = executionResult.Extensions.ToDictionary()
            };

        if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            return problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails
            }).AsTask();
        }

        IResult result = Results.Problem(problemDetails);

        return result.ExecuteAsync(context);
    }

    private static async Task SuccessHandleAsync(HttpContext context, ExecutionResult executionResult)
    {
        if (executionResult.StatusCode.IsCreated())
        {
            IResult resultCreated = (executionResult.Value is not null) switch
            {
                true => Results.Created(
                    executionResult.Location,
                    executionResult.Value),
                _ => Results.Created(executionResult.Location, null)
            };

            await resultCreated
                .ExecuteAsync(context)
                .ConfigureAwait(false);

            return;
        }

        if (executionResult.Value is Stream stream)
        {
            string fileName = executionResult.Headers
                .FirstOrDefault(h => h.Key.Equals("FileName", StringComparison.OrdinalIgnoreCase))
                .Values.FirstOrDefault() ?? "download";

            string contentType = executionResult.Headers
                .FirstOrDefault(h => h.Key.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
                .Values.FirstOrDefault() ?? "application/octet-stream";

            bool inline = executionResult.Headers
                .FirstOrDefault(h => h.Key.Equals("Inline", StringComparison.OrdinalIgnoreCase))
                .Values.FirstOrDefault()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

            string disposition = inline ? "inline" : "attachment";
            context.Response.Headers
                .Append(
                    "Content-Disposition",
                    $"{disposition}; filename={fileName}");

            IResult result = Results.Stream(
                stream,
                contentType,
                fileName);

            await result
                .ExecuteAsync(context)
                .ConfigureAwait(false);

            return;
        }

        if (executionResult.Value is not null)
        {
            if (IsAsyncPagedEnumerable(executionResult.Value))
            {
                await WritePagedEnumerableAsJsonAsync(context, executionResult.Value)
                    .ConfigureAwait(false);

                return;
            }

            await context.Response.WriteAsJsonAsync(
                executionResult.Value,
                executionResult.Value.GetType())
                .ConfigureAwait(false);

            return;
        }

        await context.Response
            .CompleteAsync()
            .ConfigureAwait(false);
    }

    private static bool IsAsyncPagedEnumerable(object value)
    {
        Type valueType = value.GetType();

        return valueType.GetInterfaces()
            .Any(i => i.IsGenericType &&
                      i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>));
    }

    private static async Task WritePagedEnumerableAsJsonAsync(HttpContext context, object pagedEnumerable)
    {
        dynamic dynamicPagedEnumerable = pagedEnumerable;

        object materializedPaged = await AsyncPagedEnumerableExtensions
            .ToListWithPaginationAsync(dynamicPagedEnumerable, CancellationToken.None)
            .ConfigureAwait(false);

        await context.Response
            .WriteAsJsonAsync(materializedPaged)
            .ConfigureAwait(false);
    }
}
