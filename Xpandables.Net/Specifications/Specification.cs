
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Xpandables.Net.Expressions;

namespace Xpandables.Net.Specifications;

/// <summary>
///  This class is a helper that provides a default 
///  implementation for <see cref="ISpecification{TSource}"/>.
/// </summary>
/// <typeparam name="TSource">The type of the object to check for.</typeparam>
public abstract record class Specification<TSource>
    : QueryExpression<TSource>, ISpecification<TSource>
{
    /// <summary>
    /// Initializes a new instance of <see cref="Specification{TSource}"/> class.
    /// </summary>
    protected Specification() { }

    /// <summary>
    /// Returns a value that determines whether or not the specification 
    /// is satisfied by the source object.
    /// </summary>
    /// <param name="source">The target source to check specification on.</param>
    /// <returns><see langword="true"/>if the specification is satisfied, 
    /// otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    public virtual bool IsSatisfiedBy(TSource source)
        => GetExpression().Compile().Invoke(source);

    /// <summary>
    /// Returns a composite specification from the 
    /// two specifications using the And operator.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification</param>
    /// <returns>A new specification.</returns>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static Specification<TSource> operator &(
        Specification<TSource> left,
        Specification<TSource> right)
#pragma warning restore CA2225 // Operator overloads have named alternates
      => new SpecificationAnd<TSource>(left, right: right);

    /// <summary>
    /// Returns a composite specification from the two 
    /// specifications using the Or operator.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification</param>
    /// <returns>A new specification.</returns>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static Specification<TSource> operator |(
        Specification<TSource> left,
        Specification<TSource> right)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => new SpecificationOr<TSource>(left, right: right);

    /// <summary>
    /// Returns a new specification that is the opposite of the specified one.
    /// </summary>
    /// <param name="other">The specification to act on.</param>
    /// <returns>An opposite specification.</returns>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static SpecificationNot<TSource> operator !(
        Specification<TSource> other)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => new(other);

    /// <summary>
    /// Returns the current specification as <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <param name="other">the target specification.</param>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Func<TSource, bool>(
        Specification<TSource> other)
#pragma warning restore CA2225 // Operator overloads have named alternates
    {
        ArgumentNullException.ThrowIfNull(other);
        return other.GetExpression().Compile();
    }

    /// <summary>
    /// Returns the current specification as <see cref="Expression{TDelegate}"/>.
    /// </summary>
    /// <param name="other">The target specification</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Expression<Func<TSource, bool>>(
        Specification<TSource> other)
#pragma warning restore CA2225 // Operator overloads have named alternates
    {
        ArgumentNullException.ThrowIfNull(other);
        return other.GetExpression();
    }

    /// <summary>Returns a string that represents the current expression.</summary>
    /// <returns>A string that represents the current expression.</returns>
    public override string ToString() => GetExpression().ToString();
}
