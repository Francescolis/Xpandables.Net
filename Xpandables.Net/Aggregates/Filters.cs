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
using System.Text.Json;

using Xpandables.Net.Expressions;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents the filter for <see cref="EntityEventDomain"/>.
/// </summary>
public sealed class EventEntityDomainFilter :
    EventEntityFilter<EntityEventDomain>
{
    ///<inheritdoc/>
    public override bool CanFilter(Type typeToFilter)
        => typeToFilter == typeof(EntityEventDomain);

    ///<inheritdoc/>
    public override Expression<Func<EntityEventDomain, bool>> Filter(
        IEventFilter eventFilter)
    {
        QueryExpression<EntityEventDomain> expression
            = QueryExpressionFactory.Create<EntityEventDomain>();

        if (eventFilter.AggregateId is not null)
        {
            expression = expression.And(x =>
            x.AggregateId == eventFilter.AggregateId.Value);
        }

        if (eventFilter.AggregateName is not null)
        {
            expression = expression.And(x =>
            x.AggregateTypeName.Contains(
                    eventFilter.AggregateName));
        }

        if (eventFilter.Id is not null)
        {
            expression = expression.And(x => x.Id == eventFilter.Id);
        }

        if (eventFilter.EventTypeName is not null)
        {
            expression = expression.And(x =>
                x.EventTypeName.Contains(eventFilter.EventTypeName));
        }

        if (eventFilter.Version is not null)
        {
            expression = expression.And(x =>
            x.Version > eventFilter.Version.Value);
        }

        if (eventFilter.FromCreatedOn is not null)
        {
            expression = expression.And(x =>
            x.CreatedOn >= eventFilter.FromCreatedOn.Value);
        }

        if (eventFilter.ToCreatedOn is not null)
        {
            expression = expression.And(x =>
            x.CreatedOn <= eventFilter.ToCreatedOn.Value);
        }

        if (eventFilter.Status is not null)
        {
            expression = expression.And(x =>
            x.Status.Contains(eventFilter.Status));
        }

        if (eventFilter.DataCriteria is not null)
        {
            expression = expression.And(
                RepositoryExtensions
                .Compose<EntityEventDomain, JsonDocument, bool>(
                    x => x.Data,
                    eventFilter.DataCriteria));
        }

        return expression;
    }
}

/// <summary>
/// Represents the filter for <see cref="EntityEventIntegration"/>.
/// </summary>
public sealed class EventEntityIntegrationFilter :
    EventEntityFilter<EntityEventIntegration>
{
    ///<inheritdoc/>
    public override bool CanFilter(Type typeToFilter)
        => typeToFilter == typeof(EntityEventIntegration);

    ///<inheritdoc/>
    public override Expression<Func<EntityEventIntegration, bool>> Filter(
        IEventFilter eventFilter)
    {
        QueryExpression<EntityEventIntegration> expression
            = QueryExpressionFactory.Create<EntityEventIntegration>();

        if (eventFilter.Id is not null)
        {
            expression = expression.And(x => x.Id == eventFilter.Id);
        }

        if (eventFilter.EventTypeName is not null)
        {
            expression = expression.And(x =>
                x.EventTypeName.Contains(eventFilter.EventTypeName));
        }

        if (eventFilter.Version is not null)
        {
            expression = expression.And(x =>
            x.Version > eventFilter.Version.Value);
        }

        if (eventFilter.FromCreatedOn is not null)
        {
            expression = expression.And(x =>
            x.CreatedOn >= eventFilter.FromCreatedOn.Value);
        }

        if (eventFilter.ToCreatedOn is not null)
        {
            expression = expression.And(x =>
            x.CreatedOn <= eventFilter.ToCreatedOn.Value);
        }

        if (eventFilter.Status is not null)
        {
            expression = expression.And(x =>
            x.Status.Contains(eventFilter.Status));
        }

        if (eventFilter.OnError is not null)
        {
            expression = expression.And(x =>
                x.ErrorMessage != null == eventFilter.OnError.Value);
        }

        if (eventFilter.DataCriteria is not null)
        {
            expression = expression.And(
                RepositoryExtensions
                .Compose<EntityEventIntegration, JsonDocument, bool>(
                    x => x.Data,
                    eventFilter.DataCriteria));
        }

        return expression;
    }
}