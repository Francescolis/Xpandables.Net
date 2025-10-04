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
namespace Xpandables.Net.Events;

/// <summary>
/// Represents an event intended for integration between different systems or services.
/// </summary>
/// <remarks>Integration events are typically used to communicate changes or actions across service boundaries in
/// distributed systems, such as in event-driven or microservices architectures. Implementations should ensure that
/// integration events are serializable and suitable for transport over messaging infrastructure.</remarks>
public interface IIntegrationEvent : IEvent;

/// <summary>
/// Defines a contract for integration events that encapsulate a domain event of a specified type.
/// </summary>
/// <remarks>Implementations of this interface are used to propagate domain events across service or application
/// boundaries, enabling integration between distributed systems. The generic type parameter allows for strong typing of
/// the underlying domain event.</remarks>
/// <typeparam name="TDomainEvent">The type of domain event associated with the integration event. Must implement <see cref="IDomainEvent"/> and cannot
/// be null.</typeparam>
public interface IIntegrationEvent<TDomainEvent> : IIntegrationEvent
    where TDomainEvent : notnull, IDomainEvent;

/// <summary>
/// Represents the base type for events intended to be published across service boundaries in an integration scenario.
/// </summary>
/// <remarks>Integration events are used to communicate significant changes or actions between different systems
/// or microservices. Derive from this type to define events that should be handled by external subscribers.</remarks>
public abstract record IntegrationEvent : BaseEvent, IIntegrationEvent;

/// <summary>
/// Represents the base integration event that encapsulates a domain event of the specified type for use in distributed
/// systems or event-driven architectures.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event associated with this integration event. Must implement <see cref="IDomainEvent"/> and
/// cannot be null.</typeparam>
public abstract record IntegrationEvent<TDomainEvent> : IntegrationEvent, IIntegrationEvent<TDomainEvent>
    where TDomainEvent : notnull, IDomainEvent
{
    /// <summary>
    /// Gets the domain event associated with this instance.
    /// </summary>
    public TDomainEvent DomainEvent { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationEvent{TDomainEvent}" /> class.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    protected IntegrationEvent(TDomainEvent domainEvent) =>
        DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
}