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
using System.Entities;

namespace System.Events.Integration;

/// <summary>
/// Represents an event that failed to be processed in the inbox, including its identifier, the consumer that attempted
/// processing, and the associated error message.
/// </summary>
/// <param name="EventId">The unique identifier of the event that failed.</param>
/// <param name="Consumer">The name of the consumer that attempted to process the event.</param>
/// <param name="Error">A description of the error that occurred during event processing.</param>
public readonly record struct FailedInboxEvent(Guid EventId, string Consumer, string Error);

/// <summary>
/// Represents an event indicating that an inbox operation has been completed by a specific consumer.
/// </summary>
/// <param name="EventId">The unique identifier for the completed event.</param>
/// <param name="Consumer">The name of the consumer that completed the inbox event. Cannot be null.</param>
public readonly record struct CompletedInboxEvent(Guid EventId, string Consumer);

/// <summary>
/// Represents the result of an inbox event, including its unique identifier and current processing state.
/// </summary>
/// <param name="EventId">The unique identifier of the event associated with this status.</param>
/// <param name="Status">The current processing status of the event.</param>
public readonly record struct InboxReceiveResult(Guid EventId, EntityStatus Status);

/// <summary>
/// Represents a contract for storing and retrieving inbox messages.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms for persisting and accessing messages in an
/// inbox. The specific storage medium and retrieval strategies depend on the concrete implementation.</remarks>
public interface IInboxStore
{
    /// <summary>
    /// Attempts to register the specified integration event for processing by the given consumer.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="consumer">Logical consumer/handler name (part of the idempotency key).</param>
    /// <param name="event">The integration event to record.</param>
    /// <param name="visibilityTimeout">Optional lease duration used when reclaiming failed events via dequeue.</param>
    /// <returns>An <see cref="InboxReceiveResult"/> indicating whether processing should proceed.</returns>
    Task<InboxReceiveResult> ReceiveAsync(
        IIntegrationEvent @event,
        string consumer,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified inbox events as completed asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <param name="events">An array of inbox events to mark as completed. Cannot be null or contain null elements.</param>
    /// <returns>A task that represents the asynchronous completion operation.</returns>
    Task CompleteAsync(CancellationToken cancellationToken, params CompletedInboxEvent[] events);

    /// <summary>
    /// Marks the specified inbox events as failed asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <param name="failures">An array of failed inbox events to be marked as failed. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task FailAsync(CancellationToken cancellationToken, params FailedInboxEvent[] failures);
}
