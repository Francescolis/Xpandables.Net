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
using System.Linq.Expressions;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Applies event filters to the query.
/// </summary>
public abstract class EventEntityFilter
{
    /// <summary>
    /// Gets the event entity type being filtered by the current filter instance.
    /// </summary>
    public abstract Type? Type { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the filter 
    /// instance can apply filters the specified object type.
    /// </summary>
    /// <param name="typeToFilter">The type of the object to apply filter.</param>
    /// <returns><see langword="true"/> if the instance can apply filters the 
    /// specified object type; otherwise, <see langword="false"/>.</returns>
    public abstract bool CanFilter(Type typeToFilter);
}

/// <summary>
/// Applies event filters to the query.
/// </summary>
/// <typeparam name="TEventEntity">The type of the event entity.</typeparam>
public abstract class EventEntityFilter<TEventEntity> : EventEntityFilter
    where TEventEntity : class
{
    /// <inheritdoc/>
    public sealed override Type? Type => typeof(TEventEntity);

    /// <summary>
    /// Gets the filter for a query.
    /// </summary>
    /// <param name="eventFilter">The event filter to be applied.</param>
    /// <returns>The expression filter to be applied.</returns>
    public abstract Expression<Func<TEventEntity, bool>> Filter(
        IEventFilter eventFilter);
}