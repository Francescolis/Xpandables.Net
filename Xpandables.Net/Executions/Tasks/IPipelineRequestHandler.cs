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
using System.ComponentModel;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Applies pipeline when handling requests.
/// </summary>
public interface IPipelineRequestHandler<TResponse>
    where TResponse : class
{
    /// <summary>
    /// Handles the specified request on a pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>The response of the request.</returns>
    Task<TResponse> HandleAsync(
        object request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Applies pipeline when handling requests with specific request type.
/// </summary>
public interface IPipelineRequestHandler<TRequest, TResponse> : IPipelineRequestHandler<TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <summary>
    /// Handles the specified request on a pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>The response of the request.</returns>
    Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<TResponse> IPipelineRequestHandler<TResponse>.HandleAsync(
        object request,
        CancellationToken cancellationToken) =>
        HandleAsync((TRequest)request, cancellationToken);
}

/// <summary>
/// Applies pipeline when handling stream requests.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineStreamRequestHandler<TResponse>
{
    /// <summary>
    /// Handles the specified request on a pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>The response of the request.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        object request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Applies pipeline when handling stream requests with specific request type.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineStreamRequestHandler<TRequest, TResponse> :
    IPipelineStreamRequestHandler<TResponse>
    where TRequest : class, IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request on a pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>The response of the request.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
    [EditorBrowsable(EditorBrowsableState.Never)]
    IAsyncEnumerable<TResponse> IPipelineStreamRequestHandler<TResponse>.HandleAsync(
        object request,
        CancellationToken cancellationToken) =>
        HandleAsync((TRequest)request, cancellationToken);
}