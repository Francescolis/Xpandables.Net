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
/// Defines a contract for building REST response objects from HTTP response messages and REST request data.
/// </summary>
public interface IRestResponseBuilder
{
    /// <summary>
    /// Asynchronously constructs and sends a response using the specified context information.
    /// </summary>
    /// <remarks>This method may throw exceptions if the provided context is invalid or if the operation is
    /// canceled via the cancellation token.</remarks>
    /// <param name="context">The context that provides the data and settings required to build the response. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A value task that represents the asynchronous operation of building and sending the response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the context cannot be composed into a valid response.</exception>
    ValueTask<RestResponse> BuildResponseAsync(RestResponseContext context, CancellationToken cancellationToken = default);
}