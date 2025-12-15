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
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for converting asynchronous paged enumerables to result objects suitable for
/// serialization or further processing.
/// </summary>
/// <remarks>These extension methods enable seamless transformation of asynchronous paged data sources into result
/// types that can be used in APIs or other layers requiring standardized result representations. The methods support
/// customization of serialization behavior through optional parameters.</remarks>
public static class AsyncPagedExtensions
{
    extension<TResult>(IAsyncPagedEnumerable<TResult> enumerable)
    {
        /// <summary>
        /// Converts the current enumerable sequence to an asynchronous paged result.
        /// </summary>
        /// <returns>An <see cref="IResult"/> instance that represents the asynchronous paged result of the sequence.</returns>
        public IResult ToResult() => new AsyncPagedResult<TResult>(enumerable);

        /// <summary>
        /// Converts the current paged enumerable to an HTTP result using the specified JSON serialization options.
        /// </summary>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing the result to JSON. Cannot be null.</param>
        /// <returns>An <see cref="IResult"/> that represents the paged data serialized as JSON using the provided options.</returns>
        public IResult ToResult(JsonSerializerOptions options) => new AsyncPagedResult<TResult>(enumerable, options);

        /// <summary>
        /// Creates an asynchronous paged result that serializes items using the specified JSON type information.
        /// </summary>
        /// <param name="jsonTypeInfo">The metadata used to control JSON serialization for the result items. Cannot be null.</param>
        /// <returns>An <see cref="IResult"/> that represents the asynchronous paged result, serialized according to the provided
        /// JSON type information.</returns>
        public IResult ToResult(JsonTypeInfo<TResult> jsonTypeInfo) => new AsyncPagedResult<TResult>(enumerable, null, jsonTypeInfo);
    }
}
