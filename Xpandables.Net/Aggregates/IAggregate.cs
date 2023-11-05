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
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines base properties for an aggregate that is identified by a <see cref="Guid"/> type.
/// Aggregate is a pattern in Domain-Driven Design. A DDD aggregate is a cluster
/// of objects that can be treated as a single unit.
/// </summary>
/// <typeparam name="TAggregateId">The type of aggregate Id</typeparam>
public interface IAggregate<TAggregateId> : IDomainEventSourcing<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    /// <summary>
    /// Gets the aggregate identifier.
    /// </summary>
    TAggregateId AggregateId { get; }

    /// <summary>
    /// Gets the current version of the instance.
    /// </summary>
    ulong Version { get; }

    /// <summary>
    /// Determines whether or not the underlying instance is a empty one.
    /// </summary>
    /// <remarks>This property is used when creating aggregate from history 
    /// to determine if the id has been set or not.
    /// You can override it to customize its behavior.</remarks>
    /// <returns>Returns <see langword="true"/> if so, otherwise <see langword="false"/></returns>
    public virtual bool IsEmpty => AggregateId.IsNew();
}