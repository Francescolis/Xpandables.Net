
using Microsoft.AspNetCore.Http;

using Xpandables.Net.Async;

namespace Xpandables.Net.ExecutionResults.Successes;

/// <summary>
/// Provides an HTTP response writer for execution results that contain asynchronous, paged data sequences.
/// </summary>
/// <remarks>Use this class to write execution results whose values implement <see cref="IAsyncPagedEnumerable"/>
/// to the HTTP response. This writer supports scenarios where data is streamed or paged asynchronously to the client.
/// Instances of this class are typically used within the execution pipeline to handle paged result sets
/// efficiently.</remarks>
public sealed class AsyncPagedExecutionResultResponseWriter : ExecutionResultResponseWriter
{
    /// <inheritdoc/>
    public override bool CanWrite(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.StatusCode.IsSuccess
            && executionResult.Value is IAsyncPagedEnumerable;
    }

    /// <summary>
    /// Asynchronously writes the execution result to the HTTP response using the specified context.
    /// </summary>
    /// <param name="context">The HTTP context for the current request. Cannot be null.</param>
    /// <param name="executionResult">The result of the execution to be written to the response. Cannot be null and must have a non-null Value.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public override async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(executionResult);
        ArgumentNullException.ThrowIfNull(executionResult.Value);

        await base.WriteAsync(context, executionResult).ConfigureAwait(false);

        IAsyncPagedEnumerable asyncPaged = (IAsyncPagedEnumerable)executionResult.Value;
    }
}
