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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions.Minimals;

namespace Xpandables.Net.Executions;

/// <summary>  
/// Provides extension methods for converting execution results.  
/// </summary>  
public static class ExecutionResultExtensions
{
    /// <summary>
    /// Converts an execution result containing errors into a model state dictionary for validation purposes.
    /// </summary>
    /// <param name="executionResult">Contains the errors that will be processed to populate the model state dictionary.</param>
    /// <returns>A model state dictionary populated with errors extracted from the execution result.</returns>
    public static ModelStateDictionary ToModelStateDictionary(this ExecutionResult executionResult)
    {
        ModelStateDictionary modelState = new();
        foreach (ElementEntry entry in executionResult.Errors
            .Where(e => e.Key != _ExecutionResult.ExceptionKey))
        {
            foreach (string? value in entry.Values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                modelState.AddModelError(entry.Key, value);
            }
        }

        return modelState;
    }

    /// <summary>
    /// Converts a model state dictionary into an execution result indicating failure with associated errors.
    /// </summary>
    /// <param name="modelState">Contains the validation errors that need to be included in the execution result.</param>
    /// <param name="statusCode">Specifies the HTTP status code to be returned in the execution result.</param>
    /// <returns>An execution result object that encapsulates the failure status and validation errors.</returns>
    public static ExecutionResult ToExecutionResult(
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
    /// Converts a BadHttpRequestException into an ExecutionResult object, providing detailed error information.
    /// </summary>
    /// <param name="exception">The exception provides context for generating the execution result.</param>
    /// <returns>An ExecutionResult object containing the status code, title, detail, and error information.</returns>
    public static ExecutionResult ToExecutionResult(this BadHttpRequestException exception)
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
            .WithTitle(((HttpStatusCode)exception.StatusCode).GetAppropriateTitle())
            .WithDetail(isDevelopment ? exception.Message : ((HttpStatusCode)exception.StatusCode).GetAppropriateDetail())
            .WithStatusCode((HttpStatusCode)exception.StatusCode)
            .WithError(parameterName, errorMessage)
            .Build();
    }

    /// <summary>
    /// Converts an exception into an execution result, handling specific types of exceptions differently.
    /// </summary>
    /// <param name="exception">Handles an error that occurs during the execution of a request.</param>
    /// <returns>An execution result representing the outcome of the exception.</returns>
    public static ExecutionResult ToExecutionResultForProblemDetails(this Exception exception) =>
        exception switch
        {
            BadHttpRequestException badHttpRequestException => badHttpRequestException.ToExecutionResult(),
            _ => exception.ToExecutionResult()
        };

    /// <summary>
    /// Converts an execution result into an action result for web responses.
    /// </summary>
    /// <param name="executionResult">Contains the outcome and status code of an operation.</param>
    /// <returns>Returns an object result with the specified status code.</returns>
    public static IActionResult ToActionResult(this ExecutionResult executionResult) =>
        new ObjectResult(executionResult)
        {
            StatusCode = (int)executionResult.StatusCode,
        };

    /// <summary>
    /// Converts an execution result into a minimal result format.
    /// </summary>
    /// <param name="executionResult">Represents the result of an execution that will be transformed into a minimal format.</param>
    /// <returns>Returns a minimal result object based on the provided execution result.</returns>
    public static IResult ToMinimalResult(this ExecutionResult executionResult) =>
        new MinimalResult(executionResult);
}
