/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

namespace System.Events;

/// <summary>
/// Defines a wrapper for handling events of a specific type asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for processing events represented by the type
/// specified in <see cref="EventType"/>. This abstraction allows event handling logic to be decoupled from the event
/// dispatching mechanism. Implementations should ensure thread safety if they are intended to be used
/// concurrently.</remarks>
public interface IEventHandlerWrapper
{
	/// <summary>
	/// Gets the runtime type of the event associated with this instance.
	/// </summary>
	Type EventType { get; }

	/// <summary>
	/// Processes the specified instance asynchronously, allowing cancellation via a token.
	/// </summary>
	/// <param name="event">The object instance to be handled. Cannot be null.</param>
	/// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous handling operation.</returns>
	Task HandleAsync(object @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides a singleton-safe wrapper that resolves event handlers from a new service scope on each invocation.
/// This prevents the captive dependency anti-pattern by ensuring scoped handlers are never trapped
/// inside a singleton wrapper.
/// </summary>
/// <typeparam name="TEvent">The type of event to be handled. Must implement <see cref="IEvent"/> and be a reference type.</typeparam>
/// <param name="serviceScopeFactory">The factory used to create service scopes for resolving scoped event handlers.</param>
public sealed class EventHandlerWrapper<TEvent>(IServiceScopeFactory serviceScopeFactory) : IEventHandlerWrapper
	where TEvent : class, IEvent
{
	/// <inheritdoc />
	public Type EventType => typeof(TEvent);

	/// <inheritdoc />
	public async Task HandleAsync(object @event, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(@event);

		if (@event is not TEvent instance)
		{
			throw new ArgumentException($"Invalid event type. Expected {typeof(TEvent).Name}, but got {@event.GetType().Name}.", nameof(@event));
		}

		AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
		await using (scope.ConfigureAwait(false))
		{
			IEnumerable<IEventHandler<TEvent>> handlers = scope.ServiceProvider
				.GetServices<IEventHandler<TEvent>>();

			foreach (IEventHandler<TEvent> handler in handlers)
			{
				await handler.HandleAsync(instance, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}

/// <summary>
/// Provides a wrapper that holds a direct reference to a collection of event handler instances.
/// Intended for use with <see cref="DynamicEventHandlerRegistry"/> where handlers are provided
/// at runtime rather than resolved from the DI container.
/// </summary>
/// <typeparam name="TEvent">The type of event to be handled. Must implement <see cref="IEvent"/> and be a reference type.</typeparam>
/// <param name="handlers">The collection of event handlers that will be invoked to process events of type <typeparamref name="TEvent"/>.</param>
internal sealed class DirectEventHandlerWrapper<TEvent>(IEnumerable<IEventHandler<TEvent>> handlers) : IEventHandlerWrapper
	where TEvent : class, IEvent
{
	/// <inheritdoc />
	public Type EventType => typeof(TEvent);

	/// <inheritdoc />
	public async Task HandleAsync(object @event, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(@event);

		if (@event is not TEvent instance)
		{
			throw new ArgumentException($"Invalid event type. Expected {typeof(TEvent).Name}, but got {@event.GetType().Name}.", nameof(@event));
		}

		foreach (IEventHandler<TEvent> handler in handlers)
		{
			await handler.HandleAsync(instance, cancellationToken).ConfigureAwait(false);
		}
	}
}
