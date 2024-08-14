/*******************************************************************************
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
namespace Xpandables.Net.Distribution;

/// <summary>
/// Allows application author to map domain event to integration event 
/// in a Centralized Event Mapper.
/// </summary>
/// <remarks>The implementation should be a singleton class or 
/// a class with singleton lifetime.</remarks>
public interface IEventDomainMapper
{
    /// <summary>
    /// Maps the specified domain event to an integration event.
    /// </summary>
    /// <param name="event">The domain event to be mapped.</param>
    /// <returns>An integration event from the domain event mapped.</returns>
    IEventIntegration Map(IEventDomain @event);

    /// <summary>
    /// Maps the specified domain events to integration events.
    /// </summary>
    /// <param name="events">The collection of domain events to be  
    /// mapped.</param>
    /// <returns>A collection of integration events from the domain events mapped
    /// .</returns>
    public IEnumerable<IEventIntegration> MapAll(
        IEnumerable<IEventDomain> events)
        => events.Select(Map);
}
