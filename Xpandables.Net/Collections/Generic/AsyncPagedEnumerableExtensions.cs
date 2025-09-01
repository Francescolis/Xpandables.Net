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
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            selector);
    }

    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form by incorporating the element's index.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The asynchronous paged sequence to transform. Cannot be <see langword="null"/>.</param>
    /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the
    /// element in the source sequence. Cannot be <see langword="null"/>.</param>
    /// <returns>An asynchronous paged sequence whose elements are the result of invoking the transform function on each element
    /// of the source sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> Select<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, int, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var iterator = SelectIndexIterator(source, selector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iterator,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Projects each element of a paged asynchronous sequence to an <see cref="IEnumerable{T}"/>  and flattens the
    /// resulting sequences into a single paged asynchronous sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source sequence to project and flatten. Cannot be <see langword="null"/>.</param>
    /// <param name="collectionSelector">A function to apply to each element of the source sequence to produce an <see cref="IEnumerable{T}"/>  of
    /// results. Cannot be <see langword="null"/>.</param>
    /// <returns>A paged asynchronous sequence whose elements are the result of invoking the  <paramref
    /// name="collectionSelector"/> function on each element of the source sequence  and flattening the resulting
    /// sequences.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, IEnumerable<TResult>> collectionSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);

        var iter = SelectManyIterator(source, collectionSelector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Projects each element of a paged asynchronous sequence to an enumerable collection, flattens the resulting
    /// sequences into a single sequence, and invokes a result selector function on each element.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TCollection">The type of the elements in the intermediate collections.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source sequence to project and flatten. Cannot be <see langword="null"/>.</param>
    /// <param name="collectionSelector">A function to project each element of the source sequence into an enumerable collection. Cannot be <see
    /// langword="null"/>.</param>
    /// <param name="resultSelector">A function to create a result element from an element of the source sequence and an element of the intermediate
    /// collection. Cannot be <see langword="null"/>.</param>
    /// <returns>A paged asynchronous sequence whose elements are the result of invoking the result selector function on each
    /// element of the source sequence and each element of the intermediate collections.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, IEnumerable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var iter = SelectManyIterator(source, collectionSelector, resultSelector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Projects each element of a paged asynchronous sequence to an asynchronous sequence and flattens the resulting
    /// sequences into a single paged asynchronous sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source sequence to project and flatten. Cannot be <see langword="null"/>.</param>
    /// <param name="collectionSelector">A function to apply to each element of the source sequence to produce an asynchronous sequence.  Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>A paged asynchronous sequence whose elements are the result of invoking the projection function on each element
    /// of the source sequence and flattening the resulting sequences.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, IAsyncEnumerable<TResult>> collectionSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);

        var iter = SelectManyAsyncIterator(source, collectionSelector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Projects each element of a sequence to an asynchronous collection and flattens the resulting sequences into a
    /// single sequence, optionally transforming the elements using a specified result selector.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TCollection">The type of the elements in the intermediate asynchronous collections.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source sequence to project and flatten.</param>
    /// <param name="collectionSelector">A function to map each element of the source sequence to an asynchronous collection.</param>
    /// <param name="resultSelector">A function to create a result element from an element of the source sequence and an element of the intermediate
    /// collection.</param>
    /// <returns>An asynchronous paged enumerable that contains the flattened and optionally transformed elements of the
    /// intermediate collections.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var iter = SelectManyAsyncIterator(source, collectionSelector, resultSelector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
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
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (item, _) => selector(item));
    }

    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form by incorporating the element's index.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source asynchronous paged sequence to project.</param>
    /// <param name="selector">A transform function to apply to each element and its index. The function receives the element of type 
    /// <typeparamref name="TSource"/> and its zero-based index, and returns a <see cref="ValueTask{TResult}"/> 
    /// representing the projected element of type <typeparamref name="TResult"/>.</param>
    /// <returns>An asynchronous paged sequence of <typeparamref name="TResult"/> containing the projected elements.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectAwait<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, int, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var iterator = SelectIndexIterator(source, selector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iterator,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (Func<TResult, TResult>?)null);
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
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            selector);
    }

    /// <summary>
    /// Projects each element of a paged asynchronous sequence into a new form by incorporating the element's index and
    /// a cancellation token.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source sequence to transform. Cannot be <see langword="null"/>.</param>
    /// <param name="selector">A transform function to apply to each element. The function receives the element, its zero-based index, and a
    /// <see cref="CancellationToken"/>. Cannot be <see langword="null"/>.</param>
    /// <returns>A paged asynchronous sequence whose elements are the result of invoking the transform function on each element
    /// of the source sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> SelectAwaitWithCancellation<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, int, CancellationToken, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var iterator = SelectIndexIterator(source, selector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iterator,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (Func<TResult, TResult>?)null);
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
    /// Filters the elements of an asynchronous paged sequence based on a predicate that includes the element's index.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged sequence to filter. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element and its index for a condition. The second parameter of the function represents
    /// the zero-based index of the element in the sequence.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the input sequence that satisfy the
    /// condition specified by <paramref name="predicate"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> Where<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, int, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = WhereIndexIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
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
    /// Filters the elements of an asynchronous paged sequence based on a predicate that incorporates the element's
    /// index.
    /// </summary>
    /// <remarks>The predicate function receives two arguments: the element to test and the zero-based index
    /// of the element in the source sequence. This method is designed for use with asynchronous paged sequences, where
    /// elements are processed lazily as pages are enumerated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged sequence to filter. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function that evaluates each element and its index to determine whether it should be included in the result.
    /// The function returns a <see cref="ValueTask{Boolean}"/> indicating whether the element satisfies the condition.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains elements from the input sequence that satisfy the
    /// condition specified by the predicate.</returns>
    public static IAsyncPagedEnumerable<TSource> WhereAwait<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, int, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = WhereIndexIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
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
    /// Filters the elements of an asynchronous sequence based on a predicate that incorporates the element's index and
    /// a cancellation token.
    /// </summary>
    /// <remarks>This method allows filtering of elements in an asynchronous sequence based on both their
    /// value and their index, while also supporting cancellation. The predicate function is invoked asynchronously for
    /// each element, and the operation respects the provided <see cref="CancellationToken"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to filter. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The function receives the element, its zero-based index, and a
    /// <see cref="CancellationToken"/>. The function must return a <see cref="ValueTask{Boolean}"/> indicating whether
    /// the element should be included in the result.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> that contains elements from the input sequence that satisfy the
    /// condition specified by the <paramref name="predicate"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> WhereAwaitWithCancellation<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = WhereIndexIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
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
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
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
                async ct =>
                {
                    var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                    return Pagination.With(p.Skip, 0, p.TotalCount);
                },
                (Func<TSource, TSource>?)null);
        }

        var iterator = TakeIterator(source, count);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                var effectiveTake = p.Take.HasValue ? Math.Min(p.Take.Value, count) : count;
                return Pagination.With(p.Skip, effectiveTake, p.TotalCount);
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Creates a new asynchronous paged enumerable that skips a specified number of elements  and then takes a
    /// specified number of elements from the source sequence.
    /// </summary>
    /// <remarks>This method allows you to create a paged view of an asynchronous sequence by specifying the
    /// number of elements  to skip and the number of elements to take. The resulting sequence will reflect the
    /// specified slice, and any  pagination metadata (e.g., total count) will be adjusted accordingly.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to slice. Cannot be <see langword="null"/>.</param>
    /// <param name="skip">The number of elements to skip before starting to take elements. Must be non-negative.</param>
    /// <param name="take">The maximum number of elements to take from the source sequence. Must be greater than zero.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that represents the sliced portion of the source sequence. If
    /// <paramref name="take"/> is less than or equal to zero, an empty sequence is returned.</returns>
    public static IAsyncPagedEnumerable<TSource> Slice<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (take <= 0)
        {
            var empty = EmptyIterator<TSource>();
            return new AsyncPagedEnumerable<TSource, TSource>(
                empty,
                async ct =>
                {
                    var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                    var newSkip = (p.Skip ?? 0) + Math.Max(0, skip);
                    return Pagination.With(newSkip, 0, p.TotalCount);
                },
                (Func<TSource, TSource>?)null);
        }

        if (skip <= 0)
        {
            return source.Take(take);
        }

        var iterator = SliceIterator(source, skip, take);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                var newSkip = (p.Skip ?? 0) + Math.Max(0, skip);
                var effectiveTake = p.Take.HasValue ? Math.Min(p.Take.Value, take) : take;
                return Pagination.With(newSkip, effectiveTake, p.TotalCount);
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Retrieves a specific page of items from the source sequence based on the specified page number and page size.
    /// </summary>
    /// <remarks>This method supports asynchronous pagination and provides metadata about the pagination
    /// state, such as the total number of items in the source sequence, if available. The pagination is zero-based
    /// internally, but the <paramref name="pageNumber"/> parameter is 1-based for user convenience.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to paginate. Cannot be <see langword="null"/>.</param>
    /// <param name="pageNumber">The 1-based page number to retrieve. Must be greater than 0.</param>
    /// <param name="pageSize">The number of items per page. Must be greater than 0.</param>
    /// <returns>An asynchronous enumerable representing the specified page of items. The enumerable will contain at most
    /// <paramref name="pageSize"/> items, or fewer if the source sequence does not contain enough items.</returns>
    public static IAsyncPagedEnumerable<TSource> Page<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var skip = (pageNumber - 1) * pageSize;
        var iterator = SliceIterator(source, skip, pageSize);

        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                var newSkip = (p.Skip ?? 0) + skip;
                var effectiveTake = p.Take.HasValue ? Math.Min(p.Take.Value, pageSize) : pageSize;
                // Derive page-related hints if total is known
                var meta = Pagination.With(newSkip, effectiveTake, p.TotalCount);
                return meta;
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Filters the elements of an asynchronous paged enumerable based on their type.
    /// </summary>
    /// <typeparam name="TResult">The type to filter the elements of the source sequence to.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable whose elements are to be filtered.</param>
    /// <returns>An asynchronous paged enumerable that contains elements from the source sequence of type <typeparamref
    /// name="TResult"/>.</returns>
    public static IAsyncPagedEnumerable<TResult> OfType<TResult>(
        this IAsyncPagedEnumerable<object?> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var iterator = OfTypeIterator<object?, TResult>(source);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Converts the elements of the specified asynchronous paged enumerable to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to which the elements of the source enumerable will be cast.</typeparam>
    /// <param name="source">The asynchronous paged enumerable whose elements are to be cast. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TResult}"/> containing the elements of the source enumerable cast to the
    /// specified type.</returns>
    public static IAsyncPagedEnumerable<TResult> Cast<TResult>(
        this IAsyncPagedEnumerable<object?> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // For upcasts (TSource -> TResult assignable) we still project, but metadata is preserved.
        return new AsyncPagedEnumerable<object?, TResult>(
            source,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            item => (TResult)item!);
    }

    /// <summary>
    /// Adds total count information to an asynchronous paged enumerable.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable to which total count information will be added.</param>
    /// <param name="totalFactory">A delegate that asynchronously computes the total count of items in the sequence.  The delegate receives a <see
    /// cref="CancellationToken"/> to support cancellation.</param>
    /// <returns>A new asynchronous paged enumerable that includes total count information alongside the original sequence.</returns>
    public static IAsyncPagedEnumerable<TSource> WithTotal<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<CancellationToken, ValueTask<long>> totalFactory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(totalFactory);

        return new AsyncPagedEnumerable<TSource, TSource>(
            source,
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                var total = await totalFactory(ct).ConfigureAwait(false);
                return Pagination.With(p.Skip, p.Take, total);
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Configures the pagination behavior for the asynchronous paged enumerable.
    /// </summary>
    /// <remarks>Use with caution as this will override any existing pagination metadata.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable to configure.</param>
    /// <param name="paginationFactory">A factory method that provides pagination settings. The method is invoked with a  <see
    /// cref="CancellationToken"/> and returns a <see cref="ValueTask{TResult}"/> containing  the pagination
    /// configuration.</param>
    /// <returns>A new asynchronous paged enumerable with the specified pagination behavior applied.</returns>
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(paginationFactory);

        return new AsyncPagedEnumerable<TSource, TSource>(
            source,
            paginationFactory,
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Transforms each element in the asynchronous paged sequence using the specified mapping function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source asynchronous paged sequence to transform. Cannot be <see langword="null"/>.</param>
    /// <param name="mapper">A function that maps each element of the source sequence to an element in the resulting sequence. Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>An asynchronous paged sequence containing the transformed elements.</returns>
    public static IAsyncPagedEnumerable<TResult> Map<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TResult> mapper) =>
        Select(source, mapper);

    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form using the specified asynchronous mapping
    /// function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source asynchronous paged sequence to transform.</param>
    /// <param name="mapper">An asynchronous function to apply to each element of the source sequence.</param>
    /// <returns>An asynchronous paged sequence whose elements are the result of invoking the specified mapping function on each
    /// element of the source sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> MapAwait<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<TResult>> mapper) =>
        SelectAwait(source, mapper);

    /// <summary>
    /// Projects each element of an asynchronous paged sequence into a new form using a mapping function that supports
    /// cancellation.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="source">The source asynchronous paged sequence to transform.</param>
    /// <param name="mapper">A function that maps each element of the source sequence to a new form. The function receives the element and a 
    /// <see cref="CancellationToken"/> to support cooperative cancellation.</param>
    /// <returns>An asynchronous paged sequence whose elements are the result of invoking the mapping function on each element of
    /// the source sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> MapAwaitWithCancellation<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<TResult>> mapper) =>
        SelectAwaitWithCancellation(source, mapper);

    /// <summary>
    /// Ensures that the specified asynchronous paged enumerable is converted to a <see cref="Pagination"/> object.
    /// </summary>
    /// <typeparam name="T">The type of elements in the paged enumerable.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to convert. Cannot be <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Pagination"/> object
    /// representing the paged enumerable.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Pagination> EnsurePaginationAsync<T>(
        this IAsyncPagedEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GetPaginationAsync();
    }

    /// <summary>
    /// Converts an asynchronous paged enumerable to a single page of items along with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of items in the paged enumerable.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to convert. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a tuple containing: <list type="bullet">
    /// <item> <description><see cref="IReadOnlyList{T}"/>: The list of items in the current page.</description> </item>
    /// <item> <description><see cref="Pagination"/>: Metadata about the pagination state.</description> </item> </list></returns>
    public static async Task<(IReadOnlyList<T> Items, Pagination Pagination)> ToPageAsync<T>(
        this IAsyncPagedEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var pagination = await source.GetPaginationAsync(cancellationToken).ConfigureAwait(false);

        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            list.Add(item);
        }

        return (list, pagination);
    }

    /// <summary>
    /// Returns an asynchronous sequence that includes elements from the source sequence as long as a specified
    /// condition is true.
    /// </summary>
    /// <remarks>The evaluation of the predicate is deferred and performed asynchronously as the sequence is
    /// enumerated. Once the predicate returns <see langword="false"/> for an element, no further elements are included
    /// in the resulting sequence.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to evaluate.</param>
    /// <param name="predicate">A function to test each element for a condition. The sequence will include elements until this function returns
    /// <see langword="false"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the start of the source sequence
    /// that satisfy the condition.</returns>
    public static IAsyncPagedEnumerable<TSource> TakeWhile<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = TakeWhileIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns an asynchronous sequence that includes elements from the source sequence  as long as the specified
    /// asynchronous predicate evaluates to <see langword="true"/>.
    /// </summary>
    /// <remarks>The evaluation of the predicate is deferred and performed asynchronously as elements are
    /// enumerated. Once the predicate returns <see langword="false"/> for an element, no further elements are evaluated
    /// or included in the result sequence.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to evaluate.</param>
    /// <param name="predicate">An asynchronous function that determines whether an element should be included in the result sequence. The
    /// function is invoked for each element in the source sequence and should return <see langword="true"/>  to include
    /// the element, or <see langword="false"/> to stop processing further elements.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the source sequence  up to, but
    /// not including, the first element for which the predicate returns <see langword="false"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> TakeWhileAwait<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = TakeWhileIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns an asynchronous sequence that includes elements from the source sequence  as long as the specified
    /// asynchronous predicate evaluates to <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to filter.</param>
    /// <param name="predicate">An asynchronous function that determines whether an element should be included in the result sequence.  The
    /// function takes the current element and a <see cref="CancellationToken"/> as parameters and returns  a <see
    /// cref="ValueTask{Boolean}"/> indicating whether the element satisfies the condition.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the source sequence  that
    /// satisfy the condition defined by the <paramref name="predicate"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> TakeWhileAwaitWithCancellation<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = TakeWhileIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Bypasses elements in the asynchronous sequence as long as the specified condition is true,  and then returns the
    /// remaining elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to process. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The method skips elements while this function  returns <see
    /// langword="true"/> and stops skipping at the first element for which the function  returns <see
    /// langword="false"/>. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the input sequence  starting at
    /// the first element that does not satisfy the condition.</returns>
    public static IAsyncPagedEnumerable<TSource> SkipWhile<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = SkipWhileIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Bypasses elements in the asynchronous sequence as long as the specified predicate evaluates to <see
    /// langword="true"/>.
    /// </summary>
    /// <remarks>The predicate is evaluated asynchronously for each element in the sequence. Once the
    /// predicate returns <see langword="false"/> for an element, that element and all subsequent elements are included
    /// in the resulting sequence.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to process. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The element is skipped while the predicate returns <see
    /// langword="true"/>. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the source sequence starting at
    /// the first element for which the predicate returns <see langword="false"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> SkipWhileAwait<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = SkipWhileIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Bypasses elements in the sequence as long as the specified asynchronous predicate evaluates to <see
    /// langword="true"/>.
    /// </summary>
    /// <remarks>The predicate is evaluated asynchronously for each element in the sequence until it returns
    /// <see langword="false"/>.  Once an element is included, all subsequent elements are included regardless of the
    /// predicate's result.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to process.</param>
    /// <param name="predicate">An asynchronous function that determines whether an element should be skipped. The function takes an element of
    /// the sequence  and a <see cref="CancellationToken"/> as input and returns a <see cref="ValueTask{Boolean}"/>
    /// indicating whether the element  should be skipped (<see langword="true"/>) or included (<see
    /// langword="false"/>).</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the input sequence starting at
    /// the first element  for which the predicate evaluates to <see langword="false"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> SkipWhileAwaitWithCancellation<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var iterator = SkipWhileIterator(source, predicate);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Appends a single element to the end of the asynchronous paged sequence.
    /// </summary>
    /// <remarks>The resulting sequence will have one additional element compared to the source sequence.  If
    /// the source sequence has a defined total count, the total count of the resulting sequence will be incremented by
    /// one.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to which the element will be appended. Cannot be <see langword="null"/>.</param>
    /// <param name="element">The element to append to the sequence.</param>
    /// <returns>A new asynchronous paged sequence that contains all elements of the source sequence followed by the appended
    /// element.</returns>
    public static IAsyncPagedEnumerable<TSource> Append<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        TSource element)
    {
        ArgumentNullException.ThrowIfNull(source);

        var iterator = AppendIterator(source, element);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                var total = p.TotalCount >= 0 ? p.TotalCount + 1 : p.TotalCount;
                return Pagination.With(p.Skip, p.Take, total);
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns a new asynchronous paged enumerable that begins with the specified element,  followed by the elements of
    /// the source sequence.
    /// </summary>
    /// <remarks>The resulting sequence includes <paramref name="element"/> as the first item, followed by the
    /// items in <paramref name="source"/>. The pagination metadata of the resulting sequence adjusts  the total count
    /// to account for the prepended element, if the total count is available.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable to prepend to. Cannot be <see langword="null"/>.</param>
    /// <param name="element">The element to prepend to the sequence.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that starts with <paramref name="element"/>  and continues with
    /// the elements of <paramref name="source"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> Prepend<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        TSource element)
    {
        ArgumentNullException.ThrowIfNull(source);

        var iterator = PrependIterator(source, element);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            async ct =>
            {
                var p = await source.GetPaginationAsync(ct).ConfigureAwait(false);
                var total = p.TotalCount >= 0 ? p.TotalCount + 1 : p.TotalCount;
                return Pagination.With(p.Skip, p.Take, total);
            },
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Concatenates two asynchronous sequences into a single asynchronous paged sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequences.</typeparam>
    /// <param name="first">The first asynchronous paged sequence to concatenate. Cannot be <see langword="null"/>.</param>
    /// <param name="second">The second asynchronous sequence to concatenate. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that represents the concatenation of the two input sequences.</returns>
    public static IAsyncPagedEnumerable<TSource> Concat<TSource>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var iterator = ConcatIterator(first, second);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Concatenates two asynchronous paged sequences into a single sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequences.</typeparam>
    /// <param name="first">The first asynchronous paged sequence to concatenate. Cannot be <see langword="null"/>.</param>
    /// <param name="second">The second asynchronous paged sequence to concatenate. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that represents the concatenation of the two input sequences.</returns>
    public static IAsyncPagedEnumerable<TSource> Concat<TSource>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncPagedEnumerable<TSource> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var iterator = ConcatIterator(first, second);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }


    /// <summary>
    /// Returns a sequence that contains distinct elements from the source sequence, using an optional equality comparer
    /// to determine uniqueness.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to remove duplicate elements from.</param>
    /// <param name="comparer">An optional equality comparer to compare elements for uniqueness. If <see langword="null"/>, the default
    /// equality comparer for <typeparamref name="TSource"/> is used.</param>
    /// <returns>An asynchronous paged enumerable that contains distinct elements from the source sequence.</returns>
    public static IAsyncPagedEnumerable<TSource> Distinct<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        IEqualityComparer<TSource>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        var iterator = DistinctIterator(source, comparer ?? EqualityComparer<TSource>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns the elements of the specified asynchronous sequence, or a single default value  if the sequence is
    /// empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to return elements from, or the default value  if the sequence is empty.</param>
    /// <returns>An asynchronous sequence that contains the elements from the input sequence, or a single  default value of type
    /// <typeparamref name="TSource"/> if the input sequence is empty.</returns>
    public static IAsyncPagedEnumerable<TSource> DefaultIfEmpty<TSource>(
        this IAsyncPagedEnumerable<TSource> source)
        => DefaultIfEmpty(source, default!);

    /// <summary>
    /// Returns the elements of the specified asynchronous paged sequence, or a single element with the specified
    /// default value if the sequence is empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged sequence to return elements from.</param>
    /// <param name="defaultValue">The value to return if the sequence is empty.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the elements from the input sequence, or a single
    /// element with the specified default value if the sequence is empty.</returns>
    public static IAsyncPagedEnumerable<TSource> DefaultIfEmpty<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        TSource defaultValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        var iterator = DefaultIfEmptyIterator(source, defaultValue);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iterator,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Combines two asynchronous sequences by applying a specified function to corresponding elements.
    /// </summary>
    /// <remarks>The sequences are combined element by element. If one sequence is longer than the other, the
    /// resulting sequence  will end when the shorter sequence is exhausted. The <paramref name="resultSelector"/>
    /// function is invoked  for each pair of elements, and its result is included in the output sequence.</remarks>
    /// <typeparam name="TFirst">The type of elements in the first sequence.</typeparam>
    /// <typeparam name="TSecond">The type of elements in the second sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the resulting sequence.</typeparam>
    /// <param name="first">The first asynchronous paged sequence to combine.</param>
    /// <param name="second">The second asynchronous sequence to combine.</param>
    /// <param name="resultSelector">A function that specifies how to combine elements from the two sequences.</param>
    /// <returns>An asynchronous paged sequence of elements, where each element is the result of invoking  <paramref
    /// name="resultSelector"/> on corresponding elements from <paramref name="first"/> and <paramref name="second"/>.</returns>
    public static IAsyncPagedEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
        this IAsyncPagedEnumerable<TFirst> first,
        IAsyncEnumerable<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var iter = ZipIterator(first, second, resultSelector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Combines two asynchronous sequences by applying a specified asynchronous function to corresponding elements.
    /// </summary>
    /// <remarks>The sequences are combined in a pairwise manner. If one sequence is shorter than the other,
    /// the resulting  sequence will end when the shorter sequence is exhausted. The <paramref name="resultSelector"/>
    /// function  is invoked asynchronously for each pair of elements.</remarks>
    /// <typeparam name="TFirst">The type of elements in the first sequence.</typeparam>
    /// <typeparam name="TSecond">The type of elements in the second sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the resulting sequence.</typeparam>
    /// <param name="first">The first asynchronous paged sequence to combine.</param>
    /// <param name="second">The second asynchronous sequence to combine.</param>
    /// <param name="resultSelector">A function that takes an element from the first sequence and an element from the second sequence  and returns a
    /// <see cref="ValueTask{TResult}"/> representing the result of combining the two elements.</param>
    /// <returns>An asynchronous paged sequence of <typeparamref name="TResult"/> elements, where each element is the result  of
    /// invoking <paramref name="resultSelector"/> on corresponding elements from the two input sequences.</returns>
    public static IAsyncPagedEnumerable<TResult> ZipAwait<TFirst, TSecond, TResult>(
        this IAsyncPagedEnumerable<TFirst> first,
        IAsyncEnumerable<TSecond> second,
        Func<TFirst, TSecond, ValueTask<TResult>> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var iter = ZipAsyncIterator(first, second, resultSelector);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Produces the set union of two asynchronous sequences, using an optional equality comparer to determine equality.
    /// </summary>
    /// <remarks>The resulting sequence contains no duplicate elements. The order of elements in the resulting
    /// sequence is not guaranteed.</remarks>
    /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
    /// <param name="first">The first asynchronous sequence to union.</param>
    /// <param name="second">The second asynchronous sequence to union.</param>
    /// <param name="comparer">An optional equality comparer to compare values. If <see langword="null"/>, the default equality comparer for
    /// the type <typeparamref name="TSource"/> is used.</param>
    /// <returns>An asynchronous sequence that contains the distinct elements from both input sequences.</returns>
    public static IAsyncPagedEnumerable<TSource> Union<TSource>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var iter = UnionIterator(first, second, comparer ?? EqualityComparer<TSource>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Produces the set intersection of two asynchronous sequences, returning elements that appear in both sequences.
    /// </summary>
    /// <remarks>The resulting sequence is evaluated lazily and does not perform any operations until
    /// enumerated.  The order of elements in the resulting sequence is determined by the order in <paramref
    /// name="first"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
    /// <param name="first">The first asynchronous sequence to compare.</param>
    /// <param name="second">The second asynchronous sequence to compare.</param>
    /// <param name="comparer">An optional equality comparer to use for comparing elements. If <see langword="null"/>, the default equality
    /// comparer is used.</param>
    /// <returns>An asynchronous sequence that contains the elements that are present in both <paramref name="first"/> and
    /// <paramref name="second"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> Intersect<TSource>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var iter = IntersectIterator(first, second, comparer ?? EqualityComparer<TSource>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Produces the set difference of two asynchronous sequences, using an optional equality comparer to determine
    /// element equality.
    /// </summary>
    /// <remarks>The operation is performed lazily and does not evaluate the sequences immediately. The
    /// resulting sequence will exclude all elements from <paramref name="first"/> that are considered equal to any
    /// element in <paramref name="second"/>, as determined by the specified or default equality comparer.</remarks>
    /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
    /// <param name="first">The first asynchronous sequence whose elements that are not in <paramref name="second"/> will be returned.</param>
    /// <param name="second">The asynchronous sequence whose elements will be excluded from <paramref name="first"/>.</param>
    /// <param name="comparer">An optional equality comparer to compare elements. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TSource"/> is used.</param>
    /// <returns>An asynchronous sequence that contains the elements from <paramref name="first"/> that do not appear in
    /// <paramref name="second"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> Except<TSource>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var iter = ExceptIterator(first, second, comparer ?? EqualityComparer<TSource>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns a sequence of elements from the source collection, ensuring that each element is unique based on a
    /// specified key.
    /// </summary>
    /// <remarks>This method ensures that only the first occurrence of each key, as determined by the
    /// <paramref name="keySelector"/> and <paramref name="comparer"/>, is included in the result. The operation is
    /// deferred, meaning the filtering is performed as the result sequence is enumerated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
    /// <typeparam name="TKey">The type of the key used to determine uniqueness.</typeparam>
    /// <param name="source">The source collection to filter for distinct elements. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract the key for each element. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains distinct elements from the source collection,
    /// determined by the specified key.</returns>
    public static IAsyncPagedEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = DistinctByIterator(source, keySelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Produces the set union of two asynchronous sequences based on a specified key selector function.
    /// </summary>
    /// <remarks>The resulting sequence preserves the order of elements from the <paramref name="first"/>
    /// sequence, followed by elements from the <paramref name="second"/> sequence that have distinct keys.</remarks>
    /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
    /// <typeparam name="TKey">The type of the key used to compare elements.</typeparam>
    /// <param name="first">The first asynchronous paged sequence to union.</param>
    /// <param name="second">The second asynchronous sequence to union.</param>
    /// <param name="keySelector">A function to extract the key for each element. Elements with duplicate keys are excluded from the result.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>An asynchronous paged sequence that contains the distinct elements from both input sequences.</returns>
    public static IAsyncPagedEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = UnionByIterator(first, second, keySelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Produces the set intersection of two asynchronous sequences based on a specified key selector function.
    /// </summary>
    /// <remarks>The comparison is performed using the keys extracted by <paramref name="keySelector"/>.  If
    /// <paramref name="comparer"/> is not provided, the default equality comparer for <typeparamref name="TKey"/> is
    /// used.</remarks>
    /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="first">The first asynchronous paged sequence to compare.</param>
    /// <param name="second">The second asynchronous sequence to compare.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If null, the default equality comparer is used.</param>
    /// <returns>An asynchronous paged sequence that contains the elements that appear in both input sequences,  based on the
    /// keys returned by <paramref name="keySelector"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = IntersectByIterator(first, second, keySelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Produces the set difference of two asynchronous sequences based on a specified key selector.
    /// </summary>
    /// <remarks>This method uses deferred execution and only begins processing when the resulting sequence is
    /// enumerated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
    /// <typeparam name="TKey">The type of the key used to compare elements.</typeparam>
    /// <param name="first">The first asynchronous sequence whose elements that are not also in <paramref name="second"/> will be returned.</param>
    /// <param name="second">The second asynchronous sequence whose elements will be excluded from the result.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>An asynchronous sequence that contains the elements from <paramref name="first"/> that do not have a matching
    /// key in <paramref name="second"/>.</returns>
    public static IAsyncPagedEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = ExceptByIterator(first, second, keySelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Correlates elements from two asynchronous sequences based on matching keys and produces a result value for each
    /// match.
    /// </summary>
    /// <remarks>This method performs an inner join, meaning that only elements with matching keys in both
    /// sequences are included in the result. The outer sequence is paged, while the inner sequence is fully
    /// enumerated.</remarks>
    /// <typeparam name="TOuter">The type of the elements in the outer sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements in the inner sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used to correlate elements from the two sequences.</typeparam>
    /// <typeparam name="TResult">The type of the result elements produced by the join operation.</typeparam>
    /// <param name="outer">The outer asynchronous paged sequence to join.</param>
    /// <param name="inner">The inner asynchronous sequence to join.</param>
    /// <param name="outerKeySelector">A function to extract the key from each element of the outer sequence.</param>
    /// <param name="innerKeySelector">A function to extract the key from each element of the inner sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>An asynchronous paged sequence of result elements, where each result is produced by the <paramref
    /// name="resultSelector"/> function applied to matching elements from the outer and inner sequences.</returns>
    public static IAsyncPagedEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncPagedEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var iter = JoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Correlates elements from two asynchronous sequences based on matching keys and projects the results into a
    /// specified form.
    /// </summary>
    /// <remarks>This method performs a group join, which means that for each element in the <paramref
    /// name="outer"/> sequence,  it produces a result that includes the element and all matching elements from the
    /// <paramref name="inner"/> sequence.  If no elements in the <paramref name="inner"/> sequence match a given
    /// element in the <paramref name="outer"/> sequence,  the result will include the outer element and an empty
    /// collection.</remarks>
    /// <typeparam name="TOuter">The type of the elements in the outer sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements in the inner sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for matching elements in the outer and inner sequences.</typeparam>
    /// <typeparam name="TResult">The type of the result elements produced by the projection function.</typeparam>
    /// <param name="outer">The outer asynchronous sequence to join.</param>
    /// <param name="inner">The inner asynchronous sequence to join.</param>
    /// <param name="outerKeySelector">A function to extract the key from each element of the outer sequence.</param>
    /// <param name="innerKeySelector">A function to extract the key from each element of the inner sequence.</param>
    /// <param name="resultSelector">A function to create a result element from an element of the outer sequence and a collection of matching
    /// elements from the inner sequence.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If null, the default equality comparer is used.</param>
    /// <returns>An asynchronous sequence of result elements, where each result element is produced by the <paramref
    /// name="resultSelector"/>  function applied to an element of the outer sequence and a collection of matching
    /// elements from the inner sequence.</returns>
    public static IAsyncPagedEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
        this IAsyncPagedEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var iter = GroupJoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<TResult, TResult>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TResult, TResult>?)null);
    }

    /// <summary>
    /// Reverses the order of the elements in the specified asynchronous paged enumerable.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The asynchronous paged enumerable whose elements are to be reversed.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that iterates over the elements of the <paramref name="source"/>
    /// in reverse order.</returns>
    public static IAsyncPagedEnumerable<TSource> Reverse<TSource>(
        this IAsyncPagedEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var iter = ReverseIterator(source);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns a new asynchronous paged enumerable that yields the last <paramref name="count"/> elements from the
    /// source sequence.
    /// </summary>
    /// <remarks>This method does not evaluate the entire source sequence immediately. Instead, it defers
    /// execution and processes elements lazily as the resulting sequence is enumerated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source asynchronous paged enumerable to take elements from.</param>
    /// <param name="count">The number of elements to take from the end of the source sequence. If <paramref name="count"/> is less than or
    /// equal to 0, the resulting sequence will be empty.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that contains the last <paramref name="count"/> elements from
    /// the source sequence, or an empty sequence if <paramref name="count"/> is less than or equal to 0.</returns>
    public static IAsyncPagedEnumerable<TSource> TakeLast<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (count <= 0)
        {
            var empty = EmptyIterator<TSource>();
            return new AsyncPagedEnumerable<TSource, TSource>(
                empty,
                static _ => new ValueTask<Pagination>(Pagination.Without()),
                (Func<TSource, TSource>?)null);
        }

        var iter = TakeLastIterator(source, count);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Returns a sequence that skips the specified number of elements from the end of the source sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to process. Cannot be <see langword="null"/>.</param>
    /// <param name="count">The number of elements to skip from the end of the sequence. If <paramref name="count"/> is less than or equal
    /// to zero, the original sequence is returned.</param>
    /// <returns>A sequence that contains the elements of the source sequence except for the specified number of elements at the
    /// end.</returns>
    public static IAsyncPagedEnumerable<TSource> SkipLast<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (count <= 0) return source;

        var iter = SkipLastIterator(source, count);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Splits the elements of the source sequence into chunks of the specified size.
    /// </summary>
    /// <remarks>This method processes the source sequence lazily, meaning that chunks are generated on demand
    /// as the resulting sequence is enumerated. It is suitable for use with large or infinite sequences.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to be divided into chunks. Cannot be <see langword="null"/>.</param>
    /// <param name="size">The maximum size of each chunk. Must be greater than zero.</param>
    /// <returns>An asynchronous sequence of arrays, where each array contains up to <paramref name="size"/> elements from the
    /// source sequence. The last chunk may contain fewer elements if the total number of elements in the source
    /// sequence is not evenly divisible by <paramref name="size"/>.</returns>
    public static IAsyncPagedEnumerable<TSource[]> Chunk<TSource>(
        this IAsyncPagedEnumerable<TSource> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        var iter = ChunkIterator(source, size);
        return new AsyncPagedEnumerable<TSource[], TSource[]>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<TSource[], TSource[]>)(x => x));
    }

    /// <summary>
    /// Determines whether the asynchronous sequence contains any elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the asynchronous sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to check for elements. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the sequence
    /// contains at least one element; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> AnyAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        return await e.MoveNextAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether any element in the asynchronous sequence satisfies the specified condition.
    /// </summary>
    /// <remarks>The method enumerates the elements of the asynchronous sequence until the predicate returns
    /// <see langword="true"/> for an element or the sequence is fully enumerated.</remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source">The asynchronous sequence to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if any element
    /// satisfies the condition; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> AnyAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (predicate(item)) return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether any element in the asynchronous sequence satisfies the specified condition.
    /// </summary>
    /// <remarks>The method enumerates the source sequence asynchronously and stops as soon as an element
    /// satisfying the condition is found. If the sequence is empty, the method returns <see
    /// langword="false"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">An asynchronous sequence of elements to test.</param>
    /// <param name="predicate">A function to test each element for a condition. The function returns a <see cref="ValueTask{Boolean}"/> that
    /// evaluates to <see langword="true"/> if the condition is met; otherwise, <see langword="false"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if any element
    /// satisfies the condition specified by <paramref name="predicate"/>; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> AnyAwaitAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await predicate(item).ConfigureAwait(false)) return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether any element in the asynchronous sequence satisfies the specified predicate.
    /// </summary>
    /// <remarks>The method evaluates the elements of the sequence lazily and stops processing as soon as the
    /// predicate returns <see langword="true"/> for any element. If the sequence is empty, the method returns <see
    /// langword="false"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The function takes an element of type <typeparamref
    /// name="TSource"/> and a <see cref="CancellationToken"/> as input and returns a <see cref="ValueTask{Boolean}"/>
    /// indicating whether the condition is met. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if any element in
    /// the sequence satisfies the predicate; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> AnyAwaitWithCancellationAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await predicate(item, cancellationToken).ConfigureAwait(false)) return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether all elements in the asynchronous sequence satisfy the specified condition.
    /// </summary>
    /// <remarks>The evaluation stops as soon as an element that does not satisfy the condition is
    /// found.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if all elements in
    /// the sequence satisfy the condition; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> AllAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!predicate(item)) return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether all elements in the asynchronous sequence satisfy the specified condition.
    /// </summary>
    /// <remarks>The evaluation of the sequence is performed lazily and stops as soon as an element that does
    /// not satisfy the  condition is found. If the sequence is empty, the method returns <see
    /// langword="true"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the asynchronous sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The function should return a <see cref="ValueTask{Boolean}"/> 
    /// indicating whether the condition is satisfied. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if all elements  in
    /// the sequence satisfy the condition; otherwise, <see langword="false"/>. Returns <see langword="true"/> if  the
    /// sequence is empty.</returns>
    public static async Task<bool> AllAwaitAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await predicate(item).ConfigureAwait(false)) return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether all elements in the asynchronous sequence satisfy the specified predicate.
    /// </summary>
    /// <remarks>The evaluation of the sequence is performed lazily and stops as soon as an element that does
    /// not satisfy the  predicate is found. The method respects the <paramref name="cancellationToken"/> and will throw
    /// an  <see cref="OperationCanceledException"/> if cancellation is requested.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function that defines the condition to check for each element. The function takes an element of the sequence 
    /// and a <see cref="CancellationToken"/> as input and returns a <see cref="ValueTask{Boolean}"/> indicating whether
    /// the condition is satisfied. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if all elements in 
    /// the sequence satisfy the predicate; otherwise, <see langword="false"/>. If the sequence is empty, the result is 
    /// <see langword="true"/>.</returns>
    public static async Task<bool> AllAwaitWithCancellationAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await predicate(item, cancellationToken).ConfigureAwait(false)) return false;
        }
        return true;
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to retrieve the first element from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element of the sequence.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains no elements.</exception>
    public static async Task<TSource> FirstAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        if (await e.MoveNextAsync().ConfigureAwait(false))
            return e.Current;

        throw new InvalidOperationException("Sequence contains no elements.");
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to return the first element from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element of the sequence, 
    /// or <see langword="default"/> if the sequence contains no elements.</returns>
    public static async Task<TSource?> FirstOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        return await e.MoveNextAsync().ConfigureAwait(false) ? e.Current : default;
    }

    /// <summary>
    /// Asynchronously returns the first element in the sequence that satisfies the specified predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to search. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the asynchronous operation to complete. The
    /// default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element in the sequence
    /// that satisfies the predicate.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains no elements that satisfy the predicate.</exception>
    public static async Task<TSource> FirstAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item)) return item;
        }
        throw new InvalidOperationException("Sequence contains no matching element.");
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified condition,  or a default value
    /// if no such element is found.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to search. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element  in the sequence
    /// that satisfies the condition defined by <paramref name="predicate"/>, or the default  value of <typeparamref
    /// name="TSource"/> if no such element is found.</returns>
    public static async Task<TSource?> FirstOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item)) return item;
        }
        return default;
    }

    /// <summary>
    /// Asynchronously returns the single element of a sequence, or throws an exception if the sequence  contains no
    /// elements or more than one element.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to retrieve the single element from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the single element  of the sequence.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains no elements or more than one element.</exception>
    public static async Task<TSource> SingleAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        if (!await e.MoveNextAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Sequence contains no elements.");

        var result = e.Current;
        if (await e.MoveNextAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Sequence contains more than one element.");

        return result;
    }

    /// <summary>
    /// Asynchronously returns the single element of a sequence, or a default value if the sequence is empty.
    /// </summary>
    /// <remarks>This method enumerates the sequence asynchronously. If the sequence contains exactly one
    /// element, that element is returned.  If the sequence is empty, the default value for the type <typeparamref
    /// name="TSource"/> is returned. If the sequence contains  more than one element, an <see
    /// cref="InvalidOperationException"/> is thrown.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to return the single element from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the single element of the sequence, 
    /// or <see langword="default"/> if the sequence is empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains more than one element.</exception>
    public static async Task<TSource?> SingleOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e = source.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        if (!await e.MoveNextAsync().ConfigureAwait(false)) return default;

        var result = e.Current;
        if (await e.MoveNextAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Sequence contains more than one element.");

        return result;
    }

    /// <summary>
    /// Asynchronously returns the single element of a sequence that satisfies a specified condition,  or throws an
    /// exception if no such element exists or if more than one element satisfies the condition.
    /// </summary>
    /// <remarks>This method enumerates the sequence asynchronously and applies the <paramref
    /// name="predicate"/> to each element.  If exactly one element satisfies the condition, it is returned. If no
    /// elements satisfy the condition, or if more  than one element satisfies the condition, an <see
    /// cref="InvalidOperationException"/> is thrown.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to search. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the single element  of the sequence
    /// that satisfies the condition defined by <paramref name="predicate"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains no elements that satisfy the condition, or if more than one element satisfies
    /// the condition.</exception>
    public static async Task<TSource> SingleAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        bool found = false;
        TSource? result = default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                if (found) throw new InvalidOperationException("Sequence contains more than one matching element.");
                found = true;
                result = item;
            }
        }

        if (!found) throw new InvalidOperationException("Sequence contains no matching element.");
        return result!;
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a specified condition, or a default value
    /// if no such element exists. Throws an exception if more than one element satisfies the condition.
    /// </summary>
    /// <remarks>This method enumerates the sequence asynchronously and applies the <paramref
    /// name="predicate"/> to each element. If exactly one element satisfies the condition, it is returned. If no
    /// elements satisfy the condition, the default value of <typeparamref name="TSource"/> is returned. If more than
    /// one element satisfies the condition, an <see cref="InvalidOperationException"/> is thrown.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence to search. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the single element of the sequence
    /// that satisfies the condition, or <see langword="null"/> if no such element exists.</returns>
    /// <exception cref="InvalidOperationException">Thrown if more than one element in the sequence satisfies the condition.</exception>
    public static async Task<TSource?> SingleOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        bool found = false;
        TSource? result = default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                if (found) throw new InvalidOperationException("Sequence contains more than one matching element.");
                found = true;
                result = item;
            }
        }

        return result;
    }

    /// <summary>
    /// Asynchronously returns the last element of an asynchronous paged sequence.
    /// </summary>
    /// <remarks>This method enumerates the entire sequence to determine the last element. If the sequence is
    /// empty, an <see cref="InvalidOperationException"/> is thrown.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous paged sequence to retrieve the last element from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the last element of the sequence.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains no elements.</exception>
    public static async Task<TSource> LastAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        bool found = false;
        TSource? last = default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            found = true;
            last = item;
        }

        if (!found) throw new InvalidOperationException("Sequence contains no elements.");
        return last!;
    }

    /// <summary>
    /// Asynchronously returns the last element of a sequence, or a default value if the sequence contains no elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to return the last element from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the last element of the sequence, 
    /// or the default value of <typeparamref name="TSource"/> if the sequence is empty.</returns>
    public static async Task<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        TSource? last = default;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            last = item;
        }
        return last;
    }

    /// <summary>
    /// Determines whether the specified value exists in the asynchronous sequence.
    /// </summary>
    /// <remarks>This method enumerates the sequence asynchronously to determine whether the specified value
    /// exists. If the sequence contains the value, the enumeration stops early and the method returns <see
    /// langword="true"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to search. Cannot be <see langword="null"/>.</param>
    /// <param name="value">The value to locate in the sequence.</param>
    /// <param name="comparer">An optional equality comparer to use for comparing elements. If <see langword="null"/>, the default equality
    /// comparer for the type <typeparamref name="TSource"/> is used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete. The default value is
    /// <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the specified
    /// value is found in the sequence; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> ContainsAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        TSource value,
        IEqualityComparer<TSource>? comparer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        comparer ??= EqualityComparer<TSource>.Default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (comparer.Equals(item, value)) return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether two asynchronous sequences are equal by comparing their elements pairwise.
    /// </summary>
    /// <remarks>The method compares the elements of the two sequences in order. The sequences are considered
    /// equal if they have the same number of elements and each pair of corresponding elements is equal according to the
    /// specified or default equality comparer.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequences.</typeparam>
    /// <param name="first">The first asynchronous sequence to compare. Cannot be <see langword="null"/>.</param>
    /// <param name="second">The second asynchronous sequence to compare. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional equality comparer to use for comparing elements. If <see langword="null"/>, the default equality
    /// comparer for <typeparamref name="TSource"/> is used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation. The operation can be canceled by
    /// passing a canceled token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the two
    /// sequences are equal; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> SequenceEqualAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource>? comparer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        comparer ??= EqualityComparer<TSource>.Default;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e1 = first.GetAsyncEnumerator(cancellationToken);
        await using var e2 = second.GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var m1 = await e1.MoveNextAsync().ConfigureAwait(false);
            var m2 = await e2.MoveNextAsync().ConfigureAwait(false);

            if (!m1 || !m2) return m1 == m2;
            if (!comparer.Equals(e1.Current, e2.Current)) return false;
        }
    }

    /// <summary>
    /// Asynchronously counts the number of elements in the specified asynchronous sequence.
    /// </summary>
    /// <remarks>This method enumerates the entire sequence to determine the count. Use caution when working
    /// with large or infinite sequences, as this may result in high memory usage or an unbounded operation.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to count. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while counting the elements. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of elements in the
    /// sequence.</returns>
    public static async Task<int> CountAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        checked
        {
            int count = 0;
            await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Asynchronously counts the total number of elements in the specified asynchronous paged enumerable.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to count the elements of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while counting the elements. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of elements in the
    /// source enumerable.</returns>
    public static async Task<long> LongCountAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        long count = 0;
        await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Asynchronously retrieves the total count of items from a paginated data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the paginated data source.</typeparam>
    /// <param name="source">The asynchronous paginated data source to retrieve the total count from. Cannot be <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of items in the
    /// paginated data source.</returns>
    public static async Task<long> CountFromPaginationAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var p = await source.GetPaginationAsync().ConfigureAwait(false);
        return p.TotalCount;
    }

    /// <summary>
    /// Asynchronously enumerates all elements in the specified <see cref="IAsyncPagedEnumerable{TSource}"/>  and
    /// returns them as a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to convert to a list. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="List{T}"/>  with all
    /// elements from the source sequence.</returns>
    public static async Task<List<TSource>> ToListAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = new List<TSource>();
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Asynchronously converts an <see cref="IAsyncPagedEnumerable{TSource}"/> to an array.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to convert. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of elements from the source
    /// sequence.</returns>
    public static async Task<TSource[]> ToArrayAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        var list = await ToListAsync(source, cancellationToken).ConfigureAwait(false);
        return [.. list];
    }

    /// <summary>
    /// Orders the elements of the asynchronous paged sequence in ascending order according to a specified key.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for ordering the elements.</typeparam>
    /// <param name="source">The asynchronous paged sequence to order. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract the key for each element. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional comparer to use for comparing keys. If <see langword="null"/>, the default comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> whose elements are sorted in ascending order based on the
    /// specified key.</returns>
    public static IAsyncPagedEnumerable<TSource> OrderBy<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = OrderByIterator(source, keySelector, comparer ?? Comparer<TKey>.Default, descending: false);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Sorts the elements of the asynchronous paged sequence in descending order based on a specified key.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for sorting.</typeparam>
    /// <param name="source">The asynchronous paged sequence to sort. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract the key from each element. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional comparer to use for comparing keys. If <see langword="null"/>, the default comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>A new asynchronous paged sequence whose elements are sorted in descending order based on the specified key.</returns>
    public static IAsyncPagedEnumerable<TSource> OrderByDescending<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = OrderByIterator(source, keySelector, comparer ?? Comparer<TKey>.Default, descending: true);
        return new AsyncPagedEnumerable<TSource, TSource>(
            iter,
            ct => source.GetPaginationAsync(ct).AsValueTask(),
            (Func<TSource, TSource>?)null);
    }

    /// <summary>
    /// Groups the elements of a sequence according to a specified key selector function.
    /// </summary>
    /// <remarks>This method allows grouping elements in an asynchronous paged sequence based on a key. The
    /// grouping is performed lazily,  and the resulting groups are returned as an asynchronous paged enumerable. Each
    /// group contains a key and the elements  associated with that key.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
    /// <param name="source">The sequence of elements to group.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>An asynchronous paged enumerable of groups, where each group is represented as an <see cref="IGrouping{TKey,
    /// TSource}"/>  containing a collection of objects and their associated key.</returns>
    public static IAsyncPagedEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var iter = GroupByIterator(source, keySelector, static x => x, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<IGrouping<TKey, TSource>, IGrouping<TKey, TSource>>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<IGrouping<TKey, TSource>, IGrouping<TKey, TSource>>?)null);
    }

    /// <summary>
    /// Groups the elements of a sequence according to a specified key selector function and projects the elements for
    /// each group.
    /// </summary>
    /// <remarks>This method allows grouping elements in an asynchronous sequence into collections based on a
    /// key.  The grouping is performed lazily, and the resulting groups are returned as an asynchronous paged
    /// enumerable.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
    /// <typeparam name="TElement">The type of the elements in each group.</typeparam>
    /// <param name="source">The sequence to group.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="elementSelector">A function to map each source element to an element in the resulting group.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If null, the default equality comparer is used.</param>
    /// <returns>An asynchronous paged enumerable of groups, where each group is represented by an <see cref="IGrouping{TKey,
    /// TElement}"/>  containing a key and a collection of elements.</returns>
    public static IAsyncPagedEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);

        var iter = GroupByIterator(source, keySelector, elementSelector, comparer ?? EqualityComparer<TKey>.Default);
        return new AsyncPagedEnumerable<IGrouping<TKey, TElement>, IGrouping<TKey, TElement>>(
            iter,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            (Func<IGrouping<TKey, TElement>, IGrouping<TKey, TElement>>?)null);
    }

    /// <summary>
    /// Asynchronously returns the element with the minimum key value from a sequence of asynchronously paged elements.
    /// </summary>
    /// <remarks>This method enumerates the source sequence asynchronously and applies the <paramref
    /// name="keySelector"/> function to each element  to determine the key for comparison. If multiple elements have
    /// the same minimum key value, the first occurrence is returned.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">An asynchronous sequence of elements to evaluate.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">An optional comparer to use for comparing key values. If <see langword="null"/>, the default comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting the asynchronous operation. The default value is
    /// <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the element with the minimum key
    /// value,  or <see langword="null"/> if the source sequence is empty.</returns>
    public static async Task<TSource?> MinByAsync<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        comparer ??= Comparer<TKey>.Default;

        bool found = false;
        TSource? best = default;
        TKey? bestKey = default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = keySelector(item);
            if (!found || comparer.Compare(key, bestKey!) < 0)
            {
                found = true;
                best = item;
                bestKey = key;
            }
        }
        return best;
    }

    /// <summary>
    /// Asynchronously returns the element with the maximum key value from a sequence of asynchronously paged elements.
    /// </summary>
    /// <remarks>This method evaluates the sequence lazily and asynchronously. It iterates through the source
    /// sequence only once and stops as soon as the maximum element is determined. If the source sequence is empty, the
    /// method returns <see langword="null"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">The sequence of asynchronously paged elements to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract the key from each element. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional comparer to use for comparing keys. If <see langword="null"/>, the default comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the element with the maximum key value, or
    /// <see langword="null"/> if the source sequence is empty.</returns>
    public static async Task<TSource?> MaxByAsync<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        comparer ??= Comparer<TKey>.Default;

        bool found = false;
        TSource? best = default;
        TKey? bestKey = default;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = keySelector(item);
            if (!found || comparer.Compare(key, bestKey!) > 0)
            {
                found = true;
                best = item;
                bestKey = key;
            }
        }
        return best;
    }

    /// <summary>
    /// Asynchronously aggregates the elements of an <see cref="IAsyncPagedEnumerable{TSource}"/> sequence  using a
    /// specified seed value and an accumulator function.
    /// </summary>
    /// <remarks>This method processes the elements of the sequence asynchronously and in a streaming fashion,
    /// making it suitable for large or infinite sequences. The aggregation stops if the sequence is exhausted  or if
    /// the operation is canceled via the <paramref name="cancellationToken"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="source">The asynchronous sequence to aggregate. Cannot be <see langword="null"/>.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="func">A function to apply to each element in the sequence. The function takes the current accumulator value  and the
    /// current element as arguments, and returns the updated accumulator value. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the aggregation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous aggregation operation. The task's result is the final accumulator value.</returns>
    public static async Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(
        this IAsyncPagedEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> func,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(func);

        var acc = seed;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            acc = func(acc, item);
        }
        return acc;
    }

    /// <summary>
    /// Asynchronously aggregates the elements of an <see cref="IAsyncPagedEnumerable{T}"/> sequence using a specified
    /// seed value and an asynchronous accumulator function.
    /// </summary>
    /// <remarks>The <paramref name="func"/> is invoked for each element in the <paramref name="source"/>
    /// sequence, passing the current accumulator value and the element as arguments. The result of the function is used
    /// as the accumulator value for the next iteration.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="source">The asynchronous sequence to aggregate. Cannot be <see langword="null"/>.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="func">An asynchronous function to apply to each element in the sequence and the current accumulator value. Cannot be
    /// <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous aggregation operation. The task's result is the final accumulator value
    /// after processing all elements in the sequence.</returns>
    public static async Task<TAccumulate> AggregateAwaitAsync<TSource, TAccumulate>(
        this IAsyncPagedEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, ValueTask<TAccumulate>> func,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(func);

        var acc = seed;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            acc = await func(acc, item).ConfigureAwait(false);
        }
        return acc;
    }

    /// <summary>
    /// Asynchronously applies an accumulator function over a sequence of elements, starting with the specified seed
    /// value,  and transforms the final accumulated result using a result selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TAccumulate">The type of the intermediate accumulated value.</typeparam>
    /// <typeparam name="TResult">The type of the final result value.</typeparam>
    /// <param name="source">The asynchronous sequence of elements to aggregate. Cannot be <see langword="null"/>.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="func">A function to apply to each element in the sequence, combining the current accumulated value and the element  to
    /// produce a new accumulated value. Cannot be <see langword="null"/>.</param>
    /// <param name="resultSelector">A function to transform the final accumulated value into the result value. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the aggregation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the final result value produced  by
    /// applying the <paramref name="resultSelector"/> to the accumulated value.</returns>
    public static async Task<TResult> AggregateAsync<TSource, TAccumulate, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> func,
        Func<TAccumulate, TResult> resultSelector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var acc = await AggregateAsync(source, seed, func, cancellationToken).ConfigureAwait(false);
        return resultSelector(acc);
    }

    /// <summary>
    /// Asynchronously retrieves the element at the specified zero-based index from a sequence of asynchronously paged
    /// elements.
    /// </summary>
    /// <remarks>If the specified index is greater than or equal to the number of elements in the sequence, an
    /// <see cref="InvalidOperationException"/> is thrown.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of asynchronously paged elements to retrieve the element from.</param>
    /// <param name="index">The zero-based index of the element to retrieve. Must be non-negative.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the element at the specified index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="index"/> is out of range of the sequence.</exception>
    public static async Task<TSource> ElementAtAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        int index,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var i = 0;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (i++ == index) return item;
        }
        throw new InvalidOperationException("Index was out of range.");
    }

    /// <summary>
    /// Asynchronously retrieves the element at the specified index in a sequence, or a default value if the index is
    /// out of range.
    /// </summary>
    /// <remarks>This method enumerates the sequence until the specified index is reached or the sequence
    /// ends.  If the index is out of range, the method returns the default value for <typeparamref
    /// name="TSource"/>.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to retrieve the element from. Cannot be <see langword="null"/>.</param>
    /// <param name="index">The zero-based index of the element to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the element at the specified index, 
    /// or the default value of <typeparamref name="TSource"/> if the index is out of range.</returns>
    public static async Task<TSource?> ElementAtOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        int index,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (index < 0) return default;

        var i = 0;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (i++ == index) return item;
        }
        return default;
    }

    /// <summary>
    /// Asynchronously returns the last element of a sequence that satisfies a specified condition.
    /// </summary>
    /// <remarks>This method enumerates the sequence asynchronously and applies the <paramref
    /// name="predicate"/>  to each element. If no matching element is found, an <see cref="InvalidOperationException"/>
    /// is thrown.</remarks>
    /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to search.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the last element in the sequence
    /// that satisfies the condition specified by <paramref name="predicate"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no element in the sequence satisfies the condition specified by <paramref name="predicate"/>.</exception>
    public static async Task<TSource> LastAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        bool found = false;
        TSource? last = default;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item)) { found = true; last = item; }
        }
        if (!found) throw new InvalidOperationException("Sequence contains no matching element.");
        return last!;
    }

    /// <summary>
    /// Asynchronously returns the last element of a sequence that satisfies a specified condition,  or a default value
    /// if no such element is found.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence to search. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the asynchronous operation to complete.  This
    /// parameter is optional and defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the last element  in the sequence
    /// that satisfies the condition specified by <paramref name="predicate"/>,  or <see langword="default"/> if no such
    /// element is found.</returns>
    public static async Task<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        TSource? last = default;
        bool found = false;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item)) { found = true; last = item; }
        }
        return found ? last : default;
    }

    /// <summary>
    /// Asynchronously converts an <see cref="IAsyncPagedEnumerable{TSource}"/> to a <see cref="Dictionary{TKey,
    /// TSource}"/>  using the specified key selector function.
    /// </summary>
    /// <remarks>This method processes the elements of the asynchronous sequence in a streaming manner, adding
    /// each element to the dictionary  as it is retrieved. The operation completes when all elements have been
    /// processed or if the operation is canceled.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary. Must be non-nullable.</typeparam>
    /// <param name="source">The asynchronous sequence to convert. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract a key from each element in the sequence. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional equality comparer to use for comparing keys. If <see langword="null"/>, the default equality
    /// comparer for <typeparamref name="TKey"/> is used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while performing the operation. Defaults to <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Dictionary{TKey,
    /// TSource}"/>  where each key is the result of applying <paramref name="keySelector"/> to an element in the source
    /// sequence,  and each value is the corresponding element.</returns>
    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var dict = new Dictionary<TKey, TSource>(comparer);
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            dict.Add(keySelector(item), item);
        }
        return dict;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IAsyncPagedEnumerable{T}"/>
    /// by applying the specified key and element selector functions.
    /// </summary>
    /// <remarks>This method processes the elements of the source sequence asynchronously and lazily,
    /// consuming elements as they are enumerated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary. Keys must be non-null.</typeparam>
    /// <typeparam name="TElement">The type of the values in the resulting dictionary.</typeparam>
    /// <param name="source">The asynchronous sequence to convert into a dictionary. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract a key from each element. Cannot be <see langword="null"/>.</param>
    /// <param name="elementSelector">A function to produce a value for each element. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting the asynchronous operation. Defaults to <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Dictionary{TKey,
    /// TValue}"/> where each key is the result of applying <paramref name="keySelector"/> to an element of the source
    /// sequence, and each value is the result of applying <paramref name="elementSelector"/> to the same element.</returns>
    public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);

        var dict = new Dictionary<TKey, TElement>(comparer);
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            dict.Add(keySelector(item), elementSelector(item));
        }
        return dict;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="HashSet{T}"/> from an <see cref="IAsyncPagedEnumerable{T}"/>.
    /// </summary>
    /// <remarks>This method enumerates the source sequence asynchronously and adds each element to the
    /// resulting <see cref="HashSet{T}"/>. Duplicate elements are ignored based on the specified or default equality
    /// comparer.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous paged enumerable to convert to a <see cref="HashSet{T}"/>. Cannot be <see langword="null"/>.</param>
    /// <param name="comparer">An optional equality comparer to use for comparing elements. If <see langword="null"/>, the default equality
    /// comparer for <typeparamref name="TSource"/> is used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> 
    /// containing all elements from the source sequence.</returns>
    public static async Task<HashSet<TSource>> ToHashSetAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        IEqualityComparer<TSource>? comparer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var set = new HashSet<TSource>(comparer);
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            set.Add(item);
        }
        return set;
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
#pragma warning disable IDE0060 // Remove unused parameter
        [EnumeratorCancellation] CancellationToken ct = default)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    private static async IAsyncEnumerable<TSource> SliceIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        int skip,
        int take,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (take <= 0) yield break;

        var skipped = 0;
        var remaining = take;

        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();

            if (skipped < skip)
            {
                skipped++;
                continue;
            }

            yield return item;
            if (--remaining == 0) yield break;
        }
    }

    private static async IAsyncEnumerable<TResult> OfTypeIterator<TSource, TResult>(
    IAsyncEnumerable<TSource> source,
    [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (item is TResult matched) yield return matched;
        }
    }

    private static async IAsyncEnumerable<TSource> TakeWhileIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (!predicate(item)) yield break;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> TakeWhileIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (!await predicate(item).ConfigureAwait(false)) yield break;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> TakeWhileIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (!await predicate(item, ct).ConfigureAwait(false)) yield break;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> SkipWhileIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var skipping = true;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (skipping && predicate(item)) continue;
            skipping = false;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> SkipWhileIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var skipping = true;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (skipping && await predicate(item).ConfigureAwait(false)) continue;
            skipping = false;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> SkipWhileIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var skipping = true;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (skipping && await predicate(item, ct).ConfigureAwait(false)) continue;
            skipping = false;
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> AppendIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        TSource element,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }
        yield return element;
    }

    private static async IAsyncEnumerable<TSource> PrependIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        TSource element,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        yield return element;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TResult> SelectIndexIterator<TSource, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, int, TResult> selector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var index = -1;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return selector(item, checked(++index));
        }
    }

    private static async IAsyncEnumerable<TResult> SelectIndexIterator<TSource, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, int, ValueTask<TResult>> selector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var index = -1;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return await selector(item, checked(++index)).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<TResult> SelectIndexIterator<TSource, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, int, CancellationToken, ValueTask<TResult>> selector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var index = -1;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return await selector(item, checked(++index), ct).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<TSource> WhereIndexIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, int, bool> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var index = -1;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (predicate(item, checked(++index))) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> WhereIndexIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, int, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var index = -1;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (await predicate(item, checked(++index)).ConfigureAwait(false)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> WhereIndexIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, int, CancellationToken, ValueTask<bool>> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var index = -1;
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (await predicate(item, checked(++index), ct).ConfigureAwait(false)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> ConcatIterator<TSource>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }

        await foreach (var item in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> DistinctIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        IEqualityComparer<TSource> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var set = new HashSet<TSource>(comparer);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (set.Add(item)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> DefaultIfEmptyIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        TSource defaultValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e = source.GetAsyncEnumerator(ct);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
        if (!await e.MoveNextAsync().ConfigureAwait(false))
        {
            yield return defaultValue!;
            yield break;
        }

        do
        {
            ct.ThrowIfCancellationRequested();
            yield return e.Current;
        }
        while (await e.MoveNextAsync().ConfigureAwait(false));
    }

    private static async IAsyncEnumerable<TResult> SelectManyIterator<TSource, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, IEnumerable<TResult>> collectionSelector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            foreach (var inner in collectionSelector(item))
            {
                yield return inner;
            }
        }
    }

    private static async IAsyncEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, IEnumerable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            foreach (var inner in collectionSelector(item))
            {
                yield return resultSelector(item, inner);
            }
        }
    }

    private static async IAsyncEnumerable<TResult> SelectManyAsyncIterator<TSource, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, IAsyncEnumerable<TResult>> collectionSelector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            await foreach (var inner in collectionSelector(item).WithCancellation(ct).ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();
                yield return inner;
            }
        }
    }

    private static async IAsyncEnumerable<TResult> SelectManyAsyncIterator<TSource, TCollection, TResult>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            await foreach (var inner in collectionSelector(item).WithCancellation(ct).ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();
                yield return resultSelector(item, inner);
            }
        }
    }

    private static async IAsyncEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(
        IAsyncEnumerable<TFirst> first,
        IAsyncEnumerable<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e1 = first.GetAsyncEnumerator(ct);
        await using var e2 = second.GetAsyncEnumerator(ct);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var m1 = await e1.MoveNextAsync().ConfigureAwait(false);
            var m2 = await e2.MoveNextAsync().ConfigureAwait(false);
            if (!m1 || !m2) yield break;
            yield return resultSelector(e1.Current, e2.Current);
        }
    }

    private static async IAsyncEnumerable<TResult> ZipAsyncIterator<TFirst, TSecond, TResult>(
        IAsyncEnumerable<TFirst> first,
        IAsyncEnumerable<TSecond> second,
        Func<TFirst, TSecond, ValueTask<TResult>> resultSelector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var e1 = first.GetAsyncEnumerator(ct);
        await using var e2 = second.GetAsyncEnumerator(ct);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var m1 = await e1.MoveNextAsync().ConfigureAwait(false);
            var m2 = await e2.MoveNextAsync().ConfigureAwait(false);
            if (!m1 || !m2) yield break;
            yield return await resultSelector(e1.Current, e2.Current).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<TSource> UnionIterator<TSource>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var set = new HashSet<TSource>(comparer);

        await foreach (var item in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (set.Add(item)) yield return item;
        }

        await foreach (var item in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (set.Add(item)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> IntersectIterator<TSource>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var set = new HashSet<TSource>(comparer);
        await foreach (var item in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            set.Add(item);
        }

        var yielded = new HashSet<TSource>(comparer);
        await foreach (var item in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (set.Contains(item) && yielded.Add(item)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> ExceptIterator<TSource>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var exclude = new HashSet<TSource>(comparer);
        await foreach (var s in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            exclude.Add(s);
        }

        var yielded = new HashSet<TSource>(comparer);
        await foreach (var f in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (!exclude.Contains(f) && yielded.Add(f)) yield return f;
        }
    }

    private static async IAsyncEnumerable<TSource> DistinctByIterator<TSource, TKey>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var keys = new HashSet<TKey>(comparer);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (keys.Add(keySelector(item))) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> UnionByIterator<TSource, TKey>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var keys = new HashSet<TKey>(comparer);

        await foreach (var item in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (keys.Add(keySelector(item))) yield return item;
        }

        await foreach (var item in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (keys.Add(keySelector(item))) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> IntersectByIterator<TSource, TKey>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var rightKeys = new HashSet<TKey>(comparer);
        await foreach (var item in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            rightKeys.Add(keySelector(item));
        }

        var yieldedKeys = new HashSet<TKey>(comparer);
        await foreach (var item in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = keySelector(item);
            if (rightKeys.Contains(key) && yieldedKeys.Add(key)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TSource> ExceptByIterator<TSource, TKey>(
        IAsyncEnumerable<TSource> first,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var rightKeys = new HashSet<TKey>(comparer);
        await foreach (var item in second.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            rightKeys.Add(keySelector(item));
        }

        var yieldedKeys = new HashSet<TKey>(comparer);
        await foreach (var item in first.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = keySelector(item);
            if (!rightKeys.Contains(key) && yieldedKeys.Add(key)) yield return item;
        }
    }

    private static async IAsyncEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(
        IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TKey : notnull
    {
        var lookup = new Dictionary<TKey, List<TInner>>(comparer);
        await foreach (var i in inner.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = innerKeySelector(i)!;
            if (!lookup.TryGetValue(key, out var list))
            {
                list = [];
                lookup[key] = list;
            }
            list.Add(i);
        }

        await foreach (var o in outer.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = outerKeySelector(o)!;
            if (lookup.TryGetValue(key, out var matches))
            {
                foreach (var i in matches)
                {
                    yield return resultSelector(o, i);
                }
            }
        }
    }

    private static async IAsyncEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(
        IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TKey : notnull
    {
        var lookup = new Dictionary<TKey, List<TInner>>(comparer);
        await foreach (var i in inner.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = innerKeySelector(i)!;
            if (!lookup.TryGetValue(key, out var list))
            {
                list = [];
                lookup[key] = list;
            }
            list.Add(i);
        }

        await foreach (var o in outer.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = outerKeySelector(o)!;
            lookup.TryGetValue(key, out var matches);
            yield return resultSelector(o, matches ?? (IEnumerable<TInner>)[]);
        }
    }

    private static async IAsyncEnumerable<TSource> ReverseIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var buffer = new List<TSource>();
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            buffer.Add(item);
        }
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            ct.ThrowIfCancellationRequested();
            yield return buffer[i];
        }
    }

    private static async IAsyncEnumerable<TSource> TakeLastIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var queue = new Queue<TSource>(count);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (queue.Count == count) queue.Dequeue();
            queue.Enqueue(item);
        }
        while (queue.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            yield return queue.Dequeue();
        }
    }

    private static async IAsyncEnumerable<TSource> SkipLastIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        int count,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var queue = new Queue<TSource>(count + 1);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            queue.Enqueue(item);
            if (queue.Count > count)
            {
                yield return queue.Dequeue();
            }
        }
    }

    private static async IAsyncEnumerable<TSource[]> ChunkIterator<TSource>(
        IAsyncEnumerable<TSource> source,
        int size,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chunk = new List<TSource>(size);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            chunk.Add(item);
            if (chunk.Count == size)
            {
                yield return chunk.ToArray();
                chunk.Clear();
            }
        }
        if (chunk.Count > 0) yield return chunk.ToArray();
    }

    private static async IAsyncEnumerable<TSource> OrderByIterator<TSource, TKey>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey> comparer,
        bool descending,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var buffer = new List<(TSource Item, TKey Key)>();
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            buffer.Add((item, keySelector(item)));
        }

        buffer.Sort((a, b) =>
        {
            var cmp = comparer.Compare(a.Key, b.Key);
            return descending ? -cmp : cmp;
        });

        foreach (var (item, _) in buffer)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<IGrouping<TKey, TElement>> GroupByIterator<TSource, TKey, TElement>(
        IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey> comparer,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TKey : notnull
    {
        var map = new Dictionary<TKey, List<TElement>>(comparer);
        await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            var key = keySelector(item)!;
            if (!map.TryGetValue(key, out var list))
            {
                list = [];
                map[key] = list;
            }
            list.Add(elementSelector(item));
        }

        foreach (var kvp in map)
        {
            ct.ThrowIfCancellationRequested();
            yield return new Grouping<TKey, TElement>(kvp.Key, kvp.Value);
        }
    }

    private sealed class Grouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements) : IGrouping<TKey, TElement>
    {
        public TKey Key { get; } = key;
        private readonly IEnumerable<TElement> _elements = elements;

        public IEnumerator<TElement> GetEnumerator() => _elements.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1849 // Call async methods when in an async method
    private static ValueTask<T> AsValueTask<T>(this Task<T> task) =>
        task.IsCompletedSuccessfully ? new ValueTask<T>(task.Result) : new ValueTask<T>(task);
#pragma warning restore CA1849 // Call async methods when in an async method
}