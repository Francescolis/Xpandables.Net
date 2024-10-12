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
using System.Linq.Expressions;

namespace Xpandables.Net.Expressions;

/// <summary>
/// Represents a query expression that can be used to filter or project data 
/// from a source.
/// </summary>
/// <typeparam name="TSource">The type of the source data.</typeparam>
/// <typeparam name="TResult">The type of the result data.</typeparam>
public interface IQueryExpression<TSource, TResult>
{
    /// <summary>
    /// Gets the expression that defines the query.
    /// </summary>
    Expression<Func<TSource, TResult>> Expression { get; }
}

/// <summary>
/// Represents a query expression that can be used to filter data from a source.
/// </summary>
/// <typeparam name="TSource">The type of the source data.</typeparam>
public interface IQueryExpression<TSource> : IQueryExpression<TSource, bool>
{
}