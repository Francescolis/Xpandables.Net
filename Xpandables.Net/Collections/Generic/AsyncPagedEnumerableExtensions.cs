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
            ct => source.GetPaginationAsync().AsValueTask(),
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
            ct => source.GetPaginationAsync().AsValueTask(),
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
            ct => source.GetPaginationAsync().AsValueTask(),
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
            ct => source.GetPaginationAsync().AsValueTask(),
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
            ct => source.GetPaginationAsync().AsValueTask(),
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
                async _ =>
                {
                    var p = await source.GetPaginationAsync().ConfigureAwait(false);
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
            async _ =>
            {
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
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
            async _ =>
            {
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
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
            ct => source.GetPaginationAsync().AsValueTask(),
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
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
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

        var pagination = await source.GetPaginationAsync().ConfigureAwait(false);

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
            async _ =>
            {
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
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
            async _ =>
            {
                var p = await source.GetPaginationAsync().ConfigureAwait(false);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1849 // Call async methods when in an async method
    private static ValueTask<T> AsValueTask<T>(this Task<T> task) =>
        task.IsCompletedSuccessfully ? new ValueTask<T>(task.Result) : new ValueTask<T>(task);
#pragma warning restore CA1849 // Call async methods when in an async method
}