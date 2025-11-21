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
using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.UnitTests.Collections.Generic;

/// <summary>
/// Integration tests for real-world scenarios with IAsyncPagedEnumerable.
/// </summary>
public sealed class AsyncPagedEnumerableRealWorldTests
{
    /// <summary>
    /// Simulates a database query with server-side pagination.
    /// </summary>
    [Fact]
    public async Task DatabaseQuery_WithServerSidePagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var dataset = SimulateDatabase(totalRecords: 250);
        const int pageSize = 25;
        int currentPage = 1;

        // Act
        var pagedResults = new List<(List<DatabaseRecord>, Pagination)>();
        int totalProcessed = 0;

        while (totalProcessed < 250)
        {
            var pageData = dataset
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToAsyncEnumerable();

            var pagination = Pagination.Create(
                pageSize: pageSize,
                currentPage: currentPage,
                totalCount: 250);

            var paged = AsyncPagedEnumerable.Create(pageData, _ => ValueTask.FromResult(pagination));
            var items = await paged.ToListAsync();
            pagedResults.Add((items, await paged.GetPaginationAsync()));

            totalProcessed += items.Count;
            currentPage++;
        }

        // Assert
        Assert.Equal(10, pagedResults.Count); // 250 records / 25 per page = 10 pages
        foreach (var (items, pagination) in pagedResults)
        {
            Assert.Equal(pageSize, items.Count);
            Assert.Equal(pageSize, pagination.PageSize);
            Assert.Equal(250, pagination.TotalCount);
        }
    }

    /// <summary>
    /// Simulates streaming large amounts of data with pagination.
    /// </summary>
    [Fact]
    public async Task StreamingLargeDataset_ShouldProcessEfficientlyWithPagination()
    {
        // Arrange
        const int totalItems = 5000;
        var source = GenerateStreamData(totalItems);
        var pagination = Pagination.Create(pageSize: 500, currentPage: 1, totalCount: totalItems);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        // Act
        int count = 0;
        int pageBreaks = 0;
        await foreach (var item in paged)
        {
            count++;
            if (count % 500 == 0)
                pageBreaks++;
        }

        // Assert
        Assert.Equal(totalItems, count);
        Assert.Equal(10, pageBreaks);
    }

    /// <summary>
    /// Simulates a scenario where multiple consumers process the same paged data concurrently.
    /// </summary>
    [Fact]
    public async Task MultipleConsumers_ProcessingSamePaginatedData_ShouldWorkConcurrently()
    {
        // Arrange
        const int dataSize = 1000;
        var source = GenerateStreamData(dataSize);
        var pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: dataSize);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        // Act - Multiple consumers
        var consumer1 = Task.Run(async () =>
        {
            var sum = 0;
            await foreach (var item in paged)
                sum += item;
            return sum;
        });

        var consumer2 = Task.Run(async () =>
        {
            var count = 0;
            await foreach (var item in paged)
                count++;
            return count;
        });

        var consumer3 = Task.Run(async () =>
        {
            var max = 0;
            await foreach (var item in paged)
            {
                if (item > max)
                    max = item;
            }
            return max;
        });

        var sum = await consumer1;
        var count = await consumer2;
        var max = await consumer3;

        // Assert
        Assert.Equal(499500, sum); // Sum of 0 to 9999
        Assert.Equal(dataSize, count);
        Assert.Equal(dataSize - 1, max);
    }

    /// <summary>
    /// Simulates a scenario with dynamically computed pagination based on filters.
    /// </summary>
    [Fact]
    public async Task DynamicPaginationWithFilters_ShouldAdaptToFilteredResults()
    {
        // Arrange
        var fullDataset = Enumerable.Range(0, 100)
            .Select(i => new { Id = i, Value = i % 2 == 0 ? "even" : "odd" })
            .ToList();

        var filteredData = fullDataset
            .Where(x => x.Value == "even")
            .Select(x => x.Id)
            .ToAsyncEnumerable();

        // Create pagination factory that adapts to filtered count
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(async _ =>
        {
            await Task.Delay(1, _); // Simulate computation
            return Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 50); // 50 even numbers
        });

        var paged = AsyncPagedEnumerable.Create(filteredData, paginationFactory);

        // Act
        var items = await paged.ToListAsync();
        var pagination = await paged.GetPaginationAsync();

        // Assert
        Assert.Equal(50, items.Count);
        Assert.Equal(50, pagination.TotalCount);
    }

    /// <summary>
    /// Simulates rate-limited API consumption with pagination.
    /// </summary>
    [Fact]
    public async Task RateLimitedApi_WithPagination_ShouldRespectLimits()
    {
        // Arrange
        const int totalRequests = 100;
        int requestCount = 0;
        var lastRequestTime = DateTime.UtcNow;
        const int minDelayMs = 10;

        async IAsyncEnumerable<int> RateLimitedGenerator()
        {
            for (int i = 0; i < totalRequests; i++)
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - lastRequestTime).TotalMilliseconds;

                if (elapsed < minDelayMs)
                {
                    await Task.Delay((int)(minDelayMs - elapsed));
                }

                lastRequestTime = DateTime.UtcNow;
                Interlocked.Increment(ref requestCount);
                yield return i;
            }
        }

        var source = RateLimitedGenerator();
        var paged = AsyncPagedEnumerable.Create(source);

        // Act
        var startTime = DateTime.UtcNow;
        var items = await paged.ToListAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(totalRequests, items.Count);
        Assert.Equal(totalRequests, requestCount);
        Assert.True(duration.TotalMilliseconds >= minDelayMs * (totalRequests - 1));
    }

    /// <summary>
    /// Simulates processing with transformation and pagination.
    /// </summary>
    [Fact]
    public async Task TransformationWithPagination_ShouldApplyTransformationsEfficiently()
    {
        // Arrange
        const int size = 100;
        var source = GenerateStreamData(size)
            .Select(x => x * 2) // Double each value
            .Where(x => x % 4 == 0) // Filter multiples of 4
            .Select(x => new { Original = x, Squared = x * x });

        var paging = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 50);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(paging));

        // Act
        var items = await paged.ToListAsync();
        var pagination = await paged.GetPaginationAsync();

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, item =>
        {
            Assert.Equal(0, item.Original % 4);
            Assert.Equal(item.Original * item.Original, item.Squared);
        });
    }

    /// <summary>
    /// Simulates error recovery with pagination.
    /// </summary>
    [Fact]
    public async Task ErrorRecovery_ShouldContinueAfterTransientError()
    {
        // Arrange
        var attemptCount = 0;
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            async _ =>
            {
                await Task.Yield();
                Interlocked.Increment(ref attemptCount);

                if (attemptCount == 1)
                    throw new InvalidOperationException("Transient error");

                return Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 20);
            });

        var source = GenerateStreamData(20);
        var paged = AsyncPagedEnumerable.Create(source, paginationFactory);

        // Act & Assert
        var firstAttempt = await Assert.ThrowsAsync<InvalidOperationException>(
            () => paged.GetPaginationAsync());

        // Second attempt should succeed (pagination is recomputable)
        var paged2 = AsyncPagedEnumerable.Create(source, paginationFactory);
        var pagination = await paged2.GetPaginationAsync();

        Assert.Equal(20, pagination.TotalCount);
    }

    /// <summary>
    /// Simulates a search result scenario with pagination and continuation tokens.
    /// </summary>
    [Fact]
    public async Task SearchResults_WithContinuationToken_ShouldSupportCursorBasedPagination()
    {
        // Arrange
        const int totalResults = 200;
        const int pageSize = 50;
        var allResults = Enumerable.Range(0, totalResults)
            .Select(i => new SearchResult { Id = i, Title = $"Result_{i}" })
            .ToList();

        var pages = new List<Pagination>();
        string? continuationToken = null;

        // Act
        for (int page = 0; page < 4; page++)
        {
            var pageData = allResults
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToAsyncEnumerable();

            continuationToken = page < 3 ? $"cursor_{page + 1}" : null;
            var pagination = Pagination.Create(
                pageSize: pageSize,
                currentPage: page + 1,
                continuationToken: continuationToken,
                totalCount: totalResults);

            var paged = AsyncPagedEnumerable.Create(pageData, _ => ValueTask.FromResult(pagination));
            var items = await paged.ToListAsync();
            pages.Add(await paged.GetPaginationAsync());

            Assert.Equal(pageSize, items.Count);
        }

        // Assert
        Assert.Equal(4, pages.Count);
        for (int i = 0; i < 3; i++)
        {
            Assert.NotNull(pages[i].ContinuationToken);
            Assert.Equal($"cursor_{i + 1}", pages[i].ContinuationToken);
        }
        Assert.Null(pages[3].ContinuationToken);
    }

    /// <summary>
    /// Simulates monitoring and logging during pagination processing.
    /// </summary>
    [Fact]
    public async Task PaginationMonitoring_ShouldTrackProcessing()
    {
        // Arrange
        const int size = 100;
        var metrics = new PaginationMetrics();
        var source = GenerateStreamData(size);
        var pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: size);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        var startTime = DateTime.UtcNow;

        // Act
        int itemCount = 0;
        await foreach (var item in paged)
        {
            itemCount++;
            metrics.ItemsProcessed++;

            if (itemCount % 10 == 0)
            {
                metrics.PagesCompleted++;
            }
        }

        var duration = DateTime.UtcNow - startTime;
        metrics.ProcessingTimeMs = (long)duration.TotalMilliseconds;

        // Assert
        Assert.Equal(size, metrics.ItemsProcessed);
        Assert.Equal(10, metrics.PagesCompleted);
        Assert.NotEqual(default, metrics.ProcessingTimeMs);
    }

    // Helper methods
    private static List<DatabaseRecord> SimulateDatabase(int totalRecords)
    {
        return [.. Enumerable.Range(1, totalRecords).Select(i => new DatabaseRecord { Id = i, Value = $"Record_{i}" })];
    }

    private static async IAsyncEnumerable<int> GenerateStreamData(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }

    // Test models
    private class DatabaseRecord
    {
        public int Id { get; set; }
        public string? Value { get; set; }
    }

    private class SearchResult
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }

    private class PaginationMetrics
    {
        public int ItemsProcessed { get; set; }
        public int PagesCompleted { get; set; }
        public long ProcessingTimeMs { get; set; }
    }
}
