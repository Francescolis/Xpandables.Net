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
using Xpandables.Net.Distribution;
using Xpandables.Net.Expressions;

namespace Xpandables.Net.Aggregates.Events;

/// <summary>
/// Represents the filter for <see cref="EntityEventDomain"/>.
/// </summary>
public sealed record EntityEventDomainFilter : EventFilter<EntityEventDomain>
{
    /// <inheritdoc/>
    public override Type Type => typeof(IEventDomain);

    ///<inheritdoc/>
    public override bool CanFilter(Type typeToFilter)
        => Type.IsAssignableFrom(typeToFilter);

    ///<inheritdoc/>
    protected override QueryExpression<EntityEventDomain> CriteriaBuilder()
    {
        QueryExpression<EntityEventDomain> expression = base.CriteriaBuilder();

        if (KeyId is not null)
        {
            expression = expression.And(x => x.AggregateId == KeyId.Value);
        }

        if (AggregateName is not null)
        {
            expression = expression.And(x =>
                x.AggregateTypeName.Contains(AggregateName));
        }

        return expression;
    }
}

/// <summary>
/// Represents the filter for <see cref="EntityEventIntegration"/>.
/// </summary>
public sealed record EntityEventIntegrationFilter : EventFilter<EntityEventIntegration>
{
    /// <inheritdoc/>
    public override Type Type => typeof(IEventIntegration);
    ///<inheritdoc/>
    public override bool CanFilter(Type typeToFilter)
         => Type.IsAssignableFrom(typeToFilter);

    ///<inheritdoc/>
    protected override QueryExpression<EntityEventIntegration> CriteriaBuilder()
    {
        QueryExpression<EntityEventIntegration> expression = base.CriteriaBuilder();

        if (OnError is not null)
        {
            expression = OnError.Value
                ? expression
                    .And(x => x.ErrorMessage != null)
                : expression
                    .And(x => x.ErrorMessage == null);
        }

        return expression;
    }
}

/// <summary>
/// Represents the filter for <see cref="EntityEventSnapshot"/>.
/// </summary>
public sealed record EntityEventSnapshotFilter : EventFilter<EntityEventSnapshot>
{
    /// <inheritdoc/>
    public override Type Type => typeof(IEventSnapshot);

    ///<inheritdoc/>
    public override bool CanFilter(Type typeToFilter)
         => Type.IsAssignableFrom(typeToFilter);
}