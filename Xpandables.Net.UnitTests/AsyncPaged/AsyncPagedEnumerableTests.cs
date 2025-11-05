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

using FluentAssertions;
using Xpandables.Net.AsyncPaged;

namespace Xpandables.Net.UnitTests.AsyncPaged;

/// <summary>
/// Unit tests for <see cref="AsyncPagedEnumerable{T}"/>.
/// </summary>
public sealed class AsyncPagedEnumerableTests
{
    [Fact]
    public async Task Constructor_WithAsyncEnumerableAndFactory_ShouldCreateInstance()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(Pagination.Create(10, 1, null, 100));

        // Act
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Assert
        pagedEnumerable.Should().NotBeNull();
        ((IAsyncPagedEnumerable<int>)pagedEnumerable).Type.Should().Be(typeof(int));
    }

    [Fact]
    public void Constructor_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IAsyncEnumerable<int> nullSource = null!;
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(Pagination.Create(10, 1));

        // Act
        Action act = () => new AsyncPagedEnumerable<int>(nullSource, factory);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);

        // Act
        Action act = () => new AsyncPagedEnumerable<int>(source, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("paginationFactory");
    }

    [Fact]
    public void Constructor_WithQueryable_ShouldCreateInstance()
    {
        // Arrange
        var queryable = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var pagedEnumerable = new AsyncPagedEnumerable<int>(queryable);

        // Assert
        pagedEnumerable.Should().NotBeNull();
        ((IAsyncPagedEnumerable<int>)pagedEnumerable).Type.Should().Be(typeof(int));
    }

    [Fact]
    public void Constructor_WithNullQueryable_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<int> nullQueryable = null!;

        // Act
        Action act = () => new AsyncPagedEnumerable<int>(nullQueryable);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("query");
    }

    [Fact]
    public async Task GetAsyncEnumerator_ShouldEnumerateAllItems()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(Pagination.Create(10, 1, null, 5));
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);
        var results = new List<int>();

        // Act
        await foreach (var item in pagedEnumerable)
        {
            results.Add(item);
        }

        // Assert
        results.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task Pagination_BeforeComputation_ShouldReturnEmpty()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Func<CancellationToken, ValueTask<Pagination>> factory = async _ =>
        {
            await Task.Delay(50); // Simulate async work
            return Pagination.Create(10, 1, null, 3);
        };
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        Pagination pagination = pagedEnumerable.Pagination;

        // Assert
        pagination.Should().Be(Pagination.Empty);
    }

    [Fact]
    public async Task GetPaginationAsync_ShouldComputeAndReturnPagination()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination expectedPagination = Pagination.Create(10, 1, "token", 100);
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(expectedPagination);
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.Should().Be(expectedPagination);
    }

    [Fact]
    public async Task GetPaginationAsync_CalledMultipleTimes_ShouldReturnSamePagination()
    {
        // Arrange
        int factoryCallCount = 0;
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination expectedPagination = Pagination.Create(10, 1, "token", 100);
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
        {
            factoryCallCount++;
            return new ValueTask<Pagination>(expectedPagination);
        };
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        Pagination pagination1 = await pagedEnumerable.GetPaginationAsync();
        Pagination pagination2 = await pagedEnumerable.GetPaginationAsync();
        Pagination pagination3 = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination1.Should().Be(expectedPagination);
        pagination2.Should().Be(expectedPagination);
        pagination3.Should().Be(expectedPagination);
        factoryCallCount.Should().Be(1); // Factory should be called only once
    }

    [Fact]
    public async Task GetPaginationAsync_WithConcurrentCalls_ShouldComputeOnlyOnce()
    {
        // Arrange
        int factoryCallCount = 0;
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination expectedPagination = Pagination.Create(10, 1, "token", 100);
        Func<CancellationToken, ValueTask<Pagination>> factory = async _ =>
        {
            Interlocked.Increment(ref factoryCallCount);
            await Task.Delay(50); // Simulate async work
            return expectedPagination;
        };
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => pagedEnumerable.GetPaginationAsync())
            .ToArray();
        Pagination[] results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(p => p.Should().Be(expectedPagination));
        factoryCallCount.Should().Be(1); // Factory should be called only once despite concurrent access
    }

    [Fact]
    public async Task GetPaginationAsync_WhenFactoryThrows_ShouldPropagateException()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var expectedException = new InvalidOperationException("Test exception");
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            throw expectedException;
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        Func<Task> act = async () => await pagedEnumerable.GetPaginationAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task GetPaginationAsync_AfterFactoryThrows_ShouldRethrowSameException()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var expectedException = new InvalidOperationException("Test exception");
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            throw expectedException;
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act & Assert
        Func<Task> act1 = async () => await pagedEnumerable.GetPaginationAsync();
        await act1.Should().ThrowAsync<InvalidOperationException>();

        Func<Task> act2 = async () => await pagedEnumerable.GetPaginationAsync();
        await act2.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Pagination_AfterComputation_ShouldReturnComputedValue()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination expectedPagination = Pagination.Create(10, 1, "token", 100);
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(expectedPagination);
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        await pagedEnumerable.GetPaginationAsync(); // Trigger computation
        Pagination pagination = pagedEnumerable.Pagination;

        // Assert
        pagination.Should().Be(expectedPagination);
    }

    [Fact]
    public async Task GetPaginationAsync_WithQueryable_ShouldExtractPaginationFromQuery()
    {
        // Arrange
        var queryable = Enumerable.Range(1, 100).AsQueryable()
            .Skip(20)
            .Take(10);
        var pagedEnumerable = new AsyncPagedEnumerable<int>(queryable);

        // Act
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(10);
        pagination.CurrentPage.Should().Be(3); // (20 / 10) + 1
        pagination.TotalCount.Should().Be(100);
    }

    [Fact]
    public async Task GetPaginationAsync_WithQueryableAndCustomTotalFactory_ShouldUseCustomTotal()
    {
        // Arrange
        var queryable = Enumerable.Range(1, 50).AsQueryable()
            .Skip(10)
            .Take(5);
        Func<CancellationToken, ValueTask<long>> totalFactory = _ =>
            new ValueTask<long>(200); // Custom total
        var pagedEnumerable = new AsyncPagedEnumerable<int>(queryable, totalFactory);

        // Act
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(5);
        pagination.CurrentPage.Should().Be(3); // (10 / 5) + 1
        pagination.TotalCount.Should().Be(200); // Custom total
    }

    [Fact]
    public async Task GetPaginationAsync_WithQueryableWithoutSkipTake_ShouldReturnDefaultPagination()
    {
        // Arrange
        var queryable = Enumerable.Range(1, 50).AsQueryable();
        var pagedEnumerable = new AsyncPagedEnumerable<int>(queryable);

        // Act
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(0);
        pagination.CurrentPage.Should().Be(0);
        pagination.TotalCount.Should().Be(50);
    }

    [Fact]
    public async Task GetPaginationAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var source = CreateAsyncEnumerable(1, 2, 3);
        Func<CancellationToken, ValueTask<Pagination>> factory = async ct =>
        {
            await Task.Delay(200, ct);
            return Pagination.Create(10, 1);
        };
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        cts.CancelAfter(50);
        Func<Task> act = async () => await pagedEnumerable.GetPaginationAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAsyncEnumerator_WithCancellation_ShouldPassCancellationToSource()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var source = CreateCancellableAsyncEnumerable(10, cts.Token);
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(Pagination.Create(10, 1));
        var pagedEnumerable = new AsyncPagedEnumerable<int>(source, factory);

        // Act
        var enumerator = pagedEnumerable.GetAsyncEnumerator(cts.Token);
        await enumerator.MoveNextAsync(); // First item
        cts.Cancel();

        Func<Task> act = async () => await enumerator.MoveNextAsync();

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Type_ShouldReturnCorrectElementType()
    {
        // Arrange
        var source = CreateAsyncEnumerable("a", "b", "c");
        Func<CancellationToken, ValueTask<Pagination>> factory = _ =>
            new ValueTask<Pagination>(Pagination.Create(10, 1));
        var pagedEnumerable = new AsyncPagedEnumerable<string>(source, factory);

        // Act & Assert
        ((IAsyncPagedEnumerable<string>)pagedEnumerable).Type.Should().Be(typeof(string));
    }

    [Fact]
    public async Task GetPaginationAsync_WithQueryableAndTotalCountExceedingIntMax_ShouldClampToIntMax()
    {
        // Arrange
        var queryable = Enumerable.Range(1, 10).AsQueryable().Skip(5).Take(5);
        Func<CancellationToken, ValueTask<long>> totalFactory = _ =>
            new ValueTask<long>((long)int.MaxValue + 100); // Exceeds int.MaxValue
        var pagedEnumerable = new AsyncPagedEnumerable<int>(queryable, totalFactory);

        // Act
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.TotalCount.Should().Be(int.MaxValue);
    }

    // Helper methods
    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<int> CreateCancellableAsyncEnumerable(
        int count,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(50, cancellationToken);
            yield return i;
        }
    }
}
