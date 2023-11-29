
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Xpandables.Net.Expressions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Specifications;

/// <summary>
///  This class is a helper that provides a default implementation for <see cref="ISpecification{TSource}"/>.
/// </summary>
/// <typeparam name="TSource">The type of the object to check for.</typeparam>
public abstract record class Specification<TSource> : QueryExpression<TSource>, ISpecification<TSource>
{
    /// <summary>
    /// Initializes a new instance of <see cref="Specification{TSource}"/> class.
    /// </summary>
    protected Specification() { }

    ///<inheritdoc/>
    ///<remarks>The default value is <see cref="OperationResults.Ok()"/>.</remarks>
    public OperationResult Result { get; protected set; } = OperationResults.Ok().Build();

    /// <summary>
    /// Returns a value that determines whether or not the specification 
    /// is satisfied by the source object.
    /// </summary>
    /// <remarks>To customize its behavior, you must override the 
    /// <see cref="ApplySpecification(TSource)"/> method.</remarks>
    /// <param name="source">The target source to check specification on.</param>
    /// <returns><see langword="true"/>if the specification is satisfied, 
    /// otherwise <see langword="false"/> and in that case, 
    /// the <see cref="Result"/> must contain a failure <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    public bool IsSatisfiedBy(TSource source)
    {
        ApplySpecification(source);
        return Result.IsSuccess;
    }

    /// <summary>
    /// When overridden in derived class, this method will do the 
    /// actual job of checking that the source satisfies 
    /// to the specification, if not satisfies to the specification, 
    /// set the <see cref="Result"/> property to a failure <see cref="IOperationResult"/>.
    /// The default <see cref="Result"/> is <see cref="OperationResults.Ok()"/>.
    /// </summary>
    /// <param name="source">The target source to be checked.</param>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    protected abstract void ApplySpecification(TSource source);

    /// <summary>
    /// Returns the unique hash code for the current instance.
    /// </summary>
    /// <returns><see cref="int"/> value.</returns>
    public override int GetHashCode()
    {
        var hash = GetExpression().GetHashCode();
        hash = hash * 17 + GetExpression().Parameters.Count;
        foreach (var param in GetExpression().Parameters)
        {
            hash *= 17;
            if (param != null) hash += param.GetHashCode();
        }

        return hash;
    }

    /// <summary>
    /// Returns a composite specification from the two specifications using the And operator.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification</param>
    /// <returns>A new specification.</returns>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static Specification<TSource> operator &(Specification<TSource> left, Specification<TSource> right)
#pragma warning restore CA2225 // Operator overloads have named alternates
      => new SpecificationAnd<TSource>(left, right: right);

    /// <summary>
    /// Returns a composite specification from the two specifications using the Or operator.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification</param>
    /// <returns>A new specification.</returns>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static Specification<TSource> operator |(Specification<TSource> left, Specification<TSource> right)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => new SpecificationOr<TSource>(left, right: right);

    /// <summary>
    /// Returns a new specification that is the opposite of the specified one.
    /// </summary>
    /// <param name="other">The specification to act on.</param>
    /// <returns>An opposite specification.</returns>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static SpecificationNot<TSource> operator !(Specification<TSource> other)
#pragma warning restore CA2225 // Operator overloads have named alternates
        => new(other);

    /// <summary>
    /// Returns the current specification as <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <param name="other">the target specification.</param>
    [return: NotNull]
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Func<TSource, bool>(Specification<TSource> other)
#pragma warning restore CA2225 // Operator overloads have named alternates
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return other.IsSatisfiedBy;
    }

    /// <summary>
    /// Returns the current specification as <see cref="Expression{TDelegate}"/>.
    /// </summary>
    /// <param name="other">The target specification</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Expression<Func<TSource, bool>>(Specification<TSource> other)
#pragma warning restore CA2225 // Operator overloads have named alternates
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        return other.GetExpression();
    }

    /// <summary>Returns a string that represents the current expression.</summary>
    /// <returns>A string that represents the current expression.</returns>
    public override string ToString() => GetExpression().ToString();
}
