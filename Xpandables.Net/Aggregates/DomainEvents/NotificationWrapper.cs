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

using Xpandables.Net.Aggregates.Notifications;

namespace Xpandables.Net.Aggregates.DomainEvents;

/// <summary>
/// Defines a marker interface to be used to mark an object to act as 
/// a notification for a specific domain event.
/// </summary>
/// <typeparam name="TDomainEvent">The type the target domain event.</typeparam>
/// <typeparam name="TAggregateId">the aggregate Id type.</typeparam>
public interface INotification<out TDomainEvent, out TAggregateId> : INotification
    where TDomainEvent : notnull, IDomainEvent<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    /// <summary>
    /// Gets the domain event associated with the notification.
    /// </summary>
    TDomainEvent DomainEvent { get; }
}

/// <summary>
/// Represents an Event Router helper class used to wrap a domain 
/// event into a notification.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event.</typeparam>
/// <typeparam name="TAggregateId">the aggregate Id type.</typeparam>
public sealed record NotificationWrapper<TDomainEvent, TAggregateId> :
    Notification, INotification<TDomainEvent, TAggregateId>
    where TDomainEvent : notnull, IDomainEvent<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    ///<inheritdoc/>
    public TDomainEvent DomainEvent { get; }

    /// <summary>
    /// Defines a new instance of <see cref="NotificationWrapper{TDomainEvent, TAggregateId}"/> 
    /// using the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to be wrapped.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="domainEvent"/> is null.</exception>
    public NotificationWrapper(TDomainEvent domainEvent)
        => DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
}
