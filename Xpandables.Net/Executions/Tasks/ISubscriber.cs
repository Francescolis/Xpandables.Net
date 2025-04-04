﻿/*******************************************************************************
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
namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines a subscriber for events.
/// </summary>
public interface ISubscriber : IDisposable
{
    /// <summary>
    /// Subscribes to an event with a specified action.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The action to be executed when the event 
    /// is published.</param>
    void Subscribe<TEvent>(Action<TEvent> subscriber)
        where TEvent : notnull, IEvent;

    /// <summary>
    /// Subscribes to an event with a specified asynchronous function.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="subscriber">The asynchronous function to be executed 
    /// when the event is published.</param>
    void Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        where TEvent : notnull, IEvent;
}
