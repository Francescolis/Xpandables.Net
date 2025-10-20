
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

using Xpandables.Net.Async;
using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.Cqrs;

/// <summary>
/// Defines a handler for processing stream requests asynchronously, producing a stream of responses.
/// </summary>
/// <remarks>This interface extends <see cref="IRequestHandler{TRequest}"/> to support handling requests that
/// result in a stream of responses. Implementations should ensure that the stream is properly disposed of and that any
/// necessary cleanup is performed.</remarks>
/// <typeparam name="TRequest">The type of the request message.</typeparam>
/// <typeparam name="TResponse">The type of the response message.</typeparam>
public interface IStreamRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest>
    where TRequest : class, IStreamRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles the specified request and returns a stream of responses.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a stream of responses.</returns>
    new Task<ExecutionResult<IAsyncPagedEnumerable<TResponse>>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<ExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(request, cancellationToken).ConfigureAwait(false);
}
