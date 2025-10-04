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
using System.Net.Async;

namespace System.Net.Async;

/// <summary>
/// Provides element access extension methods for <see cref="IAsyncPagedEnumerable{TSource}"/>.
/// </summary>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class AsyncPagedEnumerableElementAccessExtensions
{
    /// <summary>
    /// Element access operations over an <see cref="IAsyncPagedEnumerable{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable.</param>
    extension<TSource>(IAsyncPagedEnumerable<TSource> source)
    {
        #region First / FirstOrDefault

        /// <summary>
        /// Returns the first element of the asynchronous paged sequence.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous retrieval of the first element.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no elements.</exception>
        public async ValueTask<TSource> FirstPagedAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                return item;
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        /// <summary>
        /// Returns the first element of the asynchronous paged sequence that satisfies the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate function applied to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>A task that produces the first matching element.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no matching element.</exception>
        public async ValueTask<TSource> FirstPagedAsync(
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                if (predicate(item)) return item;
            throw new InvalidOperationException("Sequence contains no matching element.");
        }

        /// <summary>
        /// Returns the first element of the asynchronous paged sequence, or the default value if the sequence contains no elements.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The first element if found; otherwise the default value of <typeparamref name="TSource"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<TSource?> FirstOrDefaultPagedAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                return item;
            return default;
        }

        /// <summary>
        /// Returns the first element of the asynchronous paged sequence that satisfies the predicate, or the default value if no such element exists.
        /// </summary>
        /// <param name="predicate">The predicate function applied to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The first matching element if found; otherwise the default value of <typeparamref name="TSource"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public async ValueTask<TSource?> FirstOrDefaultPagedAsync(
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                if (predicate(item)) return item;
            return default;
        }

        #endregion

        #region Last / LastOrDefault

        /// <summary>
        /// Returns the last element of the asynchronous paged sequence.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The last element in the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no elements.</exception>
        public async ValueTask<TSource> LastPagedAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            bool found = false; TSource last = default!;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            { found = true; last = item; }
            if (!found) throw new InvalidOperationException("Sequence contains no elements.");
            return last;
        }

        /// <summary>
        /// Returns the last element of the asynchronous paged sequence that satisfies the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate function applied to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The last matching element in the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no matching element.</exception>
        public async ValueTask<TSource> LastPagedAsync(
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            bool found = false; TSource last = default!;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                if (predicate(item)) { found = true; last = item; }
            if (!found) throw new InvalidOperationException("Sequence contains no matching element.");
            return last;
        }

        /// <summary>
        /// Returns the last element of the asynchronous paged sequence, or the default value if the sequence is empty.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The last element if found; otherwise the default value of <typeparamref name="TSource"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        public async ValueTask<TSource?> LastOrDefaultPagedAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            bool found = false; TSource last = default!;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            { found = true; last = item; }
            return found ? last : default;
        }

        /// <summary>
        /// Returns the last element of the asynchronous paged sequence that satisfies the specified predicate, or the default value if no such element exists.
        /// </summary>
        /// <param name="predicate">The predicate function applied to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The last matching element if found; otherwise the default value of <typeparamref name="TSource"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        public async ValueTask<TSource?> LastOrDefaultPagedAsync(
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            bool found = false; TSource last = default!;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                if (predicate(item)) { found = true; last = item; }
            return found ? last : default;
        }

        #endregion

        #region Single / SingleOrDefault

        /// <summary>
        /// Returns the single element in the asynchronous paged sequence and throws if the sequence contains no elements or more than one element.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The sole element of the sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no elements or more than one element.</exception>
#pragma warning disable CA2007 // ConfigureAwait not applicable to await using enumerator acquisition
        public async ValueTask<TSource> SinglePagedAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            await using var e = source.GetAsyncEnumerator(cancellationToken);
            if (!await e.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains no elements.");
            TSource result = e.Current;
            if (await e.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains more than one element.");
            return result;
        }

        /// <summary>
        /// Returns the single element in the asynchronous paged sequence that satisfies the predicate and throws if no such element exists or more than one exists.
        /// </summary>
        /// <param name="predicate">The predicate function applied to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The single matching element.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no matching element or more than one matching element.</exception>
        public async ValueTask<TSource> SinglePagedAsync(
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            bool found = false; TSource match = default!;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!predicate(item)) continue;
                if (found) throw new InvalidOperationException("Sequence contains more than one matching element.");
                found = true; match = item;
            }
            if (!found) throw new InvalidOperationException("Sequence contains no matching element.");
            return match;
        }

        /// <summary>
        /// Returns the single element in the asynchronous paged sequence, or the default value if the sequence is empty; throws if more than one element is present.
        /// </summary>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The single element if the sequence contains exactly one element; otherwise the default value of <typeparamref name="TSource"/> if empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains more than one element.</exception>
        public async ValueTask<TSource?> SingleOrDefaultPagedAsync(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            await using var e = source.GetAsyncEnumerator(cancellationToken);
            if (!await e.MoveNextAsync().ConfigureAwait(false))
                return default;
            TSource result = e.Current;
            if (await e.MoveNextAsync().ConfigureAwait(false))
                throw new InvalidOperationException("Sequence contains more than one element.");
            return result;
        }

        /// <summary>
        /// Returns the single element in the asynchronous paged sequence that satisfies the predicate, or the default value if no such element exists; throws if more than one matching element exists.
        /// </summary>
        /// <param name="predicate">The predicate function applied to each element.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The single matching element if exactly one match is found; otherwise the default value of <typeparamref name="TSource"/> if none match.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence or predicate is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when more than one matching element exists.</exception>
        public async ValueTask<TSource?> SingleOrDefaultPagedAsync(
            Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            bool found = false; TSource match = default!;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!predicate(item)) continue;
                if (found) throw new InvalidOperationException("Sequence contains more than one matching element.");
                found = true; match = item;
            }
            return found ? match : default;
        }

        #endregion

        #region ElementAt / ElementAtOrDefault

        /// <summary>
        /// Returns the element at the specified zero-based index in the asynchronous paged sequence.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The element at the specified position.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is negative or greater than or equal to the number of elements.</exception>
        public async ValueTask<TSource> ElementAtPagedAsync(
            int index,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            int i = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (i == index) return item;
                i++;
            }
            throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range.");
        }

        /// <summary>
        /// Returns the element at the specified zero-based index in the asynchronous paged sequence, or the default value if the index is outside the bounds of the sequence.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <param name="cancellationToken">A token to observe cancellation.</param>
        /// <returns>The element at the specified index if found; otherwise the default value of <typeparamref name="TSource"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source sequence is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is negative.</exception>
        public async ValueTask<TSource?> ElementAtOrDefaultPagedAsync(
            int index,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            int i = 0;
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (i == index) return item;
                i++;
            }
            return default;
        }

        #endregion
    }
}