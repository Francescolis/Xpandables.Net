
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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work that manages event-related operations within a transactional context.
/// </summary>
/// <remarks>This interface extends <see cref="IUnitOfWork"/> to include operations specific to event management.
/// Implementations should ensure that all event operations are completed successfully before committing the
/// transaction, providing atomicity and consistency.</remarks>
public interface IUnitOfWorkEvent : IUnitOfWorkBase
{
    /// <summary>
    /// Retrieves an instance of the specified event store type.
    /// </summary>
    /// <remarks>The method returns an instance of the event store that matches the specified type parameter.
    /// Ensure that the type parameter is a class implementing <see cref="IEventStore"/>.</remarks>
    /// <typeparam name="TEventStore">The type of event store to retrieve. 
    /// Must be a class that implements <see cref="IEventStore"/>.</typeparam>
    /// <returns>An instance of <typeparamref name="TEventStore"/>.</returns>
    IEventStore GetEventStore<TEventStore>()
        where TEventStore : class, IEventStore;
}

/// <summary>
/// Represents a unit of work pattern for handling events within a specified data context.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context used by the unit of work. Must be a reference type.</typeparam>
public interface IUnitOfWorkEvent<TDataContext> : IUnitOfWorkEvent
    where TDataContext : class;