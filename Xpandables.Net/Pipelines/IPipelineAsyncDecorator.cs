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

using Xpandables.Net.Commands;

namespace Xpandables.Net.Pipelines;

/// <summary>
/// Represents a delegate that handles asynchronous request and returns an 
/// asynchronous enumerable response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public delegate IAsyncEnumerable<TResponse> RequestAsyncHandler<TResponse>();

/// <summary>
/// Defines a decorator for handling asynchronous queries.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineAsyncDecorator<TRequest, TResponse>
    where TRequest : class, IQueryAsync<TResponse>
{
    /// <summary>
    /// Handles the asynchronous request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next delegate in the chain.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of the response.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        RequestAsyncHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}
