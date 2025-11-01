﻿/*******************************************************************************
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

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a thread-safe collection of integration events that are pending processing.
/// </summary>
/// <remarks>This class provides methods to add, retrieve, and drain integration events in a thread-safe manner.
/// It is designed to support scenarios where integration events need to be queued and processed sequentially.</remarks>
public sealed class PendingIntegrationEventsBuffer : IPendingIntegrationEventsBuffer
{
    private readonly ConcurrentQueue<IIntegrationEvent> _queue = new();

    /// <inheritdoc/>
    public void Add(IIntegrationEvent eventInstance)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        _queue.Enqueue(eventInstance);
    }

    /// <inheritdoc/>
    public void AddRange(IEnumerable<IIntegrationEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        foreach (var e in events) _queue.Enqueue(e);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<IIntegrationEvent> Snapshot() => [.. _queue];

    /// <inheritdoc/>
    public IEnumerable<IIntegrationEvent> Drain()
    {
        while (_queue.TryDequeue(out var e)) yield return e;
    }
}