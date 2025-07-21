using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Represents a decorator that executes a series of pre-handlers before the main request handler in a pipeline.
/// </summary>
/// <remarks>This decorator iterates over a collection of pre-handlers, executing each one in sequence before
/// invoking the main request handler. It is used to perform operations or checks that should occur before the main
/// request processing.</remarks>
/// <typeparam name="TRequest">The type of the request being processed. Must implement <see cref="IRequest"/>.</typeparam>
/// <param name="preHandlers"></param>
public sealed class PipelinePreDecorator<TRequest>(
    IEnumerable<IRequestPreHandler<TRequest>> preHandlers) : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    ///<inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken)
    {
        foreach (IRequestPreHandler<TRequest> preHandler in preHandlers)
        {
            ExecutionResult result = await preHandler
                .HandleAsync(context, cancellationToken)
                .ConfigureAwait(false);

            // If any pre-handler returns a failure response, we short-circuit and return that response.
            if (!result.IsSuccessStatusCode)
            {
                return result;
            }
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
