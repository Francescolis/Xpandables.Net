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

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Represents the next delegate to be executed on a pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public delegate TResponse RequestHandler<TResponse>()
    where TResponse : class;

/// <summary>
/// Defines a method to handle a request in a pipeline process.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The response of the request.</returns>
    TResponse Handle(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}