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
namespace System.Events.Domain;

/// <summary>
/// Provides a default implementation of <see cref="IDomainEventEnricher"/> that enriches domain events with correlation and
/// causation identifiers from the current event context.
/// </summary>
/// <remarks>This class automatically sets the <c>CorrelationId</c> and <c>CausationId</c> properties on domain
/// events if they are not already specified, using values from the current event context. This helps maintain event
/// traceability across distributed systems and message flows.</remarks>
/// <param name="accessor">The accessor used to retrieve the current event context. Cannot be null.</param>
public sealed class DefaultDomainEventEnricher(IEventContextAccessor accessor) : IDomainEventEnricher
{
    private readonly IEventContextAccessor _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

    /// <inheritdoc/>
    public TDomainEvent Enrich<TDomainEvent>(TDomainEvent @event)
        where TDomainEvent : class, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

		EventContext context = _accessor.Current;

        if (@event.CorrelationId is null && context.CorrelationId is not null)
        {
            @event = (TDomainEvent)@event.WithCorrelation(context.CorrelationId);
        }

        if (@event.CausationId is null && context.CausationId is not null)
        {
            @event = (TDomainEvent)@event.WithCausation(context.CausationId);
        }

        return @event;
    }
}