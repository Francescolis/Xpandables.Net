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
/// Performance and stress tests for IAsyncPagedEnumerable.
/// </summary>
public sealed class AsyncPagedEnumerablePerformanceTests
{
    /// <summary>
    /// Tests memory efficiency with large datasets.
    /// </summary>
    [Fact]
    public async Task LargeDataset_ShouldNotCauseMemoryLeaks()
    {
        // Arrange
        const int largeSize = 100000;
        var source = GenerateStreamData(largeSize);
        var pagination = Pagination.Create(pageSize: 1000, currentPage: 1, totalCount: largeSize);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        var initialMemory = GC.GetTotalMemory(false);

        // Act
        int count = 0;
        await foreach (var item in paged)
        {
            count++;
            if (count % 10000 == 0)
                GC.Collect(0, GCCollectionMode.Optimized);
        }

        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        Assert.Equal(largeSize, count);
        // Memory increase should be reasonable (not linear to dataset size)
        Assert.True(memoryIncrease < largeSize * sizeof(int) / 10);
    }

    /// <summary>
    /// Tests pagination factory performance under concurrent load.
    /// </summary>
    [Fact]
    public async Task ConcurrentPaginationComputation_ShouldSerializeAccess()
    {
        // Arrange
        var source = GenerateStreamData(100);
        int computationCount = 0;
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            async _ =>
            {
                Interlocked.Increment(ref computationCount);
                await Task.Delay(10, _).ConfigureAwait(false);
                return Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);
            });

        var paged = AsyncPagedEnumerable.Create(source, paginationFactory);

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => paged.GetPaginationAsync())
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, computationCount); // Should compute only once
    }

    /// <summary>
    /// Tests enumeration speed with different pagination strategies.
    /// </summary>
    [Fact]
    public async Task EnumerationSpeed_WithStrategyPerPage()
    {
        // Arrange
        const int size = 50000;
        var source = GenerateStreamData(size);
        var pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: size);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        var startTime = DateTime.UtcNow;

        // Act
        int count = 0;
        await foreach (var item in paged)
        {
            count++;
        }

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(size, count);
        Assert.True(elapsed.TotalMilliseconds < 5000); // Should complete in reasonable time
    }

    /// <summary>
    /// Tests enumeration with high concurrency.
    /// </summary>
    [Fact]
    public async Task HighConcurrency_MultipleConsumers_ShouldHandleWell()
    {
        // Arrange
        const int dataSize = 10000;
        const int consumerCount = 10;
        var source = GenerateStreamData(dataSize);
        var pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: dataSize);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        // Act
        var tasks = Enumerable.Range(0, consumerCount)
            .Select(async _ =>
            {
                var items = new List<int>();
                await foreach (var item in paged)
                {
                    items.Add(item);
                }
                return items.Count;
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, count => Assert.Equal(dataSize, count));
    }

    /// <summary>
    /// Tests rapid pagination object creation and disposal.
    /// </summary>
    [Fact]
    public async Task RapidCreation_ManyPagedEnumerables_ShouldNotLeakResources()
    {
        // Arrange
        const int iterations = 1000;
        const int itemsPerIteration = 100;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var source = GenerateStreamData(itemsPerIteration);
            var pagination = Pagination.Create(
                pageSize: 10,
                currentPage: 1,
                totalCount: itemsPerIteration);
            var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

            var count = 0;
            await foreach (var item in paged)
            {
                count++;
            }

            Assert.Equal(itemsPerIteration, count);

            // Dispose explicitly
            if (paged is IDisposable disposable)
                disposable.Dispose();
        }

        // Assert - if we got here without OOM, test passed
        Assert.True(true);
    }

    /// <summary>
    /// Tests behavior under rapid token cancellation.
    /// </summary>
    [Fact]
    public async Task RapidCancellation_MultipleSources()
    {
        // Arrange
        const int iterations = 100;
        int successfulCancellations = 0;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var source = GenerateStreamData(1000);
            var paged = AsyncPagedEnumerable.Create(source);
            var cts = new CancellationTokenSource();

            try
            {
                cts.CancelAfter(TimeSpan.FromMilliseconds(1));
                int count = 0;
                await foreach (var item in paged.WithCancellation(cts.Token))
                {
                    count++;
                }
            }
            catch (OperationCanceledException)
            {
                Interlocked.Increment(ref successfulCancellations);
            }
            finally
            {
                cts.Dispose();
            }
        }

        // Assert
        Assert.True(successfulCancellations > 0);
    }

    /// <summary>
    /// Tests enumeration with frequent garbage collection.
    /// </summary>
    [Fact]
    public async Task FrequentGarbageCollection_ShouldNotAffectEnumeration()
    {
        // Arrange
        const int size = 50000;
        var source = GenerateStreamData(size);
        var pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: size);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        // Act
        int count = 0;
        await foreach (var item in paged)
        {
            count++;
            if (count % 1000 == 0)
            {
                GC.Collect(2, GCCollectionMode.Aggressive);
                GC.WaitForPendingFinalizers();
            }
        }

        // Assert
        Assert.Equal(size, count);
    }

    /// <summary>
    /// Tests edge case with zero-size pagination.
    /// </summary>
    [Fact]
    public async Task ZeroSizePagination_ShouldHandleGracefully()
    {
        // Arrange
        var source = GenerateStreamData(0);
        var pagination = Pagination.Create(pageSize: 0, currentPage: 0, totalCount: 0);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        // Act
        var items = await paged.ToListAsync();

        // Assert
        Assert.Empty(items);
    }

    /// <summary>
    /// Tests task scheduling efficiency.
    /// </summary>
    [Fact]
    public async Task TaskScheduling_LargeNumberOfAwaits_ShouldBeEfficient()
    {
        // Arrange
        const int size = 10000;
        var source = GenerateStreamData(size);
        var pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: size);
        var paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        var startTime = DateTime.UtcNow;

        // Act
        int count = 0;
        int taskCount = 0;
        await foreach (var item in paged)
        {
            count++;
            taskCount++;
            if (taskCount > 1000)
            {
                await Task.Yield();
                taskCount = 0;
            }
        }

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(size, count);
        Assert.True(elapsed.TotalSeconds < 10); // Should complete reasonably fast
    }

    /// <summary>
    /// Tests stress with nested pagination.
    /// </summary>
    [Fact]
    public async Task NestedPagination_ShouldWorkCorrectly()
    {
        // Arrange
        const int outerSize = 100;
        const int innerSize = 50;

        // Act
        var allItems = new List<int>();
        for (int i = 0; i < outerSize; i++)
        {
            var innerSource = GenerateStreamData(innerSize);
            var paged = AsyncPagedEnumerable.Create(innerSource);
            var items = await paged.ToListAsync();
            allItems.AddRange(items);
        }

        // Assert
        Assert.Equal(outerSize * innerSize, allItems.Count);
    }

    // Helper methods
    private static async IAsyncEnumerable<int> GenerateStreamData(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }
}

/// <summary>
/// Stress tests for extreme scenarios.
/// </summary>
public sealed class AsyncPagedEnumerableStressTests
{
    [Fact]
    public async Task PaginationWithDelayedComputation()
    {
        // Arrange
        const int size = 100;
        const int delayMs = 100;
        var source = GenerateStreamData(size);

        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            async ct =>
            {
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
                return Pagination.Create(pageSize: 10, currentPage: 1, totalCount: size);
            });

        var paged = AsyncPagedEnumerable.Create(source, paginationFactory);

        // Act
        var paginationTask1 = paged.GetPaginationAsync();
        var paginationTask2 = paged.GetPaginationAsync();
        var paginationTask3 = paged.GetPaginationAsync();

        var results = await Task.WhenAll(paginationTask1, paginationTask2, paginationTask3);

        // Assert
        Assert.Equal(3, results.Length);
        Assert.All(results, p => Assert.Equal(size, p.TotalCount));
    }

    private static async IAsyncEnumerable<int> GenerateStreamData(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }
}
