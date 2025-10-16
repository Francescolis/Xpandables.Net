
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
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Repositories;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides a pipeline decorator that ensures domain events are committed to the event store after handling a request
/// requiring event storage.
/// </summary>
/// <remarks>This decorator should be placed in the pipeline for requests that generate domain events requiring
/// persistence. The <see cref="IUnitOfWork"/> is a keyed service named <see cref="IEventStore"/>. After the request is handled, 
/// all pending changes are saved to the event store, and committed domain events are notified. 
/// The decorator is thread-safe and intended for use in event-driven architectures.</remarks>
/// <typeparam name="TRequest">The type of request being handled. Must implement <see cref="IRequest"/> and <see cref="IRequiresEventStorage"/>.</typeparam>
/// <param name="unitOfWork">The unit of work used to persist changes to the event store after the request is processed.</param>
public sealed class PipelineEventStoreEventDecorator<TRequest>([FromKeyedServices(nameof(IEventStore))] IUnitOfWork unitOfWork) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresEventStorage
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        try
        {
            ExecutionResult response = await nextHandler(cancellationToken).ConfigureAwait(false);

            return response;
        }
        finally
        {
            await unitOfWork
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var batch in PipelineDomainEventsDecorator<TRequest>.DomainEventCommitBuffer.Drain())
            {
                batch.OnCommitted();
            }
        }
    }
}
