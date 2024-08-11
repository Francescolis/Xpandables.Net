
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

// Ignore Spelling: Multipart

using Xpandables.Net.Http;

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.Requests;

/// <summary>
/// Provides with a method to retrieve the 
/// request content for <see cref="Location.Header"/>.
/// </summary>
public interface IHttpRequestHeader : IHttpRequest
{
    /// <summary>
    /// Returns the keys and values for the header content.
    /// If a key is already present, its value will be replaced with the new one.
    /// </summary>
    IDictionary<string, string?> GetHeaderSource();

    /// <summary>
    /// Returns the keys and values for the header content.
    /// If a key is already present, its value will be replaced with the new one.
    /// </summary>
    IDictionary<string, IEnumerable<string?>> GetHeadersSource()
        => GetHeaderSource()
        .ToDictionary(d => d.Key, d => (IEnumerable<string?>)[d.Value]);

    /// <summary>
    /// Returns the model name of the header attribute.
    /// </summary>
    /// <returns>The model name for the header attribute.</returns>
    public string? GetHeaderModelName() => null;
}
