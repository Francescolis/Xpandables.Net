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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Rests;

/// <summary>
/// Defines a contract for building REST response objects from HTTP response messages and REST request data.
/// </summary>
public interface IRestResponseBuilder
{
    /// <summary>
    /// Asynchronously builds a response based on the provided HTTP response message.
    /// </summary>
    /// <param name="context"> The REST response context used to create the response.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the asynchronous operation.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the constructed response.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    [RequiresUnreferencedCode("May use unreferenced code to build RestResponse.")]
    [RequiresDynamicCode("May use dynamic code to convert build RestResponse.")]
    ValueTask<RestResponse> BuildResponseAsync(
        RestResponseContext context,
        CancellationToken cancellationToken = default);
}