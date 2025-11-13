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
using System.Runtime.CompilerServices;

using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Collections.Extensions;

/// <summary>
/// Provides projection extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
public static class ProjectionExtensions
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
        /// </summary>
        /// <typeparam name="TResult">Result element type.</typeparam>
        /// <param name="selector">Synchronous projection function.</param>
        public IAsyncPagedEnumerable<TResult> SelectPaged<TResult>(Func<TSource, TResult> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            return new AsyncPagedEnumerable<TResult>(
                source.Select(selector),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
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

            async IAsyncEnumerable<TResult> ProjectAsync([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return await selectorAsync(item).ConfigureAwait(false);
                }
            }

            return new AsyncPagedEnumerable<TResult>(
                ProjectAsync(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
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

            async IAsyncEnumerable<TResult> ProjectAsync([EnumeratorCancellation] CancellationToken ct = default)
            {
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return await selectorAsync(item, ct).ConfigureAwait(false);
                }
            }

            return new AsyncPagedEnumerable<TResult>(
                ProjectAsync(),
                ct => new ValueTask<Pagination>(source.GetPaginationAsync(ct)));
        }

        #endregion
    }
}