
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
using System.Linq.Expressions;

using Xpandables.Net.Aggregates.DomainEvents;

namespace Xpandables.Net.Aggregates;

internal sealed class EventFilterEntityVisitor : ExpressionVisitor
{
    internal static readonly ParameterExpression EventEntityParameter = Expression.Parameter(typeof(EntityDomainEvent));
    internal static readonly EventFilterEntityVisitor EventEntityVisitor = new(typeof(EntityDomainEvent), nameof(EntityDomainEvent.Data));

    internal readonly ParameterExpression Parameter;
    private readonly Expression _expression;

    internal EventFilterEntityVisitor(Type parameterType, string member)
    {
        Parameter = Expression.Parameter(parameterType);
        _expression = Expression.PropertyOrField(Parameter, member);
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node.Type == _expression.Type ? _expression : base.VisitParameter(node);
}