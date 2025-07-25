
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Handlers;

/// <summary>
/// Handles post-processing of requests by appending aggregates to the appropriate store.
/// </summary>
/// <remarks>This handler is designed to be used in scenarios where a request results in an aggregate that needs
/// to be appended to a store. It utilizes the <see cref="IAggregateStore{T}"/> service to perform the append
/// operation.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. 
/// Must implement <see cref="IDependencyRequest"/> and <see
/// cref="IAggregateAppended"/>.</typeparam>
/// <param name="serviceProvider"></param>
public sealed class AggregateRequestPostHandler<TRequest>(IServiceProvider serviceProvider)
    : IRequestPostHandler<TRequest>
    where TRequest : class, IDependencyRequest, IAggregateAppended
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        ExecutionResult response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            Type aggregateStoreType = typeof(IAggregateStore<>)
                .MakeGenericType(context.Request.DependencyType);

            IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
                .GetRequiredService(aggregateStoreType);

            object aggregate = context.Request.DependencyInstance.Value;

            await aggregateStore
                .AppendAsync((Aggregate)aggregate, cancellationToken)
                .ConfigureAwait(false);
        }

        return response;
    }
}
