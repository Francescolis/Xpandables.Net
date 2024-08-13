
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

namespace Xpandables.Net.Distribution;

/// <summary>
/// Represents a wrapper interface that avoids use of C# dynamics with 
/// request pattern and allows type inference 
/// for <see cref="IAsyncRequestHandler{TRequest, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">Type of the response.</typeparam>
public interface IAsyncRequestHandlerWrapper<TResponse>
{
    /// <summary>
    /// Asynchronously handles the specified request 
    /// and returns an asynchronous response type.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="OperationResultException">
    /// The operation failed.</exception>
    /// <returns>An enumerator of <typeparamref name="TResponse"/> 
    /// that can be asynchronously enumerated.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        IAsyncRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}