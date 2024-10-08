﻿
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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Optionals;

namespace Xpandables.Net;

/// <summary>
/// Represents a request that targets an aggregate.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
public abstract record RequestAggregate<TAggregate> : IRequestAggregate<TAggregate>
    where TAggregate : class, IAggregate
{
    /// <summary>
    /// Constructs a new instance of <see cref="RequestAggregate{TAggregate}"/>.
    /// </summary>
    protected RequestAggregate()
    {
    }

    /// <summary>
    /// Constructs a new instance of <see cref="RequestAggregate{TAggregate}"/>.
    /// </summary>
    protected RequestAggregate(Guid keyId) => KeyId = keyId;

    ///<inheritdoc/>
    public Optional<TAggregate> Aggregate { get; set; } = Optional.Empty<TAggregate>();

    ///<inheritdoc/>
    public Guid KeyId { get; init; }

    ///<inheritdoc/>
    public virtual bool ContinueWhenNotFound => false;
}
