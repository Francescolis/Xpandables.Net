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