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

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Represents a filter for querying entities with specified criteria.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public record EntityFilter<TEntity, TResult> : IEntityFilter<TEntity, TResult>
    where TEntity : class, IEntity
{
    /// <inheritdoc/>
    public ushort PageIndex { get; init; }

    /// <inheritdoc/>
    public ushort PageSize { get; init; }

    /// <inheritdoc/>
    public int TotalCount { get; set; }

    /// <inheritdoc/>
    public required Expression<Func<TEntity, TResult>> Selector { get; init; }

    /// <inheritdoc/>
    public Expression<Func<TEntity, bool>>? Predicate { get; init; }

    /// <inheritdoc/>
    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; init; }
}

/// <summary>
/// Represents a filter for querying entities with specified criteria.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public record EntityFilter<TEntity> : EntityFilter<TEntity, TEntity>, IEntityFilter<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFilter{TEntity}"/> class.
    /// </summary>
    [SetsRequiredMembers]
#pragma warning disable CS8618, CS9264
    public EntityFilter() : base() => Selector = entity => entity;
#pragma warning restore CS8618, CS9264
}