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

namespace Xpandables.Net.EventSourcing;

/// <summary>
/// Represents a thread-safe collection of domain events that are pending processing.
/// </summary>
/// <remarks>This class is designed to manage domain events that need to be processed in batches.  It provides
/// methods to add new events and retrieve all pending events in a thread-safe manner.</remarks>
public sealed class PendingDomainEventsBuffer : IPendingDomainEventsBuffer
{
    private readonly ConcurrentQueue<PendingDomainEventsBatch> _queue = [];

    /// <inheritdoc />
    public void AddRange(IReadOnlyCollection<IDomainEvent> events, Action onCommitted)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(onCommitted);
        if (events.Count == 0) return;

        _queue.Enqueue(new PendingDomainEventsBatch { Events = events, OnCommitted = onCommitted });
    }

    /// <inheritdoc />
    public IReadOnlyCollection<PendingDomainEventsBatch> Drain()
    {
        var list = new List<PendingDomainEventsBatch>();
        while (_queue.TryDequeue(out var batch))
        {
            list.Add(batch);
        }

        return list.AsReadOnly();
    }
}
