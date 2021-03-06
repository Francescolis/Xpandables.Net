﻿/************************************************************************************************************
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

using Xpandables.Net.Aggregates;

namespace Xpandables.Net.DomainEvents
{
    /// <summary>
    /// This is the <see langword="abstract"/> class that implements <see cref="IDomainEvent{TAggregateId}"/>.
    /// </summary>
    /// <typeparam name="TAggregateId">The type of the aggregate identity.</typeparam>
    [Serializable]
    public abstract class DomainEvent<TAggregateId> : Event<TAggregateId>, IDomainEvent<TAggregateId>, IEquatable<DomainEvent<TAggregateId>>
        where TAggregateId : notnull, AggregateId
    {
        /// <summary>
        /// Constructs a new instance of <see cref="DomainEvent{TAggregateId}"/>.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="version">The version of the  related aggregate.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="aggregateId"/> is null.</exception>
        protected DomainEvent(TAggregateId aggregateId, AggregateVersion version) : base(aggregateId) => Version = version;

        ///<inheritdoc/>
        public AggregateVersion Version { get; protected set; }

        ///<inheritdoc/>
        public bool Equals(DomainEvent<TAggregateId>? other)
        {
            if (other is null)
                return false;

            return AggregateId.Equals(other.AggregateId)
                && Guid.Equals(other.Guid)
                && Version.Equals(other.Version);
        }

        ///<inheritdoc/>
        public virtual IDomainEvent<TAggregateId> WithAggregate(TAggregateId aggregateId, AggregateVersion version)
        {
            AggregateId = aggregateId;
            Version = version;
            return this;
        }

        ///<inheritdoc/>
        public override bool Equals(object? obj) => obj is DomainEvent<TAggregateId> domainEvent && Equals(domainEvent);

        ///<inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(AggregateId.Value, Guid, Version);
    }
}
