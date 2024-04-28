﻿/*******************************************************************************
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
********************************************************************************/
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines a marker interface to be used to mark an object to 
/// act as a domain event for a specific aggregate.
/// </summary>
public interface IEventDomain : IEvent
{
    /// <summary>
    /// Sets the version of the associated aggregate.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>The same event with the new version.</returns>
    IEventDomain WithVersion(ulong version);

    /// <summary>
    /// Gets the identifier of the associated aggregate.
    /// </summary>
    IAggregateId AggregateId { get; }
}

/// <summary>
/// Defines a marker interface to be used to mark an object to 
/// act as a domain event for a specific aggregate.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate id.</typeparam>
public interface IEventDomain<TAggregateId> : IEventDomain
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    /// <summary>
    /// Sets the version of the associated aggregate.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>The same event with the new version.</returns>
    new IEventDomain<TAggregateId> WithVersion(ulong version);

    IEventDomain IEventDomain.WithVersion(ulong version)
        => WithVersion(version);

    /// <summary>
    /// Gets the identifier of the associated aggregate.
    /// </summary>
    new TAggregateId AggregateId { get; }

    IAggregateId IEventDomain.AggregateId => AggregateId;
}