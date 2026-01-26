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
/// Defines a contract for building REST request contexts from request objects in an asynchronous manner.
/// </summary>
public interface IRestRequestBuilder
{
    /// <summary>
    /// Asynchronously constructs a REST request based on the specified context information.
    /// </summary>
    /// <remarks>This method does not send the request; it only prepares it for further processing. Handle
    /// cancellation appropriately by monitoring the provided <paramref name="cancellationToken"/>.</remarks>
    /// <param name="context">The context that provides details required to build the REST request, such as the target endpoint and request
    /// parameters.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="RestRequest"/>
    /// representing the constructed REST request.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the context cannot be composed into a valid response.</exception>
    ValueTask<RestRequest> BuildRequestAsync(RestRequestContext context, CancellationToken cancellationToken = default);
}