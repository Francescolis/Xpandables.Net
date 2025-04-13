﻿/*******************************************************************************
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Domains;
/// <summary>
/// Represents an integration event that is part of the event-driven architecture.
/// </summary>
public interface IEventIntegration : IEvent
{
}

/// <summary>
/// Represents an integration event with a specific event domain.
/// </summary>
/// <typeparam name="TEventDomain">The type of the event domain.</typeparam>
public interface IEventIntegration<TEventDomain> : IEventIntegration
    where TEventDomain : notnull, IEventDomain
{
}

/// <summary>
/// Represents an integration event that is used to communicate between 
/// different systems.
/// </summary>
public record EventIntegration : Event, IEventIntegration
{
}

/// <summary>
/// Represents an integration event with a specific event domain.
/// </summary>
/// <typeparam name="TEventDomain">The type of the event domain.</typeparam>
public record EventIntegration<TEventDomain> : EventIntegration, IEventIntegration<TEventDomain>
    where TEventDomain : notnull, IEventDomain
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventIntegration{TEventDomain}"/> class.
    /// </summary>
    [JsonConstructor]
    protected EventIntegration() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventIntegration{TEventDomain}"/> class.
    /// </summary>
    /// <param name="eventDomain">The event domain.</param>
    [SetsRequiredMembers]
    protected EventIntegration(TEventDomain eventDomain) => EventId = eventDomain.EventId;
}