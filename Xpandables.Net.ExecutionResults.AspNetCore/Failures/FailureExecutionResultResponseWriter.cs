using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.ExecutionResults.Failures;

/// <summary>
/// Provides an implementation of an execution result response writer that formats failure results using the Problem
/// Details standard (RFC 7807).
/// </summary>
/// <remarks>This writer is typically used to return standardized error responses for failed execution results in
/// HTTP APIs. It attempts to use an available IProblemDetailsService to generate the response; if none is found, it
/// falls back to a default Problem Details response. This class is intended for scenarios where consistent error
/// formatting is required across API endpoints.</remarks>
public sealed class FailureExecutionResultResponseWriter : ExecutionResultResponseWriter
{
    /// <inheritdoc/>
    public override bool CanWrite(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.IsFailure;
    }

    /// <summary>
    /// Asynchronously writes the specified execution result to the HTTP response using the Problem Details format.
    /// </summary>
    /// <remarks>If an <see cref="IProblemDetailsService"/> is available in the request services, it is used
    /// to write the problem details response. Otherwise, a default Problem Details response is written. This method is
    /// typically used to return error information in a standardized format according to RFC 7807.</remarks>
    /// <param name="context">The HTTP context for the current request. Cannot be null.</param>
    /// <param name="executionResult">The execution result to be written to the response. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public override async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(executionResult);

        await base.WriteAsync(context, executionResult).ConfigureAwait(false);

        ProblemDetails problem = executionResult.ToProblemDetails(context);

        if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem
            }).ConfigureAwait(false);

            return;
        }

        IResult result = Results.Problem(problem);

        await result.ExecuteAsync(context).ConfigureAwait(false);
    }
}
