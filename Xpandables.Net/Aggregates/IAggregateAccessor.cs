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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Xpandables.Net.Database;
using Xpandables.Net.Decorators;

namespace Xpandables.Net.Aggregates
{
    /// <summary>
    /// Represents a set of methods to read/write aggregates from an event store.
    /// For persistence, decorate your command/event with <see cref="IAggregatePersistenceDecorator"/> interface.
    /// </summary>
    /// <typeparam name="TAggregateId">The type of the aggregate identity.</typeparam>
    /// <typeparam name="TAggregate">The type of the target aggregate.</typeparam>
    public interface IAggregateAccessor<TAggregateId, TAggregate> :
        ISnapShotAccessor<TAggregateId, TAggregate>, IDomainEventAccessor<TAggregateId, TAggregate>,
        INotificationEventAccessor<TAggregateId, TAggregate>, IEmailEventAccessor<TAggregateId, TAggregate>
        where TAggregate : class, IAggregate<TAggregateId>
        where TAggregateId : notnull, IAggregateId
    {
        /// <summary>
        /// Gets or sets the current <see cref="JsonSerializerOptions"/> to be used for serialization.
        /// </summary>
        JsonSerializerOptions? SerializerOptions { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="JsonDocumentOptions"/> to be used for <see cref="JsonDocument"/> parsing.
        /// </summary>
        JsonDocumentOptions DocumentOptions { get; set; }

        /// <summary>
        /// Asynchronously returns the <typeparamref name="TAggregate"/> aggregate that matches the 
        /// specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier to search for.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents an object of <typeparamref name="TAggregate"/> type if found or null.</returns>
        Task<TAggregate?> ReadAsync(TAggregateId aggregateId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously appends the specified <typeparamref name="TAggregate"/> aggregate to the event store.
        /// </summary>
        /// <param name="aggregate">The aggregate to act on.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents an asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="aggregate"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="aggregate"/> must 
        /// implement <see cref="IDomainEventSourcing{TAggregateId}"/> interface.</exception>
        Task AppendAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously returns the <typeparamref name="TAggregate"/> aggregate that matches the specified
        /// aggregate identifier from its snapShot. The aggregate must implement <see cref="IOriginator"/> interface.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier to search for.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents an object of <typeparamref name="TAggregateId"/> type if found or null.</returns>
        Task<TAggregate?> ReadFromSnapShot(TAggregateId aggregateId, CancellationToken cancellationToken = default);
    }
}
