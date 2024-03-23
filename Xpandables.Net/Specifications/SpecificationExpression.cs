
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

namespace Xpandables.Net.Specifications;

internal sealed record class SpecificationExpression<TSource>
    : Specification<TSource>
{
    private readonly Expression<Func<TSource, bool>> _expression;

    public SpecificationExpression(Expression<Func<TSource, bool>> expression)
        => _expression = expression
        ?? throw new ArgumentNullException(nameof(expression));

    public override Expression<Func<TSource, bool>> GetExpression()
        => _expression;
}
