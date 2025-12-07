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
    /// Asynchronously builds a REST request for the specified request object.
    /// </summary>
    /// <typeparam name="TRestRequest">The type of the request object for which to build the REST request context. 
    /// Must implement <see cref="IRestRequest"/>.</typeparam>
    /// <param name="request">The request object for which to build the REST request context. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A value task that represents the asynchronous operation. The result contains a <see
    /// cref="RestRequestContext"/> representing the context for the specified request.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    ValueTask<RestRequest> BuildRequestAsync<TRestRequest>(TRestRequest request, CancellationToken cancellationToken = default)
        where TRestRequest : class, IRestRequest;
}