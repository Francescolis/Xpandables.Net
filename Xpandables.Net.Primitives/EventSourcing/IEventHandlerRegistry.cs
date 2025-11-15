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
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Defines a registry for retrieving event handler wrappers based on event type.
/// </summary>
/// <remarks>Implementations of this interface allow consumers to query for registered event handler wrappers
/// associated with a specific event type. This is typically used in event-driven architectures to resolve handlers
/// dynamically at runtime.</remarks>
public interface IEventHandlerRegistry
{
    /// <summary>
    /// Attempts to retrieve an event handler wrapper associated with the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event for which to obtain the handler wrapper. Cannot be null.</param>
    /// <param name="wrapper">When this method returns, contains the event handler wrapper for the specified event type if found; otherwise,
    /// null. This parameter is passed uninitialized.</param>
    /// <returns>true if a handler wrapper for the specified event type was found; otherwise, false.</returns>
    bool TryGetWrapper(Type eventType, [NotNullWhen(true)] out IEventHandlerWrapper? wrapper);
}

/// <summary>
/// Provides a thread-safe registry for event handler wrappers, allowing lookup of handlers by event type.
/// </summary>
/// <remarks>This registry is immutable after construction and is safe for concurrent access. Attempting to
/// register multiple wrappers for the same event type will result in only one being stored.</remarks>
/// <param name="wrappers">A collection of event handler wrappers to be registered. Each wrapper must have a unique event type.</param>
public sealed class StaticEventHandlerRegistry(IEnumerable<IEventHandlerWrapper> wrappers) : IEventHandlerRegistry
{
    private readonly ImmutableDictionary<Type, IEventHandlerWrapper> _mapper =
        wrappers.ToImmutableDictionary(w => w.EventType);

    /// <inheritdoc/>
    public bool TryGetWrapper(Type eventType, [NotNullWhen(true)] out IEventHandlerWrapper? wrapper) =>
        _mapper.TryGetValue(eventType, out wrapper);
}

/// <summary>
/// Provides a thread-safe registry for associating event types with their corresponding event handler wrappers.
/// </summary>
/// <remarks>This class enables dynamic registration and unregistration of event handlers for specific event types
/// at runtime. It is designed for concurrent access and can be safely used in multi-threaded environments. Typically,
/// consumers use this registry to manage event handler lifetimes and to resolve handlers for event
/// dispatching.</remarks>
public sealed class DynamycEventHandlerRegistry : IEventHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, IEventHandlerWrapper> _mapper = [];

    /// <summary>
    /// Registers a collection of event handlers for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to associate with the handlers. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="handlers">An enumerable collection of event handlers to be registered for the event type <typeparamref name="TEvent"/>.
    /// Cannot be null.</param>
    public void Register<TEvent>(IEnumerable<IEventHandler<TEvent>> handlers)
        where TEvent : class, IEvent
    {
        var wrapper = new EventHandlerWrapper<TEvent>(handlers);
        _mapper[typeof(TEvent)] = wrapper;
    }

    /// <summary>
    /// Unregisters the mapping for the specified event type.
    /// </summary>
    /// <remarks>Use this method to remove an event type mapping when it is no longer needed. If the event
    /// type was not previously registered, the method returns false and no action is taken.</remarks>
    /// <typeparam name="TEvent">The event type to unregister. Must be a reference type that implements <see cref="IEvent"/>.</typeparam>
    /// <returns>true if the mapping for the specified event type was successfully removed; otherwise, false.</returns>
    public bool Unregister<TEvent>()
        where TEvent : class, IEvent =>
        _mapper.TryRemove(typeof(TEvent), out _);

    /// <inheritdoc/>
    public bool TryGetWrapper(Type eventType, [NotNullWhen(true)] out IEventHandlerWrapper? wrapper) =>
        _mapper.TryGetValue(eventType, out wrapper);
}

/// <summary>
/// Provides a composite event handler registry that delegates event handler lookups to both static and dynamic
/// registries.
/// </summary>
/// <remarks>This registry attempts to retrieve event handler wrappers from the dynamic registry first, then falls
/// back to the static registry if not found. It is useful when both static and dynamic event handler registration
/// mechanisms are required in an application.</remarks>
/// <param name="staticRegistry">The static event handler registry used for resolving event handlers registered at compile time. Cannot be null.</param>
/// <param name="dynamicRegistry">The dynamic event handler registry used for resolving event handlers registered at runtime. Cannot be null.</param>
public sealed class CompositeEventHandlerRegistry(
    StaticEventHandlerRegistry staticRegistry,
    DynamycEventHandlerRegistry dynamicRegistry) : IEventHandlerRegistry
{
    /// <inheritdoc/>
    public bool TryGetWrapper(Type eventType, [NotNullWhen(true)] out IEventHandlerWrapper? wrapper) =>
        dynamicRegistry.TryGetWrapper(eventType, out wrapper)
        || staticRegistry.TryGetWrapper(eventType, out wrapper);
}