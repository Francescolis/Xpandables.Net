
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Commands;

/// <summary>
/// This interface is used as a marker for queries when using the synchronous 
/// query pattern that contains a specific-type result.
/// Class implementation is used with the 
/// <see cref="IQueryHandler{TQuery, TResult}"/> where
/// "TQuery" is a class that implements the 
/// <see cref="IQuery{TResult}"/> interface. 
/// This can also be enhanced with some useful decorators.
/// </summary>
/// <typeparam name="TResult">Type of the result of the query.</typeparam>
public interface IQuery<out TResult>
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public Guid Id => Guid.NewGuid();

    /// <summary>
    /// Gets When the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the name of the user running associated with the current event.
    /// The default value is associated with the current thread.
    /// </summary>
    public string CreatedBy => Environment.UserName;
}

/// <summary>
/// Represents a method signature to be used to 
/// apply <see cref="IQueryHandler{TQuery, TResult}"/> implementation.
/// </summary>
/// <typeparam name="TQuery">Type of the query that will 
/// be used as argument.</typeparam>
/// <typeparam name="TResult">Type of the result of the query.</typeparam>
/// <param name="query">The query to act on.</param>
/// <param name="cancellationToken">A CancellationToken 
/// to observe while waiting for the task to complete.</param>
/// <returns>A value that represents 
/// an <see cref="IOperationResult{TValue}"/>.</returns>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="query"/> is null.</exception>
public delegate ValueTask<IOperationResult<TResult>> QueryHandler<in TQuery, TResult>(
    TQuery query, CancellationToken cancellationToken = default)
    where TQuery : notnull, IQuery<TResult>;

/// <summary>
/// Defines a generic method that a class implements 
/// to handle a type-specific query 
/// and returns a type-specific result.
/// The implementation must be thread-safe when working 
/// in a multi-threaded environment.
/// </summary>
/// <typeparam name="TQuery">Type of the query that 
/// will be used as argument.</typeparam>
/// <typeparam name="TResult">Type of the result of the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : notnull, IQuery<TResult>
{
    /// <summary>
    /// Asynchronously handles the specified query and returns the task result.
    /// </summary>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="query"/> is null.</exception>
    /// <returns>A task that represents a
    /// n <see cref="IOperationResult{TValue}"/>.</returns>
    ValueTask<IOperationResult<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a wrapper interface that avoids use of C# dynamics 
/// with query pattern and allows 
/// type inference for <see cref="IQueryHandler{TQuery, TResult}"/>.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface IQueryHandlerWrapper<TResult>
{
    /// <summary>
    /// Asynchronously handles the specified query and returns the task result.
    /// </summary>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="query"/> is null.</exception>
    /// <returns>A task that represents an 
    /// object of <see cref="IOperationResult{TValue}"/>.</returns>
    ValueTask<IOperationResult<TResult>> HandleAsync(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default);
}