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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xpandables.Net.Executions;

/// <summary>
/// Processes for handling execution results that indicate failure.
/// </summary>
public sealed class EndpointExecutionResultFailureHandler : IEndpointExecutionResultHandler
{
    ///<inheritdoc/>
    public bool CanProcess(IExecutionResult executionResult) =>
        executionResult.IsFailureStatusCode();

    ///<inheritdoc/>
    public Task HandleAsync(HttpContext context, IExecutionResult executionResult)
    {
        context.Response.StatusCode = (int)executionResult.StatusCode;

        bool isDevelopment = context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        ProblemDetails problemDetails = executionResult.StatusCode.IsValidationProblemRequest()
            ? new ValidationProblemDetails(executionResult.ToModelStateDictionary())
            {
                Title = executionResult.Title ?? executionResult.StatusCode.GetTitle(),
                Detail = executionResult.Detail ?? executionResult.StatusCode.GetDetail(),
                Status = (int)executionResult.StatusCode,
                Instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}",
                Type = isDevelopment ? executionResult.GetType().Name : null,
                Extensions = executionResult.ToElementExtensions()
            }
            : new ProblemDetails()
            {
                Title = executionResult.Title ?? executionResult.StatusCode.GetTitle(),
                Detail = executionResult.Detail ?? executionResult.StatusCode.GetDetail(),
                Status = (int)executionResult.StatusCode,
                Instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}",
                Type = isDevelopment ? executionResult.GetType().Name : null,
                Extensions = executionResult.ToElementExtensions()
            };

        if (context.RequestServices
            .GetService<IProblemDetailsService>() is { } problemDetailsService)
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
}
