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

using Xpandables.Net.Executions;

namespace Xpandables.Net.Tasks.Pipelines;

/// <summary>
/// Represents the next delegate to be executed on a pipeline.
/// </summary>
/// <param name="cancellationToken">Optional cancellation token to observe for cancellation requests.</param>
public delegate Task<ExecutionResult> RequestHandler(CancellationToken cancellationToken = default);

/// <summary>
/// Defines a contract for decorating a pipeline handler, allowing pre- or post-processing
/// around the execution of a request within the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled by the pipeline, which must be a class.</typeparam>
public interface IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles the pipeline request and invokes the next handler in the pipeline.
    /// </summary>
    /// <param name="context">The request context to process with.</param>
    /// <param name="next">The next handler in the pipeline to be executed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken);
}