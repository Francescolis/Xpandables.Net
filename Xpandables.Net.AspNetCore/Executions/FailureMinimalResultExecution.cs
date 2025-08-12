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
        }

        IResult result = Results.Problem(problemDetails);

        await result.ExecuteAsync(context).ConfigureAwait(false);
    }
}