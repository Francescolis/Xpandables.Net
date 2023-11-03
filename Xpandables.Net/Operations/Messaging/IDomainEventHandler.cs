﻿/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations.Messaging;

/// <summary>
/// Represents a method signature to be used to apply 
/// <see cref="IDomainEventHandler{TAggregateId, TEvent}"/> implementation.
/// </summary>
/// <typeparam name="T">The event type.</typeparam>
/// <param name="event">The event instance to act on.</param>
/// <param name="cancellationToken">A CancellationToken to observe 
/// while waiting for the task to complete.</param>
/// <returns>A value that represents an implementation of <see cref="IOperationResult"/>.</returns>
public delegate ValueTask<OperationResult> DomainEventHandler<in T>(
    T @event, CancellationToken cancellationToken = default);

/// <summary>
/// Allows an application author to define a generic handler for domain events in a Distributed Event Mapper.
/// The domain event must implement <see cref="IDomainEvent{TAggregateId}"/> interface.
/// The implementation must be thread-safe when working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TAggregateId">The type of  aggregate Id</typeparam>
/// <typeparam name="TDomainEvent">The domain event type.</typeparam>
public interface IDomainEventHandler<in TDomainEvent, in TAggregateId>
    where TDomainEvent : notnull, IDomainEvent<TAggregateId>
    where TAggregateId : struct, IPrimitive<TAggregateId, Guid>
{
    /// <summary>
    ///  Asynchronously handles the domain event of specific type.
    /// </summary>
    /// <remarks>The result of the handler will be used by the control flow to determine
    /// whether or not to continue the execution process.</remarks>
    /// <param name="event">The domain event instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    /// <returns>A value that represents an <see cref="OperationResult"/>.</returns>
    ValueTask<OperationResult> HandleAsync(
        TDomainEvent @event, CancellationToken cancellationToken = default);
}