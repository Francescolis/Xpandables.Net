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
using System.ComponentModel;

namespace Xpandables.Net.EventSourcing.Aggregates;

/// <summary>
/// Defines a factory for creating new instances of aggregates that implement the <see cref="IAggregate"/> interface.
/// </summary>
/// <remarks>Implementations of this interface provide a standardized way to instantiate aggregates, typically
/// with default or initial state. The specific aggregate type and its initialization behavior are determined by the
/// implementing class.</remarks>
public interface IAggregateFactory
{
    /// <summary>
    /// Initializes a new instance of an aggregate.
    /// </summary>
    /// <remarks>This method is typically used to instantiate a default or empty aggregate. The specific type
    /// and initial state of the returned aggregate depend on the implementing class.</remarks>
    /// <returns>An <see cref="IAggregate"/> instance representing a newly created aggregate.</returns>
    static abstract IAggregate Initialize();
}

/// <summary>
/// Defines a factory interface for creating new instances of aggregates of a specified type.
/// </summary>
/// <remarks>Implementations of this interface provide a standardized way to instantiate aggregates, typically
/// used in domain-driven design scenarios. The initial state of the created aggregate is determined by the concrete
/// implementation.</remarks>
/// <typeparam name="TAggregate">The type of aggregate to be created. Must implement <see cref="IAggregate"/> and <see
/// cref="IAggregateFactory{TAggregate}"/>.</typeparam>
public interface IAggregateFactory<TAggregate> : IAggregateFactory
    where TAggregate : class, IAggregate, IAggregateFactory<TAggregate>
{
    /// <summary>
    /// Initializes a new instance of an aggregate of type <typeparamref name="TAggregate"/>.
    /// </summary>
    /// <remarks>This method is typically used to instantiate a default or empty aggregate of the specified type.
    /// The specific initial state of the returned aggregate depends on the implementing class.</remarks>
    /// <returns>An instance of <typeparamref name="TAggregate"/> representing a newly created aggregate.</returns>
    static new abstract TAggregate Initialize();

    [EditorBrowsable(EditorBrowsableState.Never)]
    static IAggregate IAggregateFactory.Initialize() => TAggregate.Initialize();
}