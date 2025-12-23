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
/// Defines a factory for creating instances of the outbox event data context.
/// </summary>
/// <remarks>Implementations of this interface are responsible for providing new instances of the data context
/// used to interact with the outbox store. This is typically used to ensure that each operation has its own context
/// instance, supporting scenarios such as dependency injection and unit of work patterns.</remarks>
public interface IOutboxStoreDataContextFactory
{
    /// <summary>
    /// Creates a new instance of the EventDataContext for interacting with event data.
    /// </summary>
    /// <returns>An EventDataContext instance that can be used to query and manipulate event data.</returns>
    EventDataContext Create();
}

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated via dependency injection or reflection.")]
internal sealed class OutboxStoreDataContextFactory(OutboxStoreDataContext context) : IOutboxStoreDataContextFactory
{
    private readonly EventDataContext _context = context ?? throw new ArgumentNullException(nameof(context));
    /// <inheritdoc/>
    public EventDataContext Create() => _context;
}