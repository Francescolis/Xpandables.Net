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
/// Represents an event that has failed, including its unique identifier and an associated error message.
/// </summary>
/// <param name="EventId">The unique identifier of the event that failed.</param>
/// <param name="Error">A message describing the error that caused the event to fail. Cannot be null.</param>
public readonly record struct FailedEvent(Guid EventId, string Error);

/// <summary>
/// Defines a contract for an outbox store that manages the reliable enqueueing, retrieval, and state tracking of
/// integration events for eventual processing.
/// </summary>
/// <remarks>Implementations of this interface provide durable storage and coordination for integration events,
/// supporting patterns such as the transactional outbox. The interface enables asynchronous operations for enqueuing
/// events, retrieving them with visibility timeouts to prevent duplicate processing, and marking events as completed or
/// failed. This abstraction is intended for use in distributed systems to ensure reliable event delivery and processing
/// across service boundaries.</remarks>
public interface IOutboxStore
{
    /// <summary>
    /// Asynchronously enqueues one or more integration events for processing.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the enqueue operation.</param>
    /// <param name="events">An array of integration events to enqueue. Cannot be null or contain null elements.</param>
    /// <returns>A task that represents the asynchronous enqueue operation.</returns>
    Task EnqueueAsync(CancellationToken cancellationToken, params IIntegrationEvent[] events);

    /// <summary>
    /// Asynchronously removes and returns up to a specified number of integration events from the queue, making them
    /// temporarily invisible to other consumers.
    /// </summary>
    /// <remarks>If the operation is cancelled via the provided cancellation token, the returned task is
    /// canceled. The visibility timeout ensures that dequeued events are not processed by other consumers for the
    /// specified duration, after which they may become available again if not acknowledged.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the dequeue operation.</param>
    /// <param name="maxEvents">The maximum number of events to dequeue in a single operation. Must be greater than zero. The default is 10.</param>
    /// <param name="visibilityTimeout">An optional duration specifying how long the dequeued events remain invisible to other consumers. If not
    /// specified, a default timeout is used.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of integration
    /// events that were dequeued. The list will be empty if no events are available.</returns>
    Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
        CancellationToken cancellationToken,
        int maxEvents = 10,
        TimeSpan? visibilityTimeout = default);

    /// <summary>
    /// Marks the specified <see cref="IIntegrationEvent"/>s as completed asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <param name="eventIds">An array of event identifiers to mark as completed. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous completion operation.</returns>
    Task CompleteAsync(CancellationToken cancellationToken, params Guid[] eventIds);

    /// <summary>
    /// Marks the specified <see cref="IIntegrationEvent"/> id as failure with the error message.
    /// </summary>
    /// <param name="failures">A read-only span containing the failed events to be recorded. The span must not be empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task FailAsync(CancellationToken cancellationToken, params FailedEvent[] failures);
}
