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

namespace Xpandables.Net.Rests;

/// <summary>
/// Defines a contract for RESTful client operations. It serves as a blueprint for implementing REST client
/// functionalities.
/// </summary>
public interface IRestClient : IDisposable
{
    /// <summary>
    /// Provides an instance of HttpClient for making HTTP requests.
    /// It is typically used for sending and receiving data over the network.
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Sends a REST request asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TRestRequest">This type parameter represents the type of the request to be sent.</typeparam>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, with a response as the result.</returns>
    /// <remarks>
    /// Make use of <see langword="using" /> keyword when calling this method.
    /// The request must be decorated with one of the <see cref="RestAttribute" /> or implement the
    /// <see cref="IRestAttributeBuilder" /> interface.
    /// </remarks>
    Task<RestResponse> SendAsync<TRestRequest>(TRestRequest request, CancellationToken cancellationToken = default)
        where TRestRequest : class, IRestRequest;
}