
/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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

using Xpandables.Net.Operations.Expressions;

namespace Xpandables.Net.Operations.Specifications;

/// <summary>
/// Provides the <see cref="Specification{TSource}"/> "And" profile.
/// </summary>
/// <typeparam name="TSource">The type of the object to check for.</typeparam>
public sealed record class SpecificationAnd<TSource> : Specification<TSource>
{
    private readonly ISpecification<TSource> _left;
    private readonly ISpecification<TSource> _right;

    /// <summary>
    /// Returns a new instance of <see cref="SpecificationAnd{TSource}"/> class with the specifications for composition.
    /// </summary>
    /// <param name="left">The specification for the left side.</param>
    /// <param name="right">The specification for the right side.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public SpecificationAnd(ISpecification<TSource> left, ISpecification<TSource> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    ///<inheritdoc/>
    protected sealed override void ApplySpecification(TSource source)
    {
        if (!_left.IsSatisfiedBy(source) && _right.IsSatisfiedBy(source))
            Result = _left.Result;
        else if (_left.IsSatisfiedBy(source) && !_right.IsSatisfiedBy(source))
        {
            Result = _right.Result;
        }
        else if (!_left.IsSatisfiedBy(source) && !_right.IsSatisfiedBy(source))
        {
            _left.Result.Errors.Merge(_right.Result.Errors);
            Result = _left.Result;
        }
    }

    /// <summary>
    /// Returns the expression to be used for the clause <see langword="Where"/> in a query.
    /// </summary>
    public override Expression<Func<TSource, bool>> GetExpression()
        => QueryExpressionFactory<bool>.And(_left.GetExpression(), _right.GetExpression());
}
