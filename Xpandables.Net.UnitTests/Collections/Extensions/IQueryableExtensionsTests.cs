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

using Microsoft.EntityFrameworkCore;

namespace Xpandables.Net.UnitTests.Collections.Extensions;

/// <summary>
/// Unit tests for IQueryableExtensions methods.
/// </summary>
public sealed class IQueryableExtensionsTests
{
    [Fact]
    public async Task ToAsyncPagedEnumerable_FromQueryable_ShouldCreatePagedEnumerable()
    {
        // Arrange
        var data = Enumerable.Range(0, 50).AsQueryable();

        // Act
        var paged = data.ToAsyncPagedEnumerable();

        // Assert
        paged.Should().NotBeNull("paged enumerable should be created from queryable");

        var items = await paged.ToListAsync();
        items.Should()
            .HaveCount(50, "all items should be enumerated")
            .And.BeInAscendingOrder("items should maintain order");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithPaginationFactory_ShouldUsePagination()
    {
        // Arrange
        var data = Enumerable.Range(0, 100).AsQueryable();
        var expectedPagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);

        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            _ => ValueTask.FromResult(expectedPagination));

        // Act
        var paged = data.ToAsyncPagedEnumerable(paginationFactory);
        var pagination = await paged.GetPaginationAsync();

        // Assert
        pagination.Should()
            .NotBe(Pagination.Empty, "pagination should be computed");
        pagination.PageSize.Should()
            .Be(expectedPagination.PageSize, "pageSize should match factory");
        pagination.CurrentPage.Should()
            .Be(expectedPagination.CurrentPage, "currentPage should match factory");
        pagination.TotalCount.Should()
            .Be(expectedPagination.TotalCount, "totalCount should match factory");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithSkipTake_ShouldPaginateCorrectly()
    {
        // Arrange
        const int pageSize = 10;
        const int skip = 20;
        var data = Enumerable.Range(0, 100).AsQueryable();
        var paged = data.Skip(skip).Take(pageSize).ToAsyncPagedEnumerable();

        // Act
        var items = await paged.ToListAsync();
        var pagination = await paged.GetPaginationAsync();

        // Assert
        items.Should()
            .HaveCount(pageSize, "should have exactly page size items")
            .And.StartWith(skip, "first item should be at skip position");

        pagination.PageSize.Should()
            .Be(pageSize, "pagination should reflect page size");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_EmptyQueryable_ShouldReturnEmpty()
    {
        // Arrange
        var emptyData = Enumerable.Empty<int>().AsQueryable();

        // Act
        var paged = emptyData.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .BeEmpty("empty queryable should produce empty enumerable");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithLargeDataset_ShouldHandleEfficiently()
    {
        // Arrange
        const int dataSize = 10000;
        var data = Enumerable.Range(0, dataSize).AsQueryable();

        // Act
        var paged = data.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(dataSize, "all items should be enumerated from large dataset")
            .And.BeInAscendingOrder("items should maintain order");
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithNullSource_ShouldThrow()
    {
        // Arrange
        IQueryable<int>? nullSource = null;

        // Act & Assert
        var act = () => nullSource!.ToAsyncPagedEnumerable();
        act.Should().Throw<ArgumentNullException>("null source should throw");
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithNullFactory_ShouldThrow()
    {
        // Arrange
        var data = Enumerable.Range(0, 10).AsQueryable();
        Func<CancellationToken, ValueTask<Pagination>>? nullFactory = null;

        // Act & Assert
        var act = () => data.ToAsyncPagedEnumerable(nullFactory!);
        act.Should().Throw<ArgumentNullException>("null factory should throw");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithFilter_ShouldApplyFiltering()
    {
        // Arrange
        var data = Enumerable.Range(0, 100).AsQueryable();
        var evenNumbers = data.Where(x => x % 2 == 0);

        // Act
        var paged = evenNumbers.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(50, "should have 50 even numbers");

        items.Should().AllSatisfy(item => (item % 2).Should().Be(0));
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithProjection_ShouldProjectCorrectly()
    {
        // Arrange
        var data = Enumerable.Range(0, 10).AsQueryable();
        var projected = data.Select(x => new { Value = x, Doubled = x * 2 });

        // Act
        var paged = projected.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(10, "should have all items after projection")
            .And.AllSatisfy(item => item.Doubled.Should().Be(item.Value * 2, "doubled value should be correct"));
    }

    static readonly IQueryable<int> data2 = new[] { 4, 1, 3, 2 }.AsQueryable();
    [Fact]
    public async Task ToAsyncPagedEnumerable_WithOrderBy_ShouldMaintainOrder()
    {
        // Arrange
        var sorted = data2.OrderBy(x => x);

        // Act
        var paged = sorted.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .BeInAscendingOrder("items should be sorted");

        items.Should().Equal([1, 2, 3, 5, 8, 9],
            "items should be in correct order");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithMultipleClauses_ShouldApplyAll()
    {
        // Arrange
        var data = Enumerable.Range(0, 100).AsQueryable();
        var query = data
            .Where(x => x > 10)
            .OrderByDescending(x => x)
            .Take(20);

        // Act
        var paged = query.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(20, "should have 20 items after take")
            .And.BeInDescendingOrder("items should be in descending order");

        items.First().Should().Be(99, "first item should be highest value");
        items.Last().Should().Be(80, "last item should be 80");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_MultipleEnumerations_ShouldWork()
    {
        // Arrange
        var data = Enumerable.Range(0, 25).AsQueryable();
        var paged = data.ToAsyncPagedEnumerable();

        // Act
        var firstEnumeration = await paged.ToListAsync();
        var secondEnumeration = await paged.ToListAsync();

        // Assert
        firstEnumeration.Should()
            .HaveCount(25, "first enumeration should have all items")
            .And.Equal(secondEnumeration, "both enumerations should be identical");
    }

    static readonly IQueryable<int> data1 = new[] { 1, 2, 2, 3, 3, 3, 4 }.AsQueryable();
    [Fact]
    public async Task ToAsyncPagedEnumerable_WithDistinct_ShouldRemoveDuplicates()
    {
        // Arrange
        var distinct = data1.Distinct();

        // Act
        var paged = distinct.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(4, "should have 4 distinct items");

        items.Should().Equal([1, 2, 3, 4],
            "distinct items should be correct");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithGroupBy_ShouldGroupCorrectly()
    {
        // Arrange
        var data = Enumerable.Range(0, 20).AsQueryable();
        var grouped = data.GroupBy(x => x % 5).Select(g => new { g.Key, Count = g.Count() });

        // Act
        var paged = grouped.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(5, "should have 5 groups")
            .And.AllSatisfy(item => item.Count.Should().Be(4, "each group should have 4 items"));
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithCancellation_ShouldRespectToken()
    {
        // Arrange
        var data = Enumerable.Range(0, 1000).AsQueryable();
        var paged = data.ToAsyncPagedEnumerable();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        var act = async () =>
        {
            await foreach (var item in paged.WithCancellation(cts.Token))
            {
                await Task.Delay(10, cts.Token);
            }
        };

        await act.Should().ThrowAsync<OperationCanceledException>(
            "should throw when cancellation is requested");
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_PaginationTotalCount_ShouldBeAccurate()
    {
        // Arrange
        const int totalItems = 150;
        const int pageSize = 10;
        var data = Enumerable.Range(0, totalItems).AsQueryable();

        // Act
        var paged = data.Take(pageSize).ToAsyncPagedEnumerable();
        var pagination = await paged.GetPaginationAsync();

        // Assert
        pagination.TotalCount.Should()
            .NotBeNull("total count should be computed");
        pagination.PageSize.Should()
            .Be(pageSize, "page size should be set");
        pagination.CurrentPage.Should()
            .BeGreaterThan(0, "current page should be at least 1");
    }
}

/// <summary>
/// Integration tests for IQueryableExtensions with complex scenarios.
/// </summary>
public sealed class IQueryableExtensionsIntegrationTests
{
    [Fact]
    public async Task ComplexQuery_DatabaseLikeScenario_ShouldWorkCorrectly()
    {
        // Arrange - Simulate a database query
        var customers = Enumerable.Range(1, 100)
            .Select(i => new Customer { Id = i, Name = $"Customer_{i}", Age = 20 + (i % 50) })
            .AsQueryable();

        var query = customers
            .Where(c => c.Age > 30)
            .OrderByDescending(c => c.Age)
            .Take(20);

        // Act
        var paged = query.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();
        var pagination = await paged.GetPaginationAsync();

        // Assert
        items.Should()
            .NotBeEmpty("should have matching customers")
            .And.AllSatisfy(c => c.Age.Should().BeGreaterThan(30, "age filter should be applied"));

        pagination.Should()
            .NotBe(Pagination.Empty, "pagination should be computed");
    }

    [Fact]
    public async Task LazyQueryComposition_ShouldWorkWithPaging()
    {
        // Arrange
        var data = Enumerable.Range(0, 100).AsQueryable();

        static IQueryable<int> BuildQuery(IQueryable<int> source)
        {
            var filtered = source.Where(x => x % 2 == 0);
            var projected = filtered.Select(x => x * 2);
            var ordered = projected.OrderBy(x => x);
            return ordered.Take(10);
        }

        var query = BuildQuery(data);

        // Act
        var paged = query.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(10, "should have exactly 10 items")
            .And.BeInAscendingOrder("items should be ordered")
            .And.AllSatisfy(item => (item % 4).Should().Be(0, "all items should be divisible by 4"));
    }

    [Fact]
    public async Task SkipTakePagination_ShouldNavigateThroughPages()
    {
        // Arrange
        const int totalItems = 100;
        const int pageSize = 10;
        var data = Enumerable.Range(0, totalItems).AsQueryable();

        // Act
        var allPages = new List<List<int>>();
        for (int pageNumber = 0; pageNumber < totalItems / pageSize; pageNumber++)
        {
            var page = data.Skip(pageNumber * pageSize).Take(pageSize).ToAsyncPagedEnumerable();
            var items = await page.ToListAsync();
            allPages.Add(items);
        }

        // Assert
        allPages.Should().HaveCount(10, "should have 10 pages");

        for (int i = 0; i < allPages.Count; i++)
        {
            allPages[i].Should()
                .HaveCount(pageSize, $"page {i} should have {pageSize} items")
                .And.Equal(
                    Enumerable.Range(i * pageSize, pageSize),
                    $"page {i} should have correct range of items");
        }
    }

    [Fact]
    public async Task FilterAndPaginate_ShouldCombineOperations()
    {
        // Arrange
        var records = Enumerable.Range(1, 1000)
            .Select(i => new Record { Id = i, Category = i % 5, Value = i * 10 })
            .AsQueryable();

        const int category = 2;
        var filtered = records.Where(r => r.Category == category).Take(50);

        // Act
        var paged = filtered.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();
        var pagination = await paged.GetPaginationAsync();

        // Assert
        items.Should()
            .NotBeEmpty("should have filtered items")
            .And.AllSatisfy(item => item.Category.Should().Be(category, "all items should match filter"));

        pagination.PageSize.Should()
            .Be(50, "page size should reflect take operation");
    }

    // Test Models
    private class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private class Record
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public int Value { get; set; }
    }
}

/// <summary>
/// Performance and stress tests for IQueryableExtensions.
/// </summary>
public sealed class IQueryableExtensionsPerformanceTests
{
    [Fact]
    public async Task LargeQueryable_ShouldHandleEfficiently()
    {
        // Arrange
        const int largeSize = 100000;
        var data = Enumerable.Range(0, largeSize).AsQueryable();

        // Act
        var paged = data.ToAsyncPagedEnumerable();
        var count = 0;

        await foreach (var item in paged)
        {
            count++;
            if (count % 10000 == 0)
                GC.Collect(0, GCCollectionMode.Optimized);
        }

        // Assert
        count.Should().Be(largeSize, "should enumerate all items from large queryable");
    }

    [Fact]
    public async Task ComplexQueryExpression_ShouldExecuteEfficiently()
    {
        // Arrange
        var data = Enumerable.Range(0, 10000).AsQueryable();
        var complex = data
            .Where(x => x > 100)
            .Where(x => x < 9000)
            .OrderBy(x => x % 7)
            .ThenBy(x => x % 11)
            .Skip(100)
            .Take(500)
            .Select(x => new { Value = x, Mod7 = x % 7, Mod11 = x % 11 });

        // Act
        var paged = complex.ToAsyncPagedEnumerable();
        var items = await paged.ToListAsync();

        // Assert
        items.Should()
            .HaveCount(500, "should have exactly 500 items after skip/take")
            .And.AllSatisfy(item =>
            {
                item.Value.Should().BeGreaterThan(100);
                item.Value.Should().BeLessThan(9000);
            });
    }

    [Fact]
    public async Task RepeatedEnumeration_ShouldNotCauseMemoryIssues()
    {
        // Arrange
        var data = Enumerable.Range(0, 1000).AsQueryable();
        var paged = data.ToAsyncPagedEnumerable();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var items = await paged.ToListAsync();
            items.Should().HaveCount(1000, "each enumeration should have all items");
        }

        // Assert - If we get here, no memory explosion occurred
        Assert.True(true, "repeated enumerations completed successfully");
    }

    [Fact(Timeout = 5000)]
    public async Task LargeQueryableEnumeration_ShouldCompleteInReasonableTime()
    {
        // Arrange
        const int largeSize = 50000;
        var data = Enumerable.Range(0, largeSize).AsQueryable();
        var paged = data.ToAsyncPagedEnumerable();

        // Act
        var startTime = DateTime.UtcNow;
        var count = await paged.CountAsync();
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        count.Should().Be(largeSize, "should count all items");
        elapsed.Should()
            .BeLessThan(TimeSpan.FromSeconds(5), "enumeration should complete quickly");
    }
}
