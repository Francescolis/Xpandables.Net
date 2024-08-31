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
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Distribution;

/// <summary>
/// This interface is used as a marker for requests targeting aggregate. 
/// It's used for implementing the Decider pattern. 
/// </summary>
public interface IRequestAggregate<TAggregate> : IAggregateDecorator
    where TAggregate : IAggregate
{
    /// <summary>
    /// Gets or sets the aggregate instance.
    /// </summary>
    /// <remarks>This get populated by the decorator.</remarks>
    Optional<TAggregate> Aggregate { get; set; }

    /// <summary>
    /// Gets the key aggretate identitifer
    /// </summary>
    Guid KeyId { get; }
}
