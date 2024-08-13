
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

namespace Xpandables.Net.Expressions;

/// <summary>
/// Defines a methods that returns an <see cref="Expression{TDelegate}"/> 
/// that can be used to request. This can significantly improve the use of the 
/// request/request pattern, allowing class conversion to an expression tree.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
/// <typeparam name="TResult">The type of the result of expression.</typeparam>
public interface IQueryExpression<TSource, TResult>
{
    /// <summary>
    /// Gets the expression tree for the underlying instance.
    /// </summary>
    Expression<Func<TSource, TResult>> GetExpression();
}

/// <summary>
/// Defines a methods that returns an <see cref="Expression{TDelegate}"/> 
/// that can be used to request the <typeparamref name="TSource"/> instance 
/// with <see cref="bool"/> result. This can significantly improve the use of 
/// the request/request pattern, allowing class conversion to an expression tree.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
public interface IQueryExpression<TSource> : IQueryExpression<TSource, bool> { }
