﻿/************************************************************************************************************
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

namespace Xpandables.Net.Operations.Messaging;

/// <summary>
/// This interface is used as a marker for queries when using the synchronous 
/// query pattern that contains a specific-type result.
/// Class implementation is used with the <see cref="IQueryHandler{TQuery, TResult}"/> where
/// "TQuery" is a class that implements the <see cref="IQuery{TResult}"/> interface. 
/// This can also be enhanced with some useful decorators.
/// </summary>
/// <typeparam name="TResult">Type of the result of the query.</typeparam>
public interface IQuery<out TResult> : IMessaging { }

/// <summary>
/// Represents a method signature to be used to apply <see cref="IQueryHandler{TQuery, TResult}"/> implementation.
/// </summary>
/// <typeparam name="TQuery">Type of the query that will be used as argument.</typeparam>
/// <typeparam name="TResult">Type of the result of the query.</typeparam>
/// <param name="query">The query to act on.</param>
/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
/// <returns>A value that represents an <see cref="OperationResult{TValue}"/>.</returns>
/// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
/// <exception cref="OperationResultException">The operation failed.</exception>
/// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
public delegate ValueTask<OperationResult<TResult>> QueryHandler<in TQuery, TResult>(
    TQuery query, CancellationToken cancellationToken = default)
    where TQuery : notnull, IQuery<TResult>;

/// <summary>
/// Defines a generic method that a class implements to handle a type-specific query 
/// and returns a type-specific result.
/// The implementation must be thread-safe when working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TQuery">Type of the query that will be used as argument.</typeparam>
/// <typeparam name="TResult">Type of the result of the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : notnull, IQuery<TResult>
{
    /// <summary>
    /// Asynchronously handles the specified query and returns the task result.
    /// </summary>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an <see cref="OperationResult{TValue}"/>.</returns>
    ValueTask<OperationResult<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a wrapper interface that avoids use of C# dynamics 
/// with query pattern and allows type inference for <see cref="IQueryHandler{TQuery, TResult}"/>.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface IQueryHandlerWrapper<TResult>
{
    /// <summary>
    /// Asynchronously handles the specified query and returns the task result.
    /// </summary>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object of <see cref="IOperationResult{TValue}"/>.</returns>
    ValueTask<OperationResult<TResult>> HandleAsync(
        IQuery<TResult> query, CancellationToken cancellationToken = default);
}

internal sealed class QueryHandlerWrapper<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee) : IQueryHandlerWrapper<TResult>
    where TQuery : notnull, IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _decoratee =
        decoratee ?? throw new ArgumentNullException($"{decoratee} : {nameof(TQuery)}.{nameof(TResult)}");

    public async ValueTask<OperationResult<TResult>> HandleAsync(
        IQuery<TResult> query, CancellationToken cancellationToken = default)
        => await _decoratee.HandleAsync((TQuery)query, cancellationToken).ConfigureAwait(false);
}