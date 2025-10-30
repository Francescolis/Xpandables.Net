/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

namespace Xpandables.Net.ExecutionResults.DataAnnotations;

/// <summary>
/// Defines a contract for evaluating whether an object satisfies a set of criteria.
/// </summary>
/// <remarks>Implementations of this interface encapsulate business rules or filtering logic that can be reused
/// and combined. Specifications are commonly used to separate criteria logic from data access or domain
/// models.</remarks>
/// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
public interface ISpecification<TSource>
{
    /// <summary>
    /// Gets the expression that defines the specification criteria.
    /// </summary>
    Expression<Func<TSource, bool>> Expression { get; }

    /// <summary>  
    /// Determines whether the specified source satisfies the criteria of this specification.  
    /// </summary>  
    /// <param name="source">The source object to evaluate.</param>  
    /// <returns><see langword="true"/> if the source satisfies the criteria; otherwise,
    /// <see langword="false"/>.</returns>  
    bool IsSatisfiedBy(TSource source);
}
