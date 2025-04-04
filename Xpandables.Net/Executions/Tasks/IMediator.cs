﻿/*******************************************************************************
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
namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines a mediator interface for sending requests and queries.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request asynchronously.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<ExecutionResult> SendAsync(
        IRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<ExecutionResult<TResult>> SendAsync<TResult>(
        IRequest<TResult> request,
        CancellationToken cancellationToken = default)
        where TResult : notnull;

    /// <summary>
    /// Sends a stream request asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of results.</returns>
    IAsyncEnumerable<TResult> SendAsync<TResult>(
        IStreamRequest<TResult> request,
        CancellationToken cancellationToken = default)
        where TResult : notnull;
}
