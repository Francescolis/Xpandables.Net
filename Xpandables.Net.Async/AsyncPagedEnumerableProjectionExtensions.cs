/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides projection extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class AsyncPagedEnumerableProjectionExtensions
{
    /// <summary>
    /// Projection operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region Select / Projection

        /// <summary>
        /// Projects each element of the asynchronous paged sequence into a new form using a synchronous selector.
        /// Reuses <see cref="AsyncPagedEnumerable{TSource, TResult}"/> for enumeration and pagination propagation.
        /// </summary>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="selector">Synchronous projection function.</param>
        public IAsyncPagedEnumerable<TResult> SelectPaged<TResult>(Func<TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            return new AsyncPagedEnumerable<TSource, TResult>(
                source,
                ct => new ValueTask<Pagination>(source.GetPageContextAsync(ct)),
                (s, ct) => ValueTask.FromResult(selector(s)));
        }

        /// <summary>
        /// Projects each element using an asynchronous selector (without cancellation token).
        /// </summary>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="selectorAsync">Asynchronous projection function.</param>
        public IAsyncPagedEnumerable<TResult> SelectPagedAsync<TResult>(Func<TSource, ValueTask<TResult>> selectorAsync)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selectorAsync);
            return new AsyncPagedEnumerable<TSource, TResult>(
                source,
                ct => new ValueTask<Pagination>(source.GetPageContextAsync(ct)),
                (s, ct) => selectorAsync(s));
        }

        /// <summary>
        /// Projects each element using an asynchronous selector supporting cancellation.
        /// </summary>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="selectorAsync">Cancellation-aware asynchronous projection function.</param>
        public IAsyncPagedEnumerable<TResult> SelectPagedAsync<TResult>(Func<TSource, CancellationToken, ValueTask<TResult>> selectorAsync)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selectorAsync);
            return new AsyncPagedEnumerable<TSource, TResult>(
                source,
                ct => new ValueTask<Pagination>(source.GetPageContextAsync(ct)),
                selectorAsync);
        }

        #endregion
    }
}