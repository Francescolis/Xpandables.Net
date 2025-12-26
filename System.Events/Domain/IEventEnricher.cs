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
/// Defines a contract for enriching domain events with additional metadata or contextual information prior to
/// publication or processing.
/// </summary>
/// <remarks>Implementations of this interface can be used to augment domain events with data such as correlation
/// identifiers, timestamps, or other information required by downstream consumers. Enrichment is typically performed
/// before events are dispatched to handlers or external systems.</remarks>
public interface IEventEnricher
{
    /// <summary>
    /// Enriches the specified domain event with additional metadata or context information.
    /// </summary>
    /// <remarks>This method is typically used to augment domain events before they are published or
    /// processed. The enrichment may include adding correlation identifiers, timestamps, or other contextual data
    /// required by downstream consumers.</remarks>
    /// <typeparam name="TDomainEvent">The type of domain event to enrich. Must implement <see cref="IDomainEvent"/>.</typeparam>
    /// <param name="event">The domain event to be enriched. Cannot be <see langword="null"/>.</param>
    /// <returns>The enriched domain event instance. Returns the same instance with added information, or a new instance if
    /// enrichment requires replacement.</returns>
    [Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
    TDomainEvent Enrich<TDomainEvent>(TDomainEvent @event)
        where TDomainEvent : class, IDomainEvent;
}