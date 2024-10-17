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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;

using Xpandables.Net.Events.Entities;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events.Filters;

/// <summary>
/// Represents a filter for event entity domains.
/// </summary>
public sealed record EventEntityDomainFilter :
    EntityFilter<EventEntityDomain>,
    IEventFilter<EventEntityDomain>
{
    /// <inheritdoc/>
    [SetsRequiredMembers]
    public EventEntityDomainFilter() : base() { }

    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? EventDataPredicate { get; init; }
}

/// <summary>
/// Represents a filter for event entity domains with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed record EventEntityDomainFilter<TResult> :
    EntityFilter<EventEntityDomain, TResult>,
    IEventFilter<EventEntityDomain, TResult>
{
    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? EventDataPredicate { get; init; }

    /// <inheritdoc/>
    public override required
        Expression<Func<EventEntityDomain, TResult>> Selector
    { get; init; }
}
