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
using System.Runtime.CompilerServices;

namespace Xpandables.Net.Collections;

public static partial class AsyncPagedEnumerableExtensions
{
    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The asynchronous paged sequence to transform. Cannot be <see langword="null"/>.</param>
    /// <param name="selector">A transform function to apply to each element of the source sequence. Cannot be <see langword="null"/>.</param>
    /// <returns>An asynchronous paged sequence whose elements are the result of invoking the transform function on each element
    /// of the source sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> Select<TSource, TResult>(
    this IAsyncPagedEnumerable<TSource> source,
    Func<TSource, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return new AsyncPagedEnumerable<TSource, TResult>(
            source,
            ct => source.GetPaginationAsync().AsValueTask(),
            selector);
    }

    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form by applying an asynchronous transform
    /// function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source asynchronous paged sequence to transform. Cannot be <see langword="null"/>.</param>
    /// <param name="selector">A function to asynchronously transform each element of the source sequence into a new form.  Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>An asynchronous paged sequence whose elements are the result of invoking the asynchronous  transform function on
    /// each element of the source sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectAwait<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return new AsyncPagedEnumerable<TSource, TResult>(
            source,
            ct => source.GetPaginationAsync().AsValueTask(),
            (item, _) => selector(item));
    }

    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form by incorporating a cancellation token
    /// into the asynchronous selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source asynchronous paged sequence to transform.</param>
    /// <param name="selector">A function to asynchronously transform each element of the source sequence. The function takes the source
    /// element and a <see cref="CancellationToken"/> as parameters and returns a <see cref="ValueTask{TResult}"/>
    /// representing the transformed element.</param>
    /// <returns>An asynchronous paged sequence of <typeparamref name="TResult"/> containing the transformed elements.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectAwaitWithCancellation<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        return new AsyncPagedEnumerable<TSource, TResult>(
            source,
            ct => source.GetPaginationAsync().AsValueTask(),
            selector);
    }

    /// <summary>
    /// Filters the elements of an asynchronous paged enumerable based on a specified predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to filter. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The function should return <see langword="true"/>      to
    /// include the element in the result; otherwise, <see langword="false"/>.     Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the input enumerable      that
    /// satisfy the condition specified by <paramref name="predicate"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> Where<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var filtered = FilterAsync(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            filtered,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Filters the elements of an asynchronous paged enumerable based on a predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to filter. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function that asynchronously evaluates each element to determine whether it should be included in the result.
    /// The function returns a <see cref="ValueTask{Boolean}"/> that resolves to <see langword="true"/> to include the
    /// element, or <see langword="false"/> to exclude it. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the input sequence that satisfy
    /// the condition specified by <paramref name="predicate"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> WhereAwait<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var filtered = FilterAsync(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            filtered,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Filters the elements of an asynchronous paged enumerable based on a predicate that supports asynchronous
    /// evaluation with cancellation.
    /// </summary>
    /// <remarks>This method allows filtering of asynchronous paged data streams where the filtering logic
    /// requires asynchronous operations or cancellation support. The predicate is evaluated for each element in the
    /// source sequence, and only elements for which the predicate returns <see langword="true"/> are included in the
    /// result.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable to filter. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function that asynchronously evaluates each element of the source sequence and determines whether it should be
    /// included in the result. The function takes the element and a <see cref="CancellationToken"/> as parameters and
    /// returns a <see cref="ValueTask{Boolean}"/> indicating whether the element satisfies the condition. Cannot be
    /// <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the source sequence that satisfy
    /// the condition specified by the <paramref name="predicate"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> WhereAwaitWithCancellation<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var filtered = FilterAsync(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            filtered,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, CancellationToken, ValueTask<TSource>>?)null);
    }

    /// <summary>
    /// Skips the specified number of elements in the asynchronous paged sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to skip elements from.</param>
    /// <param name="count">The number of elements to skip. If <paramref name="count"/> is less than or equal to 0, the original sequence is
    /// returned.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements of the source sequence after skipping
    /// the specified number of elements.</returns>
    public static IAsyncPagedEnumerable<TSource> Skip<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (count <= 0) return source;

        var iterator = SkipIterator(source, count);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async _ =>
            {
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
                var newSkip = (p.Skip ?? 0) + count;
                // Preserve original Take; only Skip changes here
                return Pagination.With(newSkip, p.Take, p.TotalCount);
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns a sequence containing the first <paramref name="count"/> elements of the source sequence.
    /// </summary>
    /// <remarks>If the source sequence contains fewer elements than <paramref name="count"/>, the entire
    /// sequence is returned. This method preserves the pagination metadata of the source sequence, adjusting the total
    /// count and take values as necessary.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to take elements from.</param>
    /// <param name="count">The number of elements to take from the beginning of the sequence. Must be greater than or equal to 0.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains at most <paramref name="count"/> elements  from
    /// the beginning of the source sequence. If <paramref name="count"/> is 0 or less, an empty sequence is returned.</returns>
    public static IAsyncPagedEnumerable<TSource> Take<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (count <= 0)
        {
            // Empty sequence, but preserve total and skip
            var empty = EmptyIterator<TSource>();
            return new AsyncPagedEnumerable<TSource, TSource>(
                empty,
                async _ =>
                {
                    var p = await source.GetPaginationAsync().ConfigureAwait(false);
                    return Pagination.With(p.Skip, 0, p.TotalCount);
                },
                (Func<TSource, TSource>?)null);
        }

        var iterator = TakeIterator(source, count);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async _ =>
            {
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
                var effectiveTake = p.Take.HasValue ? Math.Min(p.Take.Value, count) : count;
                return Pagination.With(p.Skip, effectiveTake, p.TotalCount);
            },
            (Func<TSource, TSource>?)null);
    }

    private static async IAsyncEnumerable<TSource> FilterAsync<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (predicate(item)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> FilterAsync<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (await predicate(item).ConfigureAwait(false)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> FilterAsync<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (await predicate(item, ct).ConfigureAwait(false)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> SkipIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var skipped = 0;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (skipped < count)
            {
                skipped++;
                continue;
            }

            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> TakeIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (count <= 0) yield break;

        var remaining = count;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
            if (--remaining == 0) yield break;
        }
    }

    private static async IAsyncEnumerable<TSource> EmptyIterator<TSource>(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1849 // Call async methods when in an async method
    private static ValueTask<T> AsValueTask<T>(this Task<T> task) =>
        task.IsCompletedSuccessfully ? new ValueTask<T>(task.Result) : new ValueTask<T>(task);
#pragma warning restore CA1849 // Call async methods when in an async method
}