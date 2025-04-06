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
namespace Xpandables.Net.Http;

/// <summary>
/// Defines a contract for RESTful client operations. It serves as a blueprint for implementing REST client
/// functionalities.
/// </summary>
public interface IRestClient
{
    /// <summary>
    /// Provides an instance of HttpClient for making HTTP requests. 
    /// It is typically used for sending and receiving data over the network.
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <param name="request">The request to be sent.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, with a response as the result.</returns>
    /// <remarks>Make use of <see langword="using"/> keyword when calling this method.
    /// The request must be decorated with one of the <see cref="MapRestAttribute"/> or implement the <see cref="IMapRestBuilder"/> interface.</remarks>
    Task<RestResponse> SendAsync(IRestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an asynchronous HTTP request and returns the response.
    /// </summary>
    /// <typeparam name="TResult">This type parameter represents the expected type of the response content.</typeparam>
    /// <param name="request">This parameter contains the details of the HTTP request to be sent.</param>
    /// <param name="cancellationToken">This parameter allows the operation to be canceled if needed.</param>
    /// <returns>The method returns a task that resolves to the HTTP response containing the specified type of content.</returns>
    /// <remarks>Make use of <see langword="using"/> keyword when calling this method.
    /// The request must be decorated with one of the <see cref="MapRestAttribute"/> or implement the <see cref="IMapRestBuilder"/> interface.</remarks>
    Task<RestResponse<TResult>> SendAsync<TResult>(
        IRestRequest<TResult> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an asynchronous request and returns a response containing a stream of results.
    /// </summary>
    /// <typeparam name="TResult">Represents the type of items that will be streamed in the response.</typeparam>
    /// <param name="request">Contains the details of the request to be sent asynchronously.</param>
    /// <param name="cancellationToken">Allows the operation to be canceled if needed.</param>
    /// <returns>Provides a task that resolves to a response with a stream of results.</returns>
    /// <remarks>Make use of <see langword="using"/> keyword when calling this method.
    /// The request must be decorated with one of the <see cref="MapRestAttribute"/> or implement the <see cref="IMapRestBuilder"/> interface.</remarks>
    Task<RestResponse<IAsyncEnumerable<TResult>>> SendAsync<TResult>(
        IRestStreamRequest<TResult> request,
        CancellationToken cancellationToken = default);
}
