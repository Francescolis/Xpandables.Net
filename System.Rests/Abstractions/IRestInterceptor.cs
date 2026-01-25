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
/// Defines a contract for intercepting REST requests before they are sent.
/// </summary>
/// <remarks>
/// Implement this interface to modify or inspect requests before they are sent to the server.
/// Common use cases include adding headers, logging, modifying the request body, or short-circuiting requests.
/// Multiple interceptors can be registered and will be executed in order of registration.
/// </remarks>
public interface IRestRequestInterceptor
{
    /// <summary>
    /// Gets the order in which this interceptor should be executed.
    /// Lower values execute first. Default is 0.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Intercepts a REST request before it is sent.
    /// </summary>
    /// <param name="context">The request context containing the HTTP request message and metadata.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    /// A <see cref="ValueTask{RestResponse}"/> that may return:
    /// - <c>null</c> to continue with the request pipeline
    /// - A <see cref="RestResponse"/> to short-circuit and skip the actual HTTP request
    /// </returns>
    ValueTask<RestResponse?> InterceptAsync(RestRequestContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for intercepting REST responses after they are received.
/// </summary>
/// <remarks>
/// Implement this interface to modify or inspect responses after they are received from the server.
/// Common use cases include logging, modifying the response, caching, or error handling.
/// Multiple interceptors can be registered and will be executed in reverse order of registration
/// (last registered executes first on the response path).
/// </remarks>
public interface IRestResponseInterceptor
{
    /// <summary>
    /// Gets the order in which this interceptor should be executed.
    /// Lower values execute first. Default is 0.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Intercepts a REST response after it is received.
    /// </summary>
    /// <param name="response">The response received from the server or from a previous interceptor.</param>
    /// <param name="request">The original request that was sent.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    /// A <see cref="ValueTask{RestResponse}"/> containing the potentially modified response.
    /// </returns>
    ValueTask<RestResponse> InterceptAsync(
        RestResponse response,
        IRestRequest request,
        CancellationToken cancellationToken = default);
}
