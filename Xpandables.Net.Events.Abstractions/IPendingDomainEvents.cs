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
namespace Xpandables.Net.Events;

/// <summary>
/// Represents a batch of domain events that are pending processing, along with an action to execute upon successful
/// commitment.
/// </summary>
/// <remarks>This type encapsulates a collection of domain events and provides a mechanism to define a callback
/// action that is invoked when the batch is successfully committed. It is typically used in scenarios where domain
/// events need to be processed and committed as a unit.</remarks>
public sealed record PendingDomainEventsBatch
{
    /// <summary>
    /// Gets the collection of domain events associated with the current entity.
    /// </summary>
    public required IReadOnlyCollection<IDomainEvent> Events { get; init; }
    /// <summary>
    /// Gets or sets the action to be executed when the commit operation is completed.
    /// </summary>
    public required Action OnCommitted { get; init; }
}

/// <summary>
/// Represents a contract for managing and processing domain events within a transactional context.
/// </summary>
/// <remarks>This interface provides methods to add domain events to a queue, execute a callback upon successful
/// commit,  and retrieve and clear pending domain event batches. It is typically used in scenarios where domain events 
/// need to be managed in a consistent and transactional manner.</remarks>
public interface IPendingDomainEvents
{
    /// <summary>
    /// Adds a collection of domain events to the current context and executes a callback upon successful commit.
    /// </summary>
    /// <remarks>This method is typically used to queue domain events for processing and ensures that the
    /// provided callback  is invoked only after the commit operation completes successfully.</remarks>
    /// <param name="events">The collection of domain events to add. Cannot be null or empty.</param>
    /// <param name="onCommitted">The callback to execute after the events are successfully committed. Cannot be null.</param>
    void AddRange(IReadOnlyCollection<IDomainEvent> events, Action onCommitted);

    /// <summary>
    /// Retrieves and clears all pending domain event batches from the queue.
    /// </summary>
    /// <remarks>This method returns a collection of domain event batches that were queued for processing.
    /// After calling this method, the queue will be empty. The returned collection is read-only and reflects the state
    /// of the queue at the time of the call.</remarks>
    /// <returns>A read-only collection of <see cref="PendingDomainEventsBatch"/> representing the pending domain event batches.
    /// If no events are pending, the collection will be empty.</returns>
    IReadOnlyCollection<PendingDomainEventsBatch> Drain();
}
