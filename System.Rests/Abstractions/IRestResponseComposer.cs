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
namespace System.Rests.Abstractions;

/// <summary>
/// Defines a contract for composing REST responses based on a given response context.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms to determine whether a response can be composed
/// from the current context and to asynchronously generate the response. This abstraction enables flexible and testable
/// response composition strategies in RESTful applications.</remarks>
public interface IRestResponseComposer
{
    /// <summary>
    /// Determines whether the specified response context can be composed into a valid response.
    /// </summary>
    /// <remarks>This method evaluates the state of the response context to ascertain its composability, which
    /// may depend on various factors such as the presence of required data or the current state of the
    /// request.</remarks>
    /// <param name="context">The context of the REST response, which contains information about the current request and response state.</param>
    /// <returns>true if the context can be composed into a valid response; otherwise, false.</returns>
    bool CanCompose(RestResponseContext context);

    /// <summary>
    /// Asynchronously composes a REST response based on the specified context.
    /// </summary>
    /// <remarks>The composition process may be affected by the state of the provided context. If the
    /// operation is canceled via the cancellation token, the returned task will be canceled.</remarks>
    /// <param name="context">The context that provides the information required to compose the REST response. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the composed REST response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the context cannot be composed into a valid response.</exception>
    ValueTask<RestResponse> ComposeAsync(RestResponseContext context, CancellationToken cancellationToken = default);
}