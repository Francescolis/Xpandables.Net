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
using System.Net.Http.Headers;

namespace Xpandables.Net.ExecutionResults.Collections;

/// <summary>
/// Provides extension methods for working with HTTP headers collections.
/// </summary>
/// <remarks>This class contains static methods that extend the functionality of <see cref="HttpHeaders"/>
/// instances, enabling convenient manipulation and conversion of HTTP header data. All methods are thread-safe and do
/// not modify the original headers collection.</remarks>
public static class HttpHeadersExtensions
{
    ///<summary>
    /// Provides extensions for <see cref="HttpHeaders"/>.
    ///</summary>
    extension(HttpHeaders headers)
    {
        /// <summary>
        /// Converts the current headers to an ElementCollection containing all header names and their associated
        /// values.
        /// </summary>
        /// <returns>An ElementCollection containing each header name as a key and its corresponding values as a collection. The
        /// collection is empty if there are no headers.</returns>
        public ElementCollection ToElementCollection()
        {
            ArgumentNullException.ThrowIfNull(headers);

            ElementCollection collection = [];
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                collection.Add(header.Key, [.. header.Value]);
            }

            return collection;
        }
    }
}
