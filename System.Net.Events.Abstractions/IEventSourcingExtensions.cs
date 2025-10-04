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
namespace System.Net.Events;

/// <summary>
/// Provides extension methods for working with event-sourced aggregates.
/// </summary>
/// <remarks>This class contains utility methods to simplify common operations on aggregates that implement the
/// <see cref="IEventSourcing"/> interface, such as replaying event histories and managing uncommitted events.</remarks>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IEventSourcingExtensions
{
    extension(IEventSourcing source)
    {
        /// <summary>
        /// Replays the specified history into the aggregate.
        /// </summary>
        public void Replay(IEnumerable<IDomainEvent> history)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(history);
            source.LoadFromHistory(history);
        }

        /// <summary>
        /// Returns a snapshot of uncommitted events and clears the buffer.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DequeueUncommittedEvents()
        {
            ArgumentNullException.ThrowIfNull(source);
            var events = source.GetUncommittedEvents();
            if (events.Count > 0)
            {
                source.MarkEventsAsCommitted();
            }
            return events;
        }
    }
}