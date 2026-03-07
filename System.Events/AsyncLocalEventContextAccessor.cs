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
namespace System.Events;

/// <summary>
/// Provides access to the current event context for the logical execution flow using asynchronous local storage.
/// </summary>
/// <remarks>
/// <para>This accessor ensures that the event context is preserved across asynchronous calls within the same
/// logical operation. It is suitable for scenarios where event context must flow with async operations, such as in web
/// request handling or background tasks.</para>
/// <para><strong>Lifecycle:</strong> The <see cref="AsyncLocal{T}"/> value automatically flows into child async
/// contexts but is isolated — changes in a child do not propagate back to the parent. In long-running or
/// high-throughput scenarios (e.g., background services or fire-and-forget tasks), always use
/// <see cref="EventContextScopeExtensions.BeginScope"/> to ensure the context is restored (or cleared)
/// when the operation completes:</para>
/// <code>
/// using var scope = accessor.BeginScope(eventContext);
/// // ... operations that need the context
/// // context is automatically restored on dispose
/// </code>
/// <para>For non-scoped cleanup (e.g., at the end of a background service iteration), call
/// <see cref="ClearCurrent"/>.</para>
/// </remarks>
public sealed class AsyncLocalEventContextAccessor : IEventContextAccessor
{
    private static readonly AsyncLocal<EventContext?> _current = new();

    /// <inheritdoc/>
    /// <remarks>Returns the default <see cref="EventContext"/> when no scope is active.</remarks>
    public EventContext Current => _current.Value ?? default;

    /// <summary>
    /// Sets the current event context for the logical execution flow.
    /// </summary>
    /// <param name="context">The event context to associate with the current flow.</param>
    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    internal void SetCurrent(EventContext context) => _current.Value = context;

    /// <summary>
    /// Clears the current event context, removing any value from the async-local slot.
    /// </summary>
    /// <remarks>
    /// Use this method at the end of a logical operation in non-scoped scenarios (e.g., background services)
    /// to prevent stale context from being visible in subsequent iterations or pooled threads.
    /// In ASP.NET Core middleware, prefer <see cref="EventContextScopeExtensions.BeginScope"/> instead,
    /// which automatically restores the previous context on dispose.
    /// </remarks>
    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    internal void ClearCurrent() => _current.Value = null;
}
