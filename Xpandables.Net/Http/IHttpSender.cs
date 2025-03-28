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
/// Provides with methods to handle <see cref="IHttpRequest"/> 
/// or <see cref="IHttpRequest{TResponse}"/> and 
/// <see cref="IHttpStreamRequest{TResponse}"/> requests using a typed 
/// client HTTP Client.
/// The request should implement one of the following interfaces :
/// <see cref="IHttpRequestContentString"/>, <see cref="IHttpRequestContentStream"/>, 
/// <see cref="IHttpRequestContentByteArray"/>, 
/// <see cref="IHttpRequestContentFormUrlEncoded"/>,
/// <see cref="IHttpRequestContentMultipart"/>, <see cref="IHttpRequestContentQueryString"/>, 
/// <see cref="IHttpRequestContentCookie"/>,
/// <see cref="IHttpRequestContentHeader"/>, <see cref="IHttpRequestContentPatch"/>
/// or <see cref="IHttpRequestContentPathString"/>, and must be decorated 
/// with <see cref="MapHttpAttribute"/> 
/// or implement <see cref="IMapHttpBuilder"/>.
/// </summary>
public interface IHttpSender
{
    /// <summary>
    /// Contains the <see cref="System.Net.Http.HttpClient"/> 
    /// instance for the current mediator.
    /// </summary>
    HttpClient HttpClient { get; }

    /// <summary>
    /// Sends a request that does not return a response.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <param name="request">The request to act with. The request 
    /// must be decorated with 
    /// the <see cref="MapHttpAttribute"/> or implements the 
    /// <see cref="IMapHttpBuilder"/> interface.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="HttpResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The operation failed. See inner exception.</exception>
    Task<HttpResponse> SendAsync(
        IHttpRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request that returns a response of 
    /// <typeparamref name="TResult"/> type.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to act with. 
    /// The request must be decorated with
    /// the <see cref="MapHttpAttribute"/> or implements 
    /// the <see cref="IMapHttpBuilder"/> interface.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="ResponseHttp{TResult}"/>
    /// .</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    Task<ResponseHttp<TResult>> SendAsync<TResult>(
        IHttpRequest<TResult> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request that returns a stream and                                                                                                                                                                                                                                                                                                                                                                                                     that can be async-enumerated.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to act with. The request
    /// must be decorated with 
    /// the <see cref="MapHttpAttribute"/> or implements the 
    /// <see cref="IMapHttpBuilder"/> interface.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="ResponseHttp{TResult}"/>
    /// .</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The operation failed. See inner exception.</exception>
    Task<ResponseHttp<IAsyncEnumerable<TResult>>> SendAsync<TResult>(
        IHttpStreamRequest<TResult> request,
        CancellationToken cancellationToken = default);
}