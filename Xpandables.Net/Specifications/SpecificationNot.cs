
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

using Xpandables.Net.Expressions;

namespace Xpandables.Net.Specifications;

/// <summary>
/// Provides the <see cref="Specification{TSource}"/> "Not" profile.
/// </summary>
/// <typeparam name="TSource">The type of the object to check for.</typeparam>
public sealed record class SpecificationNot<TSource> : Specification<TSource>
{
    private readonly ISpecification<TSource> _other;

    /// <summary>
    /// Returns a new instance of <see cref="SpecificationNot{TSource}"/> 
    /// class with the specification for Not.
    /// </summary>
    /// <param name="other">The specification to convert to Not.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="other"/> 
    /// is null.</exception>exception>
    public SpecificationNot(ISpecification<TSource> other)
        => _other = other ?? throw new ArgumentNullException(nameof(other));

    /// <summary>
    /// Returns the expression to be used for 
    /// the clause <see langword="Where"/> in a query.
    /// </summary>
    public override Expression<Func<TSource, bool>> GetExpression()
        => QueryExpressionFactory.Not(_other.GetExpression());
}
