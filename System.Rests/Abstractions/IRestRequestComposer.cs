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
/// Defines a contract to compose <see cref="HttpRequestMessage"/> from the request context.
/// </summary>
/// <remarks>The type of the interface
/// implemented by the request source : <see cref="IRestBasicAuthentication"/>,
/// <see cref="IRestByteArray"/>, <see cref="IRestCookie"/>,
/// <see cref="IRestFormUrlEncoded"/>, <see cref="IRestHeader"/>,
/// <see cref="IRestMultipart"/>, <see cref="IRestPatch"/>,
/// <see cref="IRestPathString"/>, <see cref="IRestQueryString"/>,
/// <see cref="IRestStream"/> and <see cref="IRestString"/>.</remarks>
public interface IRestRequestComposer
{
    /// <summary>
    /// Determines whether the specified REST request context can be composed into a valid operation.
    /// </summary>
    /// <remarks>Use this method to verify that the provided context meets all requirements for composition
    /// before attempting to build or execute a REST operation. The specific conditions for composability depend on the
    /// state and contents of the context.</remarks>
    /// <param name="context">The context of the REST request, containing information about the current operation and its parameters. Cannot
    /// be null.</param>
    /// <returns>true if the request context can be composed; otherwise, false.</returns>
    bool CanCompose(RestRequestContext context);

    /// <summary>
    /// Composes the <see cref="RestResponseContext"/> using the request context asynchronously.
    /// </summary>
    /// <param name="context">This parameter provides the necessary context for building the http request.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the context cannot be composed into a valid response.</exception>
    ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default);
}
