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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents an integration event that serves as a contract for domain-driven communication.
/// </summary>
public interface IIntegrationEvent : IEvent
{
}

/// <summary>
/// Represents an integration event used for domain-driven communication and system integration.
/// </summary>
public interface IIntegrationEvent<TDomainEvent> : IIntegrationEvent
    where TDomainEvent : notnull, IDomainEvent
{
}

/// <summary>
/// Represents a base integration event for facilitating communication within a domain or between systems.
/// </summary>
public record IntegrationEvent : Event, IIntegrationEvent
{
}

/// <summary>
/// Represents an integration event with a specific domain event.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
public record IntegrationEvent<TDomainEvent> : IntegrationEvent, IIntegrationEvent<TDomainEvent>
    where TDomainEvent : notnull, IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationEvent{TDomainEvent}" /> class.
    /// </summary>
    [JsonConstructor]
    protected IntegrationEvent() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationEvent{TDomainEvent}" /> class.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    [SetsRequiredMembers]
    protected IntegrationEvent(TDomainEvent domainEvent) => EventId = domainEvent.EventId;
}