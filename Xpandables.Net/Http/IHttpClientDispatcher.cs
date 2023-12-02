﻿
/************************************************************************************************************
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
************************************************************************************************************/
using System.Text.Json;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with methods to handle <see cref="IHttpClientRequest"/> 
/// or <see cref="IHttpClientRequest{TResponse}"/> requests using a typed client HTTP Client.
/// The request should implement one of the following interfaces :
/// <see cref="IHttpRequestString"/>, <see cref="IHttpRequestStream"/>, <see cref="IHttpRequestByteArray"/>, 
/// <see cref="IHttpRequestFormUrlEncoded"/>,
/// <see cref="IHttpRequestMultipart"/>, <see cref="IHttpRequestQueryString"/>, <see cref="IHttpRequestCookie"/>,
/// <see cref="IHttpRequestHeader"/>, <see cref="IHttpRequestPatch"/>
/// or <see cref="IHttpRequestPathString"/>, and must be decorated with <see cref="HttpClientAttribute"/> 
/// or implement <see cref="IHttpClientAttributeBuilder"/>.
/// </summary>
public interface IHttpClientDispatcher : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Contains the <see cref="System.Net.Http.HttpClient"/> instance for the current dispatcher.
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the current <see cref="JsonSerializerOptions"/> to be used for serialization.
    /// </summary>
    /// <remarks>The instance can be provided by registering the <see cref="JsonSerializerOptions"/> that will be automatically resolved by the system.</remarks>
    JsonSerializerOptions? SerializerOptions { get; }

    /// <summary>
    /// Sends the request that returns a collection that can be async-enumerated.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to act with. The request must be decorated with 
    /// the <see cref="HttpClientAttribute"/> or implements the <see cref="IHttpClientAttributeBuilder"/> interface.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    ValueTask<HttpClientResponse<IAsyncEnumerable<TResult>>> SendAsync<TResult>(
        IHttpClientAsyncRequest<TResult> request,
        JsonSerializerOptions? serializerOptions = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the request that does not return a response.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <param name="request">The request to act with. The request must be decorated with 
    /// the <see cref="HttpClientAttribute"/> or implements the <see cref="IHttpClientAttributeBuilder"/> interface.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    ValueTask<HttpClientResponse> SendAsync(
        IHttpClientRequest request,
        JsonSerializerOptions? serializerOptions = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the request that returns a response of <typeparamref name="TResult"/> type.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to act with. The request must be decorated with
    /// the <see cref="HttpClientAttribute"/> or implements the <see cref="IHttpClientAttributeBuilder"/> interface.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    ValueTask<HttpClientResponse<TResult>> SendAsync<TResult>(
        IHttpClientRequest<TResult> request,
        JsonSerializerOptions? serializerOptions = default,
        CancellationToken cancellationToken = default);
}
