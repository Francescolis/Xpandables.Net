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

namespace Xpandables.Net.UnitTests.Collections.Generic;

/// <summary>
/// Unit tests for IAsyncPagedEnumerable interface and AsyncPagedEnumerable implementation.
/// </summary>
public sealed class IAsyncPagedEnumerableTests
{
    [Fact]
    public async Task Create_FromAsyncEnumerable_ShouldCreateValidPagedEnumerable()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(100);

		// Act
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);

        // Assert
        paged.Should().NotBeNull("paged enumerable should be created");
        paged.Pagination.Should().Be(Pagination.Empty, "initial pagination should be empty");
    }

    [Fact]
    public async Task Create_WithPaginationFactory_ShouldUsePaginationFactory()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(50);
        var expectedPagination = Pagination.Create(
            pageSize: 10,
            currentPage: 1,
            totalCount: 50);

        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            _ => ValueTask.FromResult(expectedPagination));

		// Act
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source, paginationFactory);
		Pagination pagination = await paged.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(expectedPagination.PageSize, "pageSize should match expected");
        pagination.CurrentPage.Should().Be(expectedPagination.CurrentPage, "currentPage should match expected");
        pagination.TotalCount.Should().Be(expectedPagination.TotalCount, "totalCount should match expected");
    }

    [Fact]
    public async Task Empty_ShouldReturnEmptyEnumerable()
    {
		// Act
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Empty<int>();
		List<int> items = await paged.ToListAsync();

        // Assert
        items.Should().BeEmpty("empty paged enumerable should have no items");
		Pagination pagination = await paged.GetPaginationAsync();
        pagination.Should().Be(Pagination.Empty, "empty enumerable should have empty pagination");
    }

    [Fact]
    public async Task Empty_WithPagination_ShouldReturnEmptyWithPagination()
    {
        // Arrange
        var expectedPagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 0);

		// Act
		IAsyncPagedEnumerable<string> paged = AsyncPagedEnumerable.Empty<string>(expectedPagination);
		List<string> items = await paged.ToListAsync();
		Pagination pagination = await paged.GetPaginationAsync();

        // Assert
        items.Should().BeEmpty("empty enumerable should have no items");
        pagination.Should().Be(expectedPagination, "pagination should match provided value");
    }

    [Fact]
    public async Task Enumeration_ShouldYieldAllElements()
    {
        // Arrange
        const int count = 25;
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(count);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);

        // Act
        var items = new List<int>();
        await foreach (int item in paged)
        {
            items.Add(item);
        }

        // Assert
        items.Should()
            .HaveCount(count, "should enumerate all items")
            .And.Equal(Enumerable.Range(0, count), "items should be in correct order");
    }

    [Fact]
    public async Task GetPaginationAsync_ShouldComputePaginationOnce()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(100);
		int callCount = 0;
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            async ct =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(10, ct).ConfigureAwait(false);
                return Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);
            });

		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source, paginationFactory);

		// Act
		Pagination pagination1 = await paged.GetPaginationAsync();
		Pagination pagination2 = await paged.GetPaginationAsync();

        // Assert
        callCount.Should().Be(1, "factory should be called only once due to caching");
        pagination1.Should().Be(pagination2, "pagination should be identical on subsequent calls");
    }

    [Fact]
    public async Task Pagination_Property_ShouldReturnEmptyBeforeComputation()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(50);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);

		// Act
		Pagination pagination = paged.Pagination;

        // Assert
        pagination.Should().Be(Pagination.Empty, "pagination should be empty before computation");
    }

    [Fact]
    public async Task MultipleEnumerations_ShouldWork()
    {
        // Arrange
        const int count = 20;
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(count);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);

		// Act
		List<int> firstEnumeration = await paged.ToListAsync();
		List<int> secondEnumeration = await paged.ToListAsync();

        // Assert
        firstEnumeration.Should().HaveCount(count, "first enumeration should have all items");
        secondEnumeration.Should().HaveCount(count, "second enumeration should also have all items");
    }

    [Fact]
    public async Task CancellationToken_ShouldBePropagated()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(1000);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(10));

        // Act & Assert
        // Note: OperationCanceledException is thrown by CancellationToken.ThrowIfCancellationRequested(),
        // while TaskCanceledException (a subclass) is thrown by Task-based operations.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
            {
                await foreach (int item in paged.WithCancellation(cts.Token))
                {
                    await Task.Delay(5, cts.Token);
                }
            });
    }

    [Fact]
    public async Task WithCancellation_ShouldRespectToken()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(100);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);
        var cts = new CancellationTokenSource();

        // Act
        var items = new List<int>();
        int count = 0;
        try
        {
            await foreach (int item in paged.WithCancellation(cts.Token))
            {
                items.Add(item);
                count++;
                if (count == 50)
                {
                    cts.Cancel();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        count.Should().BeLessThanOrEqualTo(50, "enumeration should stop at cancellation point");
    }

    [Fact]
    public async Task Dispose_ShouldCleanupResources()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(100);
        var paged = AsyncPagedEnumerable.Create(source) as IDisposable;

		// Act & Assert - should not throw
		Action act = () => paged?.Dispose();
        act.Should().NotThrow("dispose should complete without errors");
    }

    private static async IAsyncEnumerable<int> CreateAsyncEnumerable(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }
}

/// <summary>
/// Unit tests for Pagination record and its operations.
/// </summary>
public sealed class PaginationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Act
        var pagination = Pagination.Create(
            pageSize: 10,
            currentPage: 2,
            continuationToken: "token123",
            totalCount: 100);

        // Assert
        pagination.PageSize.Should().Be(10, "pageSize should be set correctly");
        pagination.CurrentPage.Should().Be(2, "currentPage should be set correctly");
        pagination.ContinuationToken.Should().Be("token123", "continuation token should be set correctly");
        pagination.TotalCount.Should().Be(100, "totalCount should be set correctly");
    }

    [Fact]
    public void Create_WithNegativePageSize_ShouldThrow()
    {
		// Act & Assert
		Func<Pagination> act = () => Pagination.Create(pageSize: -1, currentPage: 1);
        act.Should().Throw<ArgumentOutOfRangeException>("negative pageSize should throw");
    }

    [Fact]
    public void FromTotalCount_ShouldCreateWithTotalCount()
    {
        // Act
        var pagination = Pagination.FromTotalCount(50);

        // Assert
        pagination.TotalCount.Should().Be(50, "totalCount should match provided value");
        pagination.PageSize.Should().Be(0, "pageSize should default to 0");
        pagination.CurrentPage.Should().Be(0, "currentPage should default to 0");
    }

    [Fact]
    public void NextPage_ShouldIncrementCurrentPage()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

		// Act
		Pagination nextPagination = pagination.NextPage();

        // Assert
        nextPagination.CurrentPage.Should().Be(2, "next page should increment current page");
    }

    [Fact]
    public void PreviousPage_ShouldDecrementCurrentPage()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 2, totalCount: 100);

		// Act
		Pagination prevPagination = pagination.PreviousPage();

        // Assert
        prevPagination.CurrentPage.Should().Be(1, "previous page should decrement current page");
    }

    [Fact]
    public void PreviousPage_OnFirstPage_ShouldRemainOnFirstPage()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

		// Act
		Pagination prevPagination = pagination.PreviousPage();

        // Assert
        prevPagination.CurrentPage.Should().Be(1, "should remain on first page when already on first page");
    }

    [Fact]
    public void Skip_ShouldCalculateCorrectly()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 3, totalCount: 100);

        // Act
        int skip = pagination.Skip;

        // Assert
        skip.Should().Be(20, "skip should be (currentPage - 1) * pageSize");
    }

    [Fact]
    public void HasNextPage_ShouldReturnTrueWhenMorePages()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

        // Act
        bool hasNext = pagination.HasNextPage;

        // Assert
        hasNext.Should().BeTrue("should have next page when not on last page");
    }

    [Fact]
    public void HasNextPage_ShouldReturnFalseOnLastPage()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 10, totalCount: 100);

        // Act
        bool hasNext = pagination.HasNextPage;

        // Assert
        hasNext.Should().BeFalse("should not have next page when on last page");
    }

    [Fact]
    public void IsLastPage_ShouldReturnTrueWhenOnLastPage()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 10, totalCount: 100);

        // Act
        bool isLast = pagination.IsLastPage;

        // Assert
        isLast.Should().BeTrue("should identify last page correctly");
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

        // Act
        int? totalPages = pagination.TotalPages;

        // Assert
        totalPages.Should().Be(10, "total pages should be calculated correctly");
    }

    [Fact]
    public void TotalPages_WithNonDivisibleCount_ShouldRoundUp()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 105);

        // Act
        int? totalPages = pagination.TotalPages;

        // Assert
        totalPages.Should().Be(11, "total pages should round up for non-divisible counts");
    }

    [Fact]
    public void HasContinuation_ShouldReturnTrueWhenTokenPresent()
    {
        // Arrange
        var pagination = Pagination.Create(
            pageSize: 10,
            currentPage: 1,
            continuationToken: "next_page_token",
            totalCount: 100);

        // Act
        bool hasContinuation = pagination.HasContinuation;

        // Assert
        hasContinuation.Should().BeTrue("should have continuation when token is present");
    }

    [Fact]
    public void HasContinuation_ShouldReturnFalseWhenNoToken()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

        // Act
        bool hasContinuation = pagination.HasContinuation;

        // Assert
        hasContinuation.Should().BeFalse("should not have continuation when token is null");
    }

    [Fact]
    public void IsUnknown_ShouldReturnTrueWhenTotalCountNull()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: null);

        // Act
        bool isUnknown = pagination.IsUnknown;

        // Assert
        isUnknown.Should().BeTrue("should be unknown when totalCount is null");
    }

    [Fact]
    public void WithTotalCount_ShouldUpdateTotalCount()
    {
        // Arrange
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

		// Act
		Pagination updated = pagination.WithTotalCount(200);

        // Assert
        updated.TotalCount.Should().Be(200, "should update total count");
        pagination.TotalCount.Should().Be(100, "original should remain unchanged");
    }
}
