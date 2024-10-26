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

namespace Xpandables.Net.Operations.Executors;

/// <summary>
/// Executor for handling operation results that indicate failure.
/// </summary>
public sealed class OperationResultFailureExecutor : IOperationResultExecutor
{
    ///<inheritdoc/>
    public bool CanExecute(IOperationResult operationResult) =>
        operationResult.IsFailureStatusCode();

    ///<inheritdoc/>
    public Task ExecuteAsync(
        HttpContext context,
        IOperationResult operationResult)
    {
        context.Response.StatusCode = (int)operationResult.StatusCode;

        bool isDevelopment = context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        ProblemDetails problemDetails = operationResult.StatusCode.IsBadRequest()
            ? new ValidationProblemDetails(operationResult.ToModelStateDictionary())
            {
                Title = operationResult.Title ?? operationResult.StatusCode.GetTitle(),
                Detail = operationResult.Detail ?? operationResult.StatusCode.GetDetail(),
                Status = (int)operationResult.StatusCode,
                Instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}",
                Type = isDevelopment ? operationResult.GetType().Name : null,
                Extensions = operationResult.ToElementExtensions()
            }
            : new ProblemDetails()
            {
                Title = operationResult.Title ?? operationResult.StatusCode.GetTitle(),
                Detail = operationResult.Detail ?? operationResult.StatusCode.GetDetail(),
                Status = (int)operationResult.StatusCode,
                Instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}",
                Type = isDevelopment ? operationResult.GetType().Name : null,
                Extensions = operationResult.ToElementExtensions()
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
