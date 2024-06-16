
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

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents an Event Router helper class used to wrap a domain 
/// event into an integration event.
/// </summary>
/// <typeparam name="TEventDomain">The type of domain event.</typeparam>
public sealed record EventIntegrationWrapper<TEventDomain> :
    EventIntegration, IEventIntegration<TEventDomain>
    where TEventDomain : notnull, IEventDomain
{
    ///<inheritdoc/>
    public TEventDomain DomainEvent { get; }

    /// <summary>
    /// Defines a new instance of 
    /// <see cref="EventIntegrationWrapper{TEventDomain}"/> 
    /// using the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to be wrapped.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="domainEvent"/> is null.</exception>
    public EventIntegrationWrapper(TEventDomain domainEvent)
        => DomainEvent = domainEvent
            ?? throw new ArgumentNullException(nameof(domainEvent));
}
