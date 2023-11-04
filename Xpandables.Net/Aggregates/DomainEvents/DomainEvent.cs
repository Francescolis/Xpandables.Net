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
namespace Xpandables.Net.Aggregates.DomainEvents;

/// <summary>
/// Helper class used to create a domain event with aggregate.
/// </summary>
/// <typeparam name="TAggregateId">The type of aggregate.</typeparam>
/// <remarks>Initializes a new instance of <see cref="DomainEvent{TAggregateId}"/>.</remarks>
public abstract record class DomainEvent<TAggregateId> : IDomainEvent<TAggregateId>
    where TAggregateId : struct, IAggregateId<TAggregateId>
{
    ///<inheritdoc/>
    public required ulong Version { get; init; }

    ///<inheritdoc/>
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    ///<inheritdoc/>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <inheritdoc/>
    public IDomainEvent<TAggregateId> WithVersion(ulong version)
        => this with { Version = Version + 1 };

    /// <inheritdoc/>
    public required TAggregateId AggregateId { get; init; }
}
