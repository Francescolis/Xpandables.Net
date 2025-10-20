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

namespace Xpandables.Net.Rests.Abstractions;

/// <summary>
/// Gets or sets the underlying HTTP request message to be sent with the REST request.
/// </summary>
/// <remarks>Use this property to configure the HTTP method, headers, URI, and content for the REST request.
/// Changes to this message will affect how the request is sent and processed by the server.</remarks>
public sealed class RestRequest : Disposable
{
    /// <summary>
    /// Gets the HTTP request message associated with this operation.
    /// </summary>
    /// <remarks>Use this property to access or inspect the underlying HTTP request, including its headers,
    /// method, URI, and content. The property is required and must be initialized before use.</remarks>
    public required HttpRequestMessage HttpRequestMessage { get; init; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            HttpRequestMessage.Dispose();
        }

        IsDisposed = true;
        base.Dispose(disposing);
    }
}
