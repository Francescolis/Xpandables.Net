/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines a method to automatically subscribe to events.
/// </summary>
public interface IEventSubscriber : IDisposable
{
    /// <summary>
    /// Allows application author to subscribe to an event 
    /// with the specific handler.
    /// </summary>
    /// <typeparam name="TEvent">Type of event.</typeparam>
    /// <param name="subscriber">The action to be used to 
    /// handle the event.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="subscriber"/> is null</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    void Subscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : notnull, IEvent;

    /// <summary>
    /// Allows application author to subscribe 
    /// to an event with the specific handler.
    /// </summary>
    /// <typeparam name="TEvent">Type of event.</typeparam>
    /// <param name="subscriber">The action to be used 
    /// to handle the event.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="subscriber"/> is null</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    void Subscribe<TEvent>(Func<TEvent, ValueTask> subscriber)
        where TEvent : notnull, IEvent;
}