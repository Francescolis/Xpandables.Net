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
namespace System.Events.Integration;

/// <summary>
/// Defines a contract for enriching integration events with additional metadata or context information required for
/// event processing.
/// </summary>
public interface IIntegrationEventEnricher
{
    /// <summary>
    /// Enriches the specified integration event with additional metadata or context information required for
    /// processing.
    /// </summary>
    /// <typeparam name="TIntegrationEvent">The type of integration event to enrich. Must be a class that implements <see cref="IIntegrationEvent"/>.</typeparam>
    /// <param name="event">The integration event instance to enrich. Cannot be <see langword="null"/>.</param>
    /// <returns>The enriched integration event instance. The returned object is of the same type as the input event.</returns>
    [Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
    TIntegrationEvent Enrich<TIntegrationEvent>(TIntegrationEvent @event)
        where TIntegrationEvent : class, IIntegrationEvent;
}