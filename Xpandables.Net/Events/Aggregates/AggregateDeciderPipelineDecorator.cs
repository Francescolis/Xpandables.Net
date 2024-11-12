using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Operations;
using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Decorators;

namespace Xpandables.Net.Events.Aggregates;
internal sealed class AggregateDeciderPipelineDecorator<TRequest, TResponse>(
    IServiceProvider serviceProvider) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDeciderCommand
    where TResponse : IOperationResult
{
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type aggregateStoreType = typeof(IAggregateStore<>)
                .MakeGenericType(request.Type);

            IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
                .GetRequiredService(aggregateStoreType);

            IAggregate dependency = await aggregateStore
                .PeekAsync((Guid)request.KeyId, cancellationToken)
                .ConfigureAwait(false);

            request.Dependency = dependency;

            try
            {
                TResponse result = await next().ConfigureAwait(false);
                return result;
            }
            finally
            {

                await aggregateStore
                    .AppendAsync(dependency, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"An error occurred when applying decider pattern to aggregate " +
                $"with the key '{request.KeyId}'.",
                exception);
        }
    }
}
