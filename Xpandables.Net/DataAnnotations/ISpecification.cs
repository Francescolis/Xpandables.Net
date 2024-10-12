
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
using Xpandables.Net.Expressions;

namespace Xpandables.Net.DataAnnotations;
/// <summary>  
/// Defines a specification that determines whether a given source satisfies 
/// certain criteria.  
/// </summary>  
/// <typeparam name="TSource">The type of the source object.</typeparam>  
public interface ISpecification<TSource> : IQueryExpression<TSource, bool>
{
    /// <summary>  
    /// Determines whether the specified source satisfies the criteria of 
    /// this specification.  
    /// </summary>  
    /// <param name="source">The source object to evaluate.</param>  
    /// <returns><see langword="true"/> if the source satisfies the criteria; otherwise,
    /// <see langword="false"/>.</returns>  
    bool IsSatisfiedBy(TSource source);
}
