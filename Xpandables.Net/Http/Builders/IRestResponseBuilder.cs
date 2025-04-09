﻿/*******************************************************************************
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
namespace Xpandables.Net.Http.Builders;

/// <summary>
/// Defines method to build <see cref="RestResponse"/> and <see cref="RestResponse{TResult}"/>.
/// </summary>
public interface IRestResponseBuilder
{
    /// <summary>
    /// Asynchronously builds a response based on the provided context.
    /// </summary>
    /// <param name="context">This parameter provides the necessary context for building the response.</param>
    /// <param name="cancellationToken">This parameter allows the operation to be canceled if needed.</param>
    /// <returns>The method returns a task that resolves to the generated response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
    Task<RestResponse> BuildAsync(RestResponseContext context, CancellationToken cancellationToken = default);
}