﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Xpandables.Net.EmailEvents;

namespace Xpandables.Net.Aggregates
{
    /// <summary>
    /// Provides with methods to retrieve and persist email events.
    /// </summary>
    /// <typeparam name="TAggregateId">The type of the aggregate identity.</typeparam>
    /// <typeparam name="TAggregate">The type of the target aggregate.</typeparam>
    public interface IEmailEventAccessor<TAggregateId, TAggregate>
        where TAggregateId : notnull, IAggregateId
        where TAggregate : notnull, IAggregate<TAggregateId>
    {
        /// <summary>
        /// Asynchronously returns a collection of email events matching the criteria.
        /// if not found, returns an empty collection.
        /// </summary>
        /// <typeparam name="TEmailMessage">the type of the message.</typeparam>
        /// <param name="criteria">The criteria to be applied to entities.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>An enumerator of <see cref="IEmailEvent{TEmailMessage}"/> that can be asynchronously enumerated.</returns>
        IAsyncEnumerable<IEmailEvent<TEmailMessage>> ReadAllEmailEventsAsync<TEmailMessage>(
            EventStoreEntityCriteria<EmailEventStoreEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEmailMessage : notnull;

        /// <summary>
        /// Asynchronously appends the specified email event.
        /// </summary>
        /// <typeparam name="TEmailMessage">the type of the message.</typeparam>
        /// <param name="event">Then target email to be appended.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents an asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
        Task AppendEmailEventAsync<TEmailMessage>(IEmailEvent<TEmailMessage> @event, CancellationToken cancellationToken = default)
            where TEmailMessage : notnull;
    }
}
