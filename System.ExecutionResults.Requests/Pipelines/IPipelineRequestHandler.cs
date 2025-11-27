/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

namespace System.ExecutionResults.Pipelines;

/// <summary>
/// Defines the contract for a pipeline request handler that processes a request
/// of a specified type and returns an execution result.
/// </summary>
public interface IPipelineRequestHandler<in TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Processes the given request asynchronously and returns an execution result.
    /// </summary>
    /// <param name="request">The request instance containing required information for processing.</param>
    /// <param name="cancellationToken">A token that enables the operation to be cancelled, if requested.</param>
    /// <returns>A task representing the asynchronous execution, providing an <see cref="OperationResult"/> upon completion.</returns>
    Task<OperationResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}