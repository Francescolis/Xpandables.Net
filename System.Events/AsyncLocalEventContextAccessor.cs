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
/// Provides access to the current event context for the logical execution flow using asynchronous local storage.
/// </summary>
/// <remarks>This accessor ensures that the event context is preserved across asynchronous calls within the same
/// logical operation. It is suitable for scenarios where event context must flow with async operations, such as in web
/// request handling or background tasks.</remarks>
public sealed class AsyncLocalEventContextAccessor : IEventContextAccessor
{
    private static readonly AsyncLocal<EventContext> _current = new();

    /// <inheritdoc/>
    public EventContext Current => _current.Value;

    /// <summary>
    /// Sets the current event context for the executing thread.
    /// </summary>
    /// <remarks>This method updates the event context stored for the current thread. Use this to ensure that
    /// subsequent operations reference the correct context within thread-local storage.</remarks>
    /// <param name="context">The event context to associate with the current thread. Cannot be null.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    internal void SetCurrent(EventContext context) => _current.Value = context;
}