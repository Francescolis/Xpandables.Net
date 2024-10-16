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
/// Defines a builder interface for creating 
/// <see cref="HttpClientRequestOptionsAttribute"/>.
/// </summary>
/// <remarks>This interface take priority over the 
/// <see cref="HttpClientRequestOptionsAttribute"/>.</remarks>
public interface IHttpClientRequestOptionsBuilder
{
    /// <summary>
    /// Builds the <see cref="HttpClientRequestOptionsAttribute"/>.
    /// </summary>
    /// <param name="options">The <see cref="HttpClientOptions"/>.</param>
    /// <returns>The built <see cref="HttpClientRequestOptionsAttribute"/>.</returns>
    HttpClientRequestOptionsAttribute Build(HttpClientOptions options);
}
