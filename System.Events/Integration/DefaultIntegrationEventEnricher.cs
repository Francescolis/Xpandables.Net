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
using System.Events.Domain;

namespace System.Events.Integration;

/// <summary>
/// Provides a default implementation of the integration event enricher that populates correlation and causation
/// identifiers on integration events using the current event context and any wrapped domain events.
/// </summary>
/// <remarks>This enricher ensures that integration events have their CorrelationId and CausationId properties
/// set, preferring values already present on the event, then those from a wrapped domain event, and finally those from
/// the ambient event context. This is useful for maintaining event traceability across distributed systems.</remarks>
/// <param name="accessor">The accessor used to retrieve the current event context. Cannot be null.</param>
public sealed class DefaultIntegrationEventEnricher(IEventContextAccessor accessor) : IIntegrationEventEnricher
{
    private readonly IEventContextAccessor _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

    /// <inheritdoc/>
    public TIntegrationEvent Enrich<TIntegrationEvent>(TIntegrationEvent @event)
        where TIntegrationEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        // Prefer values already set on the integration event
        var correlationId = @event.CorrelationId;
        var causationId = @event.CausationId;

        // If it wraps a domain event, prefer that next
        if (@event is IntegrationEvent<IDomainEvent> wrapper)
        {
            correlationId ??= wrapper.DomainEvent.CorrelationId;
            causationId ??= wrapper.DomainEvent.CausationId;
        }

        // Finally fallback to ambient context
        var context = _accessor.Current;
        correlationId ??= context.CorrelationId;
        causationId ??= context.CausationId;

        if (correlationId == @event.CorrelationId && causationId == @event.CausationId)
        {
            return @event;
        }

        if (correlationId.HasValue) @event.WithCorrelation(correlationId.Value);
        if (causationId.HasValue) @event.WithCausation(causationId.Value);

        return @event;
    }
}