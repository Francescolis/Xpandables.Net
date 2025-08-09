using Xpandables.Net.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Represents a decorator that executes post-handling logic for a request in a pipeline.
/// </summary>
/// <remarks>This decorator is used to apply additional processing after the main request handler has executed. It
/// iterates over a collection of post-handlers, invoking each one with the request context and the result of the main
/// handler. This allows for operations such as logging, auditing, or modifying the response.</remarks>
/// <typeparam name="TRequest">The type of the request being processed. Must implement <see cref="IRequest"/>.</typeparam>
/// <param name="postHandlers"></param>
public sealed class PipelinePostDecorator<TRequest>(
    IEnumerable<IRequestPostHandler<TRequest>> postHandlers) : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    ///<inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken)
    {
        ExecutionResult response = await next(cancellationToken).ConfigureAwait(false);

        foreach (IRequestPostHandler<TRequest> postHandler in postHandlers)
        {
            ExecutionResult result = await postHandler
                .HandleAsync(context, response, cancellationToken)
                .ConfigureAwait(false);

            // If any post-handler returns a failure response, we short-circuit and return that response.
            if (!result.IsSuccessStatusCode)
            {
                return result;
            }
        }

        return response;
    }
}
