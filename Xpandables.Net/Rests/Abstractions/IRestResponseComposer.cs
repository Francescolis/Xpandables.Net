
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
using System.ComponentModel;

using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Rests.Abstractions;

/// <summary>
/// Defines the contract for a composer responsible for creating REST responses.
/// </summary>
public interface IRestResponseComposer
{
    /// <summary>
    /// Checks if the composer can handle the provided context.
    /// </summary>
    /// <param name="context"> This parameter provides the necessary context for building the response.</param>
    /// <returns>true if the composer can handle the context; otherwise, false.</returns>
    bool CanCompose(RestResponseContext context);

    /// <summary>
    /// Composes a response based on the provided context.
    /// </summary>
    /// <param name="context">This parameter provides the necessary context for building the response.</param>
    /// <param name="cancellationToken">This parameter allows the operation to be canceled if needed.</param>
    /// <returns>The method returns a task that resolves to the generated response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    ValueTask<RestResponse> ComposeAsync
        (RestResponseContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for composing a REST response from a strongly typed context.
/// </summary>
/// <typeparam name="TResult">The type of the response data to be composed. Must not be null.</typeparam>
public interface IRestResponseResultComposer<TResult> : IRestResponseComposer
    where TResult : notnull
{
    /// <summary>
    /// Asynchronously composes a REST response based on the specified response context.
    /// </summary>
    /// <param name="context">The context containing information required to generate the REST response. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A value task that represents the asynchronous operation. The result contains the composed REST response.</returns>
    ValueTask<RestResponse<TResult>> ComposeAsync(
        RestResponseContext<TResult> context,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async ValueTask<RestResponse> IRestResponseComposer.ComposeAsync(
        RestResponseContext context,
        CancellationToken cancellationToken) =>
        await ComposeAsync(RestResponseContext<TResult>.Create(context), cancellationToken).ConfigureAwait(false);
}

/// <summary>
/// Defines a contract for asynchronously composing REST responses from a streaming context.
/// </summary>
/// <typeparam name="TResult">The type of the response data to be composed. Must not be null.</typeparam>
public interface IRestResponseStreamComposer<TResult> : IRestResponseComposer
    where TResult : notnull
{
    /// <summary>
    /// Asynchronously composes a REST response based on the provided streaming context.
    /// </summary>
    /// <param name="context">The context containing the response data and metadata to be used for composing the REST response. Cannot be
    /// null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A value task that represents the asynchronous operation. The result contains the composed REST response.</returns>
    ValueTask<RestResponse<IAsyncPagedEnumerable<TResult>>> ComposeAsync(
        RestResponseStreamContext<TResult> context,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async ValueTask<RestResponse> IRestResponseComposer.ComposeAsync(
        RestResponseContext context,
        CancellationToken cancellationToken) =>
        await ComposeAsync(RestResponseStreamContext<TResult>.Create(context), cancellationToken).ConfigureAwait(false);
}