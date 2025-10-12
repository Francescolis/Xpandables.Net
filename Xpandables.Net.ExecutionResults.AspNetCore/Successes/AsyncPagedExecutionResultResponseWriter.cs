
using Microsoft.AspNetCore.Http;

using Xpandables.Net.Async;

namespace Xpandables.Net.ExecutionResults.Successes;

public sealed class AsyncPagedExecutionResultResponseWriter : ExecutionResultResponseWriter
{
    /// <inheritdoc/>
    public override bool CanWrite(ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        return executionResult.StatusCode.IsSuccess
            && executionResult.Value is IAsyncPagedEnumerable;
    }


    public override async Task WriteAsync(HttpContext context, ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(executionResult);
        ArgumentNullException.ThrowIfNull(executionResult.Value);

        await base.WriteAsync(context, executionResult).ConfigureAwait(false);

        IAsyncPagedEnumerable asyncPaged = (IAsyncPagedEnumerable)executionResult.Value;
    }
}
