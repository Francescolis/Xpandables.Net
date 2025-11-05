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
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.UnitTests.AsyncPaged.Integration;

/// <summary>
/// End-to-end integration tests for async paged enumerable functionality.
/// </summary>
public sealed class EndToEndIntegrationTests
{
    [Fact]
    public async Task CompleteWorkflow_QueryableToPagedEnumerable_WithFiltersAndPagination()
    {
        // Arrange - Simulate a database query
        var dataSource = CreateSampleDataSource(500);
        var query = dataSource.AsQueryable()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Skip(40)
            .Take(20);

        // Act - Convert to paged enumerable
        IAsyncPagedEnumerable<Product> pagedEnumerable = query.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();
        var items = new List<Product>();
        await foreach (var item in pagedEnumerable)
        {
            items.Add(item);
        }

        // Assert
        pagination.PageSize.Should().Be(20);
        pagination.CurrentPage.Should().Be(3); // (40 / 20) + 1
        pagination.TotalCount.Should().BeGreaterThan(0);
        items.Should().HaveCountLessThanOrEqualTo(20);
        items.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task CompleteWorkflow_WithCustomPaginationFactory_AndExtensions()
    {
        // Arrange
        var dataSource = CreateSampleDataSource(100);

        async IAsyncEnumerable<Product> AsyncSource()
        {
            foreach (var item in dataSource)
            {
                await Task.Yield();
                yield return item;
            }
        }

        // Act - Create paged enumerable with custom factory
        var pagedEnumerable = new AsyncPagedEnumerable<Product>(
            AsyncSource(),
            async ct =>
            {
                await Task.Delay(10, ct);
                return Pagination.Create(10, 1, null, dataSource.Count);
            });

        // Apply extensions
        var result = pagedEnumerable
            .WherePaged(p => p.Price > 50)
            .TakePaged(15);

        var items = new List<Product>();
        await foreach (var item in result)
        {
            items.Add(item);
        }
        var pagination = await result.GetPaginationAsync();

        // Assert
        items.Should().HaveCountLessThanOrEqualTo(15);
        items.Should().AllSatisfy(p => p.Price.Should().BeGreaterThan(50));
        pagination.TotalCount.Should().Be(dataSource.Count);
    }

    [Fact]
    public async Task CompleteWorkflow_WithEnumeratorStrategies()
    {
        // Arrange
        var dataSource = Enumerable.Range(1, 50).ToArray();
        async IAsyncEnumerable<int> AsyncSource()
        {
            foreach (var item in dataSource)
            {
                await Task.Yield();
                yield return item;
            }
        }

        var pagedEnumerable = new AsyncPagedEnumerable<int>(
            AsyncSource(),
            _ => new ValueTask<Pagination>(Pagination.Create(10, 0, null, 50)));

        // Act - The GetAsyncEnumerator returns AsyncPagedEnumerator which implements IAsyncPagedEnumerator
        await using IAsyncPagedEnumerator<int> enumerator = (IAsyncPagedEnumerator<int>)pagedEnumerable.GetAsyncEnumerator();
        enumerator.WithPerItemStrategy();

        var itemCount = 0;
        while (await enumerator.MoveNextAsync())
        {
            itemCount++;
            if (itemCount == 10)
            {
                // Check pagination after 10 items
                enumerator.Pagination.CurrentPage.Should().Be(10);
            }
        }

        // Assert
        itemCount.Should().Be(50);
        enumerator.Pagination.TotalCount.Should().Be(50);
    }

    [Fact]
    public async Task CompleteWorkflow_MultiplePages_WithNavigationMetadata()
    {
        // Arrange - Simulate paginated API calls
        const int totalItems = 100;
        const int pageSize = 10;
        var allData = CreateSampleDataSource(totalItems);

        var pages = new List<PageData>();

        // Act - Simulate fetching multiple pages
        for (int pageNumber = 1; pageNumber <= 5; pageNumber++)
        {
            var query = allData.AsQueryable()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            IAsyncPagedEnumerable<Product> pagedEnumerable = query.ToAsyncPagedEnumerable();
            Pagination pagination = await pagedEnumerable.GetPaginationAsync();
            var items = new List<Product>();
            await foreach (var item in pagedEnumerable)
            {
                items.Add(item);
            }

            pages.Add(new PageData(pagination, items));
        }

        // Assert
        pages.Should().HaveCount(5);

        // First page
        pages[0].Pagination.IsFirstPage.Should().BeTrue();
        pages[0].Pagination.HasPreviousPage.Should().BeFalse();
        pages[0].Pagination.HasNextPage.Should().BeTrue();

        // Middle page
        pages[2].Pagination.IsFirstPage.Should().BeFalse();
        pages[2].Pagination.HasPreviousPage.Should().BeTrue();
        pages[2].Pagination.HasNextPage.Should().BeTrue();

        // Last page (page 5) - still has next because total is 100 and we're only at 50
        // Page 5: skip 40, take 10, so items 41-50. With 100 total, there are still items left
        pages[4].Pagination.HasNextPage.Should().BeTrue();
        pages[4].Pagination.IsLastPage.Should().BeFalse();  // Not the last page yet (we have 10 pages total)
        pages[4].Pagination.TotalPages.Should().Be(10);
    }

    [Fact]
    public async Task CompleteWorkflow_WithChaining_AndComplexOperations()
    {
        // Arrange
        var dataSource = CreateSampleDataSource(200);

        async IAsyncEnumerable<Product> AsyncSource()
        {
            foreach (var item in dataSource.OrderBy(p => p.Category))
            {
                await Task.Yield();
                yield return item;
            }
        }

        var pagedEnumerable = new AsyncPagedEnumerable<Product>(
            AsyncSource(),
            _ => new ValueTask<Pagination>(Pagination.Create(20, 1, null, 200)));

        // Act - Chain multiple operations
        var result = pagedEnumerable
            .WherePaged(p => p.IsActive && p.Price > 20)
            .DistinctByPaged(p => p.Category)
            .TakePaged(10);

        var items = new List<Product>();
        await foreach (var item in result)
        {
            items.Add(item);
        }

        // Assert
        items.Should().HaveCountLessThanOrEqualTo(10);
        items.Should().AllSatisfy(p =>
        {
            p.IsActive.Should().BeTrue();
            p.Price.Should().BeGreaterThan(20);
        });
        items.Select(p => p.Category).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CompleteWorkflow_WithCancellation_AndGracefulHandling()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var dataSource = CreateSampleDataSource(1000);

        async IAsyncEnumerable<Product> AsyncSource([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var item in dataSource)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(5, ct);
                yield return item;
            }
        }

        var pagedEnumerable = new AsyncPagedEnumerable<Product>(
            AsyncSource(cts.Token),
            _ => new ValueTask<Pagination>(Pagination.Create(100, 1, null, 1000)));

        // Act
        var items = new List<Product>();
        cts.CancelAfter(100); // Cancel after 100ms

        Func<Task> act = async () =>
        {
            await foreach (var item in pagedEnumerable.WithCancellation(cts.Token))
            {
                items.Add(item);
            }
        };

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        items.Should().NotBeEmpty(); // Some items should have been processed
    }

    [Fact]
    public async Task CompleteWorkflow_WithEmptyDataSet_ShouldHandleGracefully()
    {
        // Arrange
        var emptyQuery = Enumerable.Empty<Product>().AsQueryable();

        // Act
        IAsyncPagedEnumerable<Product> pagedEnumerable = emptyQuery.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();
        var items = new List<Product>();
        await foreach (var item in pagedEnumerable)
        {
            items.Add(item);
        }

        // Assert
        pagination.TotalCount.Should().Be(0);
        items.Should().BeEmpty();
        pagination.TotalPages.Should().BeNull();
    }

    [Fact]
    public async Task CompleteWorkflow_WithLazyPaginationComputation()
    {
        // Arrange
        var dataSource = CreateSampleDataSource(100);
        int computationCount = 0;

        async IAsyncEnumerable<Product> AsyncSource()
        {
            foreach (var item in dataSource)
            {
                await Task.Yield();
                yield return item;
            }
        }

        var pagedEnumerable = new AsyncPagedEnumerable<Product>(
            AsyncSource(),
            async _ =>
            {
                Interlocked.Increment(ref computationCount);
                await Task.Delay(50);
                return Pagination.Create(10, 1, null, 100);
            });

        // Act - Access Pagination property before computation
        Pagination beforeComputation = pagedEnumerable.Pagination;

        // Trigger computation
        Pagination afterComputation = await pagedEnumerable.GetPaginationAsync();

        // Access property again
        Pagination cached = pagedEnumerable.Pagination;

        // Assert
        beforeComputation.Should().Be(Pagination.Empty);
        afterComputation.PageSize.Should().Be(10);
        afterComputation.TotalCount.Should().Be(100);
        cached.Should().Be(afterComputation);
        computationCount.Should().Be(1); // Computed only once
    }

    [Fact]
    public async Task CompleteWorkflow_RealWorldScenario_PaginatedApiEndpoint()
    {
        // Arrange - Simulate database with filtering, sorting, and pagination
        var database = CreateSampleDataSource(1000);
        const int requestedPage = 5;
        const int requestedPageSize = 25;

        // Simulate typical API query
        var query = database.AsQueryable()
            .Where(p => p.IsActive)
            .Where(p => p.Category == "Electronics" || p.Category == "Books")
            .OrderByDescending(p => p.Price)
            .ThenBy(p => p.Name)
            .Skip((requestedPage - 1) * requestedPageSize)
            .Take(requestedPageSize);

        // Act
        IAsyncPagedEnumerable<Product> pagedEnumerable = query.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();
        var items = new List<Product>();
        await foreach (var item in pagedEnumerable)
        {
            items.Add(item);
        }

        // Assert - Validate typical API response
        pagination.PageSize.Should().Be(requestedPageSize);
        pagination.CurrentPage.Should().Be(requestedPage);
        pagination.TotalCount.Should().BeGreaterThan(0);
        pagination.TotalPages.Should().BeGreaterThan(0);
        pagination.HasPreviousPage.Should().BeTrue();
        pagination.Skip.Should().Be((requestedPage - 1) * requestedPageSize);

        items.Should().HaveCountLessThanOrEqualTo(requestedPageSize);
        items.Should().AllSatisfy(p =>
        {
            p.IsActive.Should().BeTrue();
            (p.Category == "Electronics" || p.Category == "Books").Should().BeTrue();
        });

        // Verify ordering
        items.Should().BeInDescendingOrder(p => p.Price);
    }

    // Helper methods
    private static List<Product> CreateSampleDataSource(int count)
    {
        var categories = new[] { "Electronics", "Books", "Clothing", "Food", "Toys" };
        var random = new Random(42); // Fixed seed for reproducibility
        var products = new List<Product>();

        for (int i = 1; i <= count; i++)
        {
            products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Price = random.Next(10, 500),
                Category = categories[random.Next(categories.Length)],
                IsActive = random.Next(0, 2) == 1
            });
        }

        return products;
    }

    private record PageData(Pagination Pagination, List<Product> Items);

    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
