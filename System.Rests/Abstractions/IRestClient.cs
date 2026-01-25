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
/// Defines a contract for RESTful client operations. It serves as a blueprint for implementing REST client
/// functionalities.
/// </summary>
/// <remarks>
/// The <see cref="HttpClient"/> is typically managed by the dependency injection container via 
/// <see cref="IHttpClientFactory"/> and should not be disposed by the consumer.
/// </remarks>
public interface IRestClient
{
    /// <summary>
    /// Provides an instance of HttpClient for making HTTP requests.
    /// It is typically used for sending and receiving data over the network.
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Sends a REST request asynchronously and returns the response.
    /// <para>The request must implement one or more of the following interfaces :
    /// <see cref="IRestBasicAuthentication"/>, <see cref="IRestByteArray"/>, <see cref="IRestCookie"/>, <see cref="IRestFormUrlEncoded"/>,
    /// <see cref="IRestHeader"/>, <see cref="IRestMultipart"/>, <see cref="IRestPatch"/>, <see cref="IRestPathString"/>, <see cref="IRestQueryString"/>,
    /// <see cref="IRestString"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TRestRequest">This type parameter represents the type of the request to be sent.</typeparam>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, with a response as the result.</returns>
    /// <remarks>
    /// Make use of <see langword="using" /> keyword when calling this method.
    /// The request must be decorated with one of the <see cref="RestAttribute" /> derived classes or implement the
    /// <see cref="IRestAttributeBuilder" /> interface.
    /// </remarks>
    Task<RestResponse> SendAsync<TRestRequest>(TRestRequest request, CancellationToken cancellationToken = default)
        where TRestRequest : class, IRestRequest;
}