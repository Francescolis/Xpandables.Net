/************************************************************************************************************
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
namespace Xpandables.Net.IntegrationEvents;

/// <summary>
/// Out-box pattern interface (integration event) that allows integration event to be outside published.
/// </summary>
public interface IIntegrationEventSourcing
{
    /// <summary>
    /// Appends the <see cref="IIntegrationEvent"/>.
    /// </summary>
    /// <param name="event">The integration event to be used.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unbale to append the specified integration event.</exception>
    void Append(IIntegrationEvent @event);

    /// <summary>
    /// Returns a collection of registered integration events for the control flow.
    /// </summary>
    /// <returns>An ordered list of integration events.</returns>
    IEnumerable<IIntegrationEvent> GetIntegrationEvents();

    /// <summary>
    /// Marks all integration events as committed.
    /// </summary>
    void MarkIntegrationEventsAsCommitted();
}