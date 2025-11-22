/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Events.Domain;

namespace System.ExecutionResults.Pipelines;

/// <summary>
/// Provides a pipeline decorator that ensures domain events are committed to the event store after handling a request
/// requiring event storage.
/// </summary>
/// <remarks>This decorator should be placed in the pipeline for requests that generate domain events requiring
/// persistence. After the request is handled, 
/// all pending changes are saved to the event store, and committed domain events are notified. 
/// The decorator is thread-safe and intended for use in event-driven architectures.</remarks>
/// <typeparam name="TRequest">The type of request being handled. Must implement <see cref="IRequest"/> and <see cref="IRequiresEventStorage"/>.</typeparam>
/// <param name="eventStore">The event store used to flush events.</param>
public sealed class PipelineEventStoreEventDecorator<TRequest>(IEventStore eventStore) :
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
            await eventStore
                .FlushEventsAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var batch in PipelineDomainEventsDecorator<TRequest>.DomainEventCommitBuffer.Drain())
            {
                batch.OnCommitted();
            }
        }
    }
}
