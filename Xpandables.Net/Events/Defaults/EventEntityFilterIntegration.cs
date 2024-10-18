
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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events.Defaults;

/// <summary>
/// Represents a filter for event entity integration.
/// </summary>
public sealed record EventEntityFilterIntegration :
    EntityFilter<EventEntityIntegration>,
    IEventFilter<EventEntityIntegration>
{
    /// <inheritdoc/>
    [SetsRequiredMembers]
    public EventEntityFilterIntegration() : base() { }

    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? EventDataPredicate { get; init; }
}

/// <summary>
/// Represents a filter for event entity integration with a specified result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed record EventEntityIntegrationFilter<TResult> :
    EntityFilter<EventEntityIntegration, TResult>,
    IEventFilter<EventEntityIntegration, TResult>
{
    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? EventDataPredicate { get; init; }
    /// <inheritdoc/>
    public override required
        Expression<Func<EventEntityIntegration, TResult>> Selector
    { get; init; }
}
