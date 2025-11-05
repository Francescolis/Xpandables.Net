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
/// Unit tests for <see cref="AsyncPagedEnumerator{T}"/>.
/// </summary>
public sealed class AsyncPagedEnumeratorTests
{
    [Fact]
    public async Task MoveNextAsync_WithEmptyEnumerator_ShouldReturnFalse()
    {
        // Arrange
        AsyncPagedEnumerator<int> enumerator = AsyncPagedEnumerator.Empty<int>();

        // Act
        bool result = await enumerator.MoveNextAsync();

        // Assert
        result.Should().BeFalse();
        enumerator.Current.Should().Be(default);
    }

    [Fact]
    public async Task MoveNextAsync_WithSourceEnumerator_ShouldEnumerateAllItems()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator());
        var results = new List<int>();

        // Act
        while (await enumerator.MoveNextAsync())
        {
            results.Add(enumerator.Current);
        }

        // Assert
        results.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task Pagination_WithNoneStrategy_ShouldNotUpdatePagination()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination initialPagination = Pagination.Create(10, 1, null, 100);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator(), initialPagination);

        // Act
        enumerator.WithStrategy(PaginationStrategy.None);
        while (await enumerator.MoveNextAsync())
        {
            // Enumerate through all items
        }

        // Assert
        enumerator.Pagination.Should().Be(initialPagination);
    }

    [Fact]
    public async Task Pagination_WithPerItemStrategy_ShouldUpdateCurrentPagePerItem()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3, 4);
        Pagination initialPagination = Pagination.Create(10, 0, null, 100);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator(), initialPagination)
            .WithPerItemStrategy();

        // Act & Assert
        await enumerator.MoveNextAsync();
        enumerator.Pagination.CurrentPage.Should().Be(1);

        await enumerator.MoveNextAsync();
        enumerator.Pagination.CurrentPage.Should().Be(2);

        await enumerator.MoveNextAsync();
        enumerator.Pagination.CurrentPage.Should().Be(3);

        await enumerator.MoveNextAsync();
        enumerator.Pagination.CurrentPage.Should().Be(4);
    }

    [Fact]
    public async Task Pagination_WithPerItemStrategy_ShouldFinalizeTotalCountAtEnd()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination initialPagination = Pagination.Create(10, 0); // No total count
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator(), initialPagination)
            .WithPerItemStrategy();

        // Act
        while (await enumerator.MoveNextAsync())
        {
            // Enumerate through all items
        }

        // Assert
        enumerator.Pagination.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Pagination_WithPerPageStrategy_ShouldUpdateCurrentPageBasedOnPageSize()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
        Pagination initialPagination = Pagination.Create(3, 0, null, 100); // Page size 3
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator(), initialPagination)
            .WithPerPageStrategy();

        // Act & Assert
        await enumerator.MoveNextAsync(); // Item 1
        enumerator.Pagination.CurrentPage.Should().Be(1);

        await enumerator.MoveNextAsync(); // Item 2
        enumerator.Pagination.CurrentPage.Should().Be(1);

        await enumerator.MoveNextAsync(); // Item 3
        enumerator.Pagination.CurrentPage.Should().Be(1);

        await enumerator.MoveNextAsync(); // Item 4
        enumerator.Pagination.CurrentPage.Should().Be(2);

        await enumerator.MoveNextAsync(); // Item 5
        enumerator.Pagination.CurrentPage.Should().Be(2);

        await enumerator.MoveNextAsync(); // Item 6
        enumerator.Pagination.CurrentPage.Should().Be(2);

        await enumerator.MoveNextAsync(); // Item 7
        enumerator.Pagination.CurrentPage.Should().Be(3);
    }

    [Fact]
    public async Task WithStrategy_ShouldAllowFluentConfiguration()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator());

        // Act
        var result = enumerator.WithStrategy(PaginationStrategy.PerPage);

        // Assert
        result.Should().BeSameAs(enumerator);
        enumerator.Strategy.Should().Be(PaginationStrategy.PerPage);
    }

    [Fact]
    public void WithPerPageStrategy_ShouldSetPerPageStrategy()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator());

        // Act
        enumerator.WithPerPageStrategy();

        // Assert
        enumerator.Strategy.Should().Be(PaginationStrategy.PerPage);
    }

    [Fact]
    public void WithPerItemStrategy_ShouldSetPerItemStrategy()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator());

        // Act
        enumerator.WithPerItemStrategy();

        // Assert
        enumerator.Strategy.Should().Be(PaginationStrategy.PerItem);
    }

    [Fact]
    public void WithNoStrategy_ShouldSetNoneStrategy()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator())
            .WithPerPageStrategy(); // Set to non-None first

        // Act
        enumerator.WithNoStrategy();

        // Assert
        enumerator.Strategy.Should().Be(PaginationStrategy.None);
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeSourceEnumerator()
    {
        // Arrange
        var disposable = new DisposableAsyncEnumerator<int>(new[] { 1, 2, 3 });
        var enumerator = AsyncPagedEnumerator.Create(disposable);

        // Act
        await enumerator.DisposeAsync();

        // Assert
        disposable.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_CalledMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var disposable = new DisposableAsyncEnumerator<int>(new[] { 1, 2, 3 });
        var enumerator = AsyncPagedEnumerator.Create(disposable);

        // Act
        await enumerator.DisposeAsync();
        await enumerator.DisposeAsync();
        await enumerator.DisposeAsync();

        // Assert
        disposable.DisposeCount.Should().Be(1);
    }

    [Fact]
    public async Task MoveNextAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator());
        await enumerator.DisposeAsync();

        // Act
        Func<Task> act = async () => await enumerator.MoveNextAsync();

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task MoveNextAsync_WithCancellationToken_ShouldStopEnumerating()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var source = CreateCancellableAsyncEnumerable(10, cts.Token);
        var enumerator = AsyncPagedEnumerator.Create(
            source.GetAsyncEnumerator(cts.Token),
            Pagination.Empty,
            cts.Token);

        // Act
        _ = await enumerator.MoveNextAsync(); // First item succeeds
        cts.Cancel();

        // The enumerator will stop when the source respects cancellation
        bool result = false;
        try
        {
            result = await enumerator.MoveNextAsync();
        }
        catch (OperationCanceledException)
        {
            // Expected if source throws on cancellation
        }

        // Assert - Either returns false or throws OperationCanceledException
        result.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullEnumerator_ShouldNotThrow()
    {
        // Arrange & Act
        // Note: AsyncPagedEnumerator.Create accepts null and wraps it
        var enumerator = AsyncPagedEnumerator.Create<int>(null!);

        // Assert
        enumerator.Should().NotBeNull();
    }

    [Fact]
    public void Empty_ShouldCreateEnumeratorWithEmptyPagination()
    {
        // Act
        var enumerator = AsyncPagedEnumerator.Empty<string>();

        // Assert
        enumerator.Pagination.Should().Be(Pagination.Empty);
    }

    [Fact]
    public void Empty_WithCustomPagination_ShouldUseProvidedPagination()
    {
        // Arrange
        Pagination customPagination = Pagination.Create(25, 3, "token", 200);

        // Act
        var enumerator = AsyncPagedEnumerator.Empty<int>(customPagination);

        // Assert
        enumerator.Pagination.Should().Be(customPagination);
    }

    [Fact]
    public async Task Pagination_WithPerPageStrategyAndZeroPageSize_ShouldNotUpdateCurrentPage()
    {
        // Arrange
        var source = CreateAsyncEnumerable(1, 2, 3);
        Pagination initialPagination = Pagination.Create(0, 1); // Zero page size
        var enumerator = AsyncPagedEnumerator.Create(source.GetAsyncEnumerator(), initialPagination)
            .WithPerPageStrategy();

        // Act
        while (await enumerator.MoveNextAsync())
        {
            // Enumerate
        }

        // Assert
        enumerator.Pagination.CurrentPage.Should().Be(1); // Should remain unchanged
    }

    [Fact]
    public void ExtensionMethods_WithNullEnumerator_ShouldThrowArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerator<int> nullEnumerator = null!;

        // Act & Assert
        Action act1 = () => nullEnumerator.WithPerPageStrategy();
        Action act2 = () => nullEnumerator.WithPerItemStrategy();
        Action act3 = () => nullEnumerator.WithNoStrategy();

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
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
            await Task.Delay(10, cancellationToken);
            yield return i;
        }
    }

    // Helper class for tracking disposal
    private sealed class DisposableAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly T[] _items;
        private int _index = -1;

        public DisposableAsyncEnumerator(T[] items)
        {
            _items = items;
        }

        public T Current => _index >= 0 && _index < _items.Length ? _items[_index] : default!;

        public bool IsDisposed { get; private set; }
        public int DisposeCount { get; private set; }

        public ValueTask<bool> MoveNextAsync()
        {
            _index++;
            return new ValueTask<bool>(_index < _items.Length);
        }

        public ValueTask DisposeAsync()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                DisposeCount++;
            }
            return ValueTask.CompletedTask;
        }
    }
}
