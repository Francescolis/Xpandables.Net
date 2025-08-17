
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
/// This class represents a minimal result execution that handles failure scenarios.
/// </summary>
public sealed class FailureMinimalResultExecution : MinimalResultExecution
{
    /// <inheritdoc/>
    public sealed override bool CanExecute(ExecutionResult executionResult) =>
        !executionResult.IsSuccessStatusCode;
    /// <inheritdoc/>
    public sealed override async Task ExecuteAsync(HttpContext context, ExecutionResult executionResult)
    {
        await base
            .ExecuteAsync(context, executionResult)
            .ConfigureAwait(false);

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
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails
            }).ConfigureAwait(false);

            return;
        }

        IResult result = Results.Problem(problemDetails);

        await result.ExecuteAsync(context).ConfigureAwait(false);
    }
}