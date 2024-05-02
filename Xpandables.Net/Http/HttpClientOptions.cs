/*******************************************************************************
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
********************************************************************************/
namespace Xpandables.Net.Http;

/// <summary>
/// Defines the <see cref="IHttpClientDispatcher"/>> configuration options.
/// </summary>
public sealed record HttpClientOptions
{
    /// <summary>
    /// Gets the list of user-defined response builders that were registered.
    /// </summary>
    public IList<HttpClientResponseBuilder> ResponseBuilders { get; }
        = new List<HttpClientResponseBuilder>();
}
