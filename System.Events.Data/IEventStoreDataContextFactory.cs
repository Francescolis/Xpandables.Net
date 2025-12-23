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
using System.Diagnostics.CodeAnalysis;

namespace System.Events.Data;

/// <summary>
/// Defines a factory for creating instances of EventDataContext used to interact with event data in the event store.
/// </summary>
/// <remarks>Implementations of this interface are responsible for providing properly configured EventDataContext
/// instances. Each call to Create should return a new, independent context. This interface is typically used to
/// abstract the creation of data contexts for dependency injection or testing scenarios.</remarks>
public interface IEventStoreDataContextFactory
{
    /// <summary>
    /// Creates a new instance of the EventDataContext for interacting with event data.
    /// </summary>
    /// <returns>An EventDataContext instance that can be used to query and manipulate event data.</returns>
    EventDataContext Create();
}

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated via dependency injection or reflection.")]
internal sealed class EventStoreDataContextFactory(EventStoreDataContext context) : IEventStoreDataContextFactory
{
    private readonly EventDataContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc/>
    public EventDataContext Create() => _context;
}