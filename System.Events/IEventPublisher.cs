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
namespace System.Events;

/// <summary>
/// Defines a contract for asynchronously publishing events to all registered event handlers.
/// </summary>
/// <remarks>Implementations of this interface are responsible for delivering events to all appropriate
/// subscribers. Thread safety and delivery guarantees may vary depending on the implementation. Events are dispatched
/// based on their type, and only handlers registered for the specific event type will be invoked.</remarks>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event asynchronously to all subscribed handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event being published.</typeparam>
    /// <param name="eventInstance">The event to be published.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when publishing the event fails. See inner exception for additional details.
    /// </exception>
    Task PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}

/// <summary>
/// Provides extension methods for publishing events asynchronously using an <see cref="IEventPublisher"/>.
/// </summary>
/// <remarks>This static class contains helper methods that simplify the process of dispatching multiple events to
/// subscribers via an event bus. All methods are thread-safe and intended for use with implementations of <see
/// cref="IEventPublisher"/>.</remarks>
public static partial class IEventPublisherExtensions
{
    /// <summary>
    /// Extension methods for <see cref="IEventPublisher"/> to simplify event publishing.
    /// </summary>
    /// <param name="eventPublisher">The event publisher instance to extend.</param>
    extension(IEventPublisher eventPublisher)
    {
        /// <summary>
        /// Publishes a collection of events asynchronously to the event bus.
        /// </summary>
        /// <param name="events">The events to be published. Cannot be null. Each event in the collection will be dispatched to subscribers.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when publishing the event fails. See inner exception for additional details.</exception>
        public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventPublisher);
            ArgumentNullException.ThrowIfNull(events);

            var tasks = events.Select(@event => eventPublisher.PublishAsync(@event, cancellationToken));

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception exception)
                when (exception is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    "Unable to publish the events. See inner exception for details.",
                    exception);
            }
        }
    }
}