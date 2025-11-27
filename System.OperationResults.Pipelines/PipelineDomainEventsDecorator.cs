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
using System.Events;
using System.Events.Domain;

namespace System.OperationResults.Pipelines;

/// <summary>
/// A pipeline decorator that processes and publishes domain events after the execution of a request handler.
/// </summary>
/// <remarks>This decorator ensures that any domain events generated during the execution of a request handler are
/// published in batches. It processes up to 16 passes of event batches, draining and publishing events in each pass. 
/// The decorator relies on the <see cref="IPendingDomainEventsBuffer"/> service to retrieve pending events and the  <see
/// cref="IEventPublisher"/> service to publish them.  Domain event batches are temporarily stored in a thread-local buffer
/// and are committed after the unit of work  completes. This ensures that domain events are only published if the
/// associated transaction is successfully committed.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. Must implement <see cref="IRequest"/> and <see
/// cref="IRequiresEventStorage"/>.</typeparam>
/// <param name="pendingDomainEvents"></param>
/// <param name="publisher"></param>
public sealed class PipelineDomainEventsDecorator<TRequest>(
    IPendingDomainEventsBuffer pendingDomainEvents,
    IEventPublisher publisher) : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresEventStorage
{
    /// <inheritdoc/>
    public async Task<OperationResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        var result = await nextHandler(cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
            return result;

        for (int pass = 0; pass < 16; pass++)
        {
            var batches = pendingDomainEvents.Drain();
            if (batches.Count == 0) break;

            foreach (var batch in batches)
            {
                await publisher.PublishAsync(batch.Events, cancellationToken).ConfigureAwait(false);

                // Important: do NOT call OnCommitted here; commit happens after SaveChanges.
                // The unit-of-work event decorator will commit and then invoke callbacks.
                DomainEventCommitBuffer.Current.Add(batch);
            }
        }

        return result;
    }

    /// <summary>
    /// Provides a thread-local buffer for managing batches of pending domain events.
    /// </summary>
    /// <remarks>This class is designed to facilitate the temporary storage and management of domain event
    /// batches within the context of a single thread. It ensures that each thread has its own isolated
    /// buffer.</remarks>
    internal static class DomainEventCommitBuffer
    {
        [ThreadStatic] private static List<PendingDomainEventsBatch>? _batches;
        /// <summary>
        /// Gets the current list of pending domain event batches.
        /// </summary>
        public static List<PendingDomainEventsBatch> Current => _batches ??= [];
        /// <summary>
        /// Retrieves and clears all pending domain event batches.
        /// </summary>
        /// <remarks>This method collects all currently pending domain event batches, clears the internal
        /// collection,  and returns the collected events. Subsequent calls will return an empty list unless new events
        /// are added.</remarks>
        /// <returns>A list of <see cref="PendingDomainEventsBatch"/> objects representing the pending domain event batches.  The
        /// list will be empty if no events are pending.</returns>
        public static List<PendingDomainEventsBatch> Drain()
        {
            var list = Current.ToList();
            Current.Clear();
            return list;
        }

        /// <summary>
        /// Adds a batch of pending domain events to the current context.
        /// </summary>
        /// <param name="batch">The batch of pending domain events to add. Cannot be null.</param>
        public static void Add(PendingDomainEventsBatch batch) => Current.Add(batch);
    }
}
