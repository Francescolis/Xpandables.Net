
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Optionals;

namespace Xpandables.Net.Responsibilities;

/// <summary>
/// Represents a command aggregate that contains the aggregate and its key 
/// identifier.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public abstract record CommandAggregate<TAggregate> : ICommandAggregate<TAggregate>
    where TAggregate : class, IAggregate, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAggregate{TAggregate}"/> class.
    /// </summary>
    protected CommandAggregate() { }

    /// <inheritdoc/>
    public Type AggregateType => typeof(TAggregate);

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="CommandAggregate{TAggregate}"/> class with the specified 
    /// key identifier.
    /// </summary>
    /// <param name="keyId">The key identifier.</param>
    protected CommandAggregate(Guid keyId) => KeyId = keyId;

    /// <inheritdoc/>
    public Optional<TAggregate> Aggregate { get; set; } = Optional.Empty<TAggregate>();

    /// <inheritdoc/>
    public Guid KeyId { get; init; }

    /// <inheritdoc/>
    public virtual bool ContinueWhenNotFound => false;
}
