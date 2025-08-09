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
/// Represents a subscriber interface that defines methods to subscribe to events using different mechanisms.
/// </summary>
/// <remarks>
/// This interface allows subscribing to events with an action, an asynchronous function,
/// or an event handler of a specified event type. Implementations of this interface should
/// handle the management of event subscriptions and disposal of resources.
/// </remarks>
public interface ISubscriber : IDisposable
{
    /// <summary>
    /// Subscribes to an event with a specified action.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">
    /// The action to be executed when the event
    /// is published.
    /// </param>
    void Subscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to an event with a specified asynchronous function.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">
    /// The asynchronous function to be executed
    /// when the event is published.
    /// </param>
    void Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to an event with a specified event handler.
    /// </summary>
    /// <param name="subscriber">The event handler to be executed when the event is published.</param>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    void Subscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to an event with a specified action and returns a disposable token.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">
    /// The action to be executed when the event is published.
    /// </param>
    /// <returns>A disposable token that can be used to unsubscribe.</returns>
    IDisposable SubscribeDisposable<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to an event with a specified asynchronous function and returns a disposable token.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">
    /// The asynchronous function to be executed when the event is published.
    /// </param>
    /// <returns>A disposable token that can be used to unsubscribe.</returns>
    IDisposable SubscribeDisposable<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to an event with a specified event handler and returns a disposable token.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The event handler to be executed when the event is published.</param>
    /// <returns>A disposable token that can be used to unsubscribe.</returns>
    IDisposable SubscribeDisposable<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes from an event with the specified action.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The action to unsubscribe.</param>
    /// <returns>True if the subscriber was found and removed; otherwise, false.</returns>
    bool Unsubscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes from an event with the specified asynchronous function.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The asynchronous function to unsubscribe.</param>
    /// <returns>True if the subscriber was found and removed; otherwise, false.</returns>
    bool Unsubscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes from an event with the specified event handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The event handler to unsubscribe.</param>
    /// <returns>True if the subscriber was found and removed; otherwise, false.</returns>
    bool Unsubscribe<TEvent>(IEventHandler<TEvent> subscriber)
        where TEvent : class, IEvent;
}