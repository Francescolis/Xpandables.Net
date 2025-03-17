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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions.Minimals;

namespace Xpandables.Net.Executions;

/// <summary>  
/// Provides extension methods for converting execution results.  
/// </summary>  
public static class ExecutionResultExtensions
{
    /// <summary>  
    /// Converts the specified execution result to an <see cref="ModelStateDictionary"/>.
    /// </summary>  
    /// <param name="executionResult">The execution result to convert.</param>
    /// <returns>A <see cref="ModelStateDictionary"/> containing the converted  
    /// elements.</returns>  
    public static ModelStateDictionary ToModelStateDictionary(
        this IExecutionResult executionResult)
    {
        ModelStateDictionary modelState = new();
        foreach (ElementEntry entry in executionResult.Errors)
        {
            foreach (string value in entry.Values)
            {
                modelState.AddModelError(entry.Key, value);
            }
        }

        return modelState;
    }

    /// <summary>  
    /// Converts a <see cref="ModelStateDictionary"/> to an <see cref="IExecutionResult"/>.  
    /// </summary>  
    /// <param name="modelState">The model state dictionary to convert.</param>  
    /// <param name="statusCode">The HTTP status code to use for the execution 
    /// result. Defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>  
    /// <returns>An <see cref="IExecutionResult"/> representing the execution 
    /// result.</returns>  
    public static IExecutionResult ToExecutionResult(
        this ModelStateDictionary modelState,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        ExecutionResults
            .Failure(statusCode)
            .WithErrors(ElementCollection.With(
                [.. modelState
                    .Keys
                    .Where(key => modelState[key]!.Errors.Count > 0)
                    .Select(key =>
                        new ElementEntry(
                            key,
                            [.. modelState[key]!.Errors.Select(error => error.ErrorMessage)]))]))
            .Build();

    /// <summary>  
    /// Converts a <see cref="BadHttpRequestException"/> to an 
    /// <see cref="IExecutionResult"/>.  
    /// </summary>  
    /// <param name="exception">The exception to convert.</param>  
    /// <returns>An <see cref="IExecutionResult"/> representing the 
    /// execution result.</returns>  
    public static IExecutionResult ToExecutionResult(
        this BadHttpRequestException exception)
    {
        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        int startParameterNameIndex = exception.Message
            .IndexOf('"', StringComparison.InvariantCulture) + 1;

        int endParameterNameIndex = exception.Message
            .IndexOf('"', startParameterNameIndex);

        string parameterName = exception
            .Message[startParameterNameIndex..endParameterNameIndex];

        parameterName = parameterName.Trim();

        string errorMessage = exception.Message
            .Replace("\\", string.Empty, StringComparison.InvariantCulture)
            .Replace("\"", string.Empty, StringComparison.InvariantCulture);

        return ExecutionResults
            .BadRequest()
            .WithTitle(((HttpStatusCode)exception.StatusCode).GetTitle())
            .WithDetail(isDevelopment ? exception.Message : ((HttpStatusCode)exception.StatusCode).GetDetail())
            .WithStatusCode((HttpStatusCode)exception.StatusCode)
            .WithError(parameterName, errorMessage)
            .Build();
    }

    /// <summary>  
    /// Converts an <see cref="Exception"/> to an <see cref="IExecutionResult"/> 
    /// for problem details.
    /// </summary>  
    /// <param name="exception">The exception to convert.</param>  
    /// <returns>An <see cref="IExecutionResult"/> representing the execution 
    /// result.</returns>  
    public static IExecutionResult ToExecutionResultForProblemDetails(
        this Exception exception) =>
        exception switch
        {
            BadHttpRequestException badHttpRequestException =>
                badHttpRequestException.ToExecutionResult(),
            _ => exception.ToExecutionResult()
        };

    /// <summary>  
    /// Converts an <see cref="IExecutionResult"/> to an <see cref="IActionResult"/>.  
    /// </summary>  
    /// <param name="executionResult">The execution result to convert.</param>  
    /// <returns>An <see cref="IActionResult"/> representing the execution
    /// result.</returns>  
    public static IActionResult ToActionResult(
        this IExecutionResult executionResult) =>
        new ObjectResult(executionResult)
        {
            StatusCode = (int)executionResult.StatusCode,
        };

    /// <summary>  
    /// Converts an <see cref="IExecutionResult"/> to an <see cref="IResult"/>.  
    /// </summary>  
    /// <param name="executionResult">The execution result to convert.</param>  
    /// <returns>An <see cref="IResult"/> representing the execution result.</returns>  
    public static IResult ToMinimalResult(this IExecutionResult executionResult) =>
        new MinimalResult(executionResult);

    /// <summary>
    /// Sets the metadata for the HTTP response based on the execution result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="executionResult">The execution result.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    public static async Task MetadataSetter(
        this HttpContext context,
        IExecutionResult executionResult)
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
}
