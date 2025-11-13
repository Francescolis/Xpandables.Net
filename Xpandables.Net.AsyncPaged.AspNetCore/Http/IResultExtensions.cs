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

using Microsoft.AspNetCore.Http;

using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides extension methods for converting asynchronous paged enumerables to result objects.
/// </summary>
public static class IResultExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult">Result element type.</typeparam>
    /// <param name="source">The asynchronous enumerable representing the data source to be paginated.</param>
    extension<TResult>(IAsyncPagedEnumerable<TResult> source)
    {
        /// <summary>
        /// Converts the current asynchronous paged enumerable to an <see cref="IResult"/> instance.
        /// </summary>
        /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{TResult}"/> used for serializing the result items. Cannot be null.</param>
        /// <returns>An <see cref="IResult"/> that represents the asynchronous paged enumerable.</returns>
        public IResult ToResult(JsonTypeInfo<TResult> jsonTypeInfo) => new AsyncPagedEnumerableResult<TResult>(source, default, jsonTypeInfo);

        /// <summary>
        /// Converts the current asynchronous paged enumerable to an <see cref="IResult"/> instance.
        /// </summary>
        /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> used for serializing the result items. Cannot be null.</param>
        /// <returns>An <see cref="IResult"/> that represents the asynchronous paged enumerable.</returns>
        public IResult ToResult(JsonSerializerOptions serializerOptions) => new AsyncPagedEnumerableResult<TResult>(source, serializerOptions);

        /// <summary>
        /// Converts the current asynchronous paged enumerable to an <see cref="IResult"/> instance.
        /// </summary>
        /// <returns>An <see cref="IResult"/> that represents the asynchronous paged enumerable.</returns>
        /// <remarks>This overload uses will require the <see cref="JsonTypeInfo"/> from the context.</remarks>
        public IResult ToResult() => new AsyncPagedEnumerableResult<TResult>(source);
    }
}
