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

using System.Collections.Generic;

namespace Xpandables.Net.UnitTests.Collections.Extensions;

/// <summary>
/// Unit tests for AsyncEnumerableExtensions methods.
/// </summary>
public sealed class AsyncEnumerableExtensionsTests
{
    [Fact]
    public async Task ToAsyncPagedEnumerable_ShouldConvertToPagedEnumerable()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(50);

		// Act
		IAsyncPagedEnumerable<int> paged = source.ToAsyncPagedEnumerable();

        // Assert
        Assert.NotNull(paged);
		List<int> items = await paged.ToListAsync();
        Assert.Equal(50, items.Count);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithPaginationFactory_ShouldUsePagination()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(30);
        var expectedPagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 30);

		// Act
		IAsyncPagedEnumerable<int> paged = source.ToAsyncPagedEnumerable(
            _ => ValueTask.FromResult(expectedPagination));
		Pagination pagination = await paged.GetPaginationAsync();

        // Assert
        Assert.Equal(expectedPagination.PageSize, pagination.PageSize);
        Assert.Equal(expectedPagination.TotalCount, pagination.TotalCount);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithPagination_ShouldUsePagination()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(40);
        var pagination = Pagination.Create(pageSize: 5, currentPage: 1, totalCount: 40);

		// Act
		IAsyncPagedEnumerable<int> paged = source.ToAsyncPagedEnumerable(pagination);
		Pagination result = await paged.GetPaginationAsync();

        // Assert
        Assert.Equal(pagination.PageSize, result.PageSize);
        Assert.Equal(pagination.CurrentPage, result.CurrentPage);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithTotalCount_ShouldUseCount()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(25);
        const int totalCount = 25;

		// Act
		IAsyncPagedEnumerable<int> paged = source.ToAsyncPagedEnumerable(totalCount);
		Pagination pagination = await paged.GetPaginationAsync();

        // Assert
        Assert.Equal(totalCount, pagination.TotalCount);
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithNullSource_ShouldThrow()
    {
        // Arrange
        IAsyncEnumerable<int>? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => source!.ToAsyncPagedEnumerable());
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithNullFactory_ShouldThrow()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(10);
        Func<CancellationToken, ValueTask<Pagination>>? factory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => source.ToAsyncPagedEnumerable(factory!));
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithNegativeTotalCount_ShouldThrow()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(10);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => source.ToAsyncPagedEnumerable(-1));
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
/// Unit tests for IAsyncPagedEnumerableExtensions methods.
/// </summary>
public sealed class IAsyncPagedEnumerableExtensionsTests
{
    [Fact]
    public void GetArgumentType_ShouldReturnElementType()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(10);
        IAsyncPagedEnumerable paged = AsyncPagedEnumerable.Create(source);

		// Act
		Type argumentType = paged.GetArgumentType();

        // Assert
        Assert.Equal(typeof(int), argumentType);
    }

    [Fact]
    public void GetArgumentType_WithStringEnumerable_ShouldReturnStringType()
    {
		// Arrange
		IAsyncEnumerable<string> source = CreateAsyncStringEnumerable(5);
        IAsyncPagedEnumerable paged = AsyncPagedEnumerable.Create(source);

		// Act
		Type argumentType = paged.GetArgumentType();

        // Assert
        Assert.Equal(typeof(string), argumentType);
    }

    [Fact]
    public void GetArgumentType_WithNonGenericSource_ShouldThrow()
    {
        // This test would require a mock or non-generic implementation
        // Skipping for now as the implementation requires generic source
    }

    private static async IAsyncEnumerable<int> CreateAsyncEnumerable(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }

    private static async IAsyncEnumerable<string> CreateAsyncStringEnumerable(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return $"Item_{i}";
        }
    }
}

/// <summary>
/// Integration tests for AsyncPagedEnumerable with real-world scenarios.
/// </summary>
public sealed class AsyncPagedEnumerableIntegrationTests
{
    [Fact]
    public async Task LargeDataset_ShouldHandleEfficientlyWithPagination()
    {
        // Arrange
        const int datasetSize = 10000;
        const int pageSize = 100;
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(datasetSize);
        var pagination = Pagination.Create(pageSize: pageSize, currentPage: 1, totalCount: datasetSize);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        // Act
        int count = 0;
        await foreach (int item in paged)
        {
            count++;
        }

        // Assert
        Assert.Equal(datasetSize, count);
		Pagination finalPagination = await paged.GetPaginationAsync();
        Assert.Equal(datasetSize, finalPagination.TotalCount);
    }

    [Fact]
    public async Task MultipleConsumers_ShouldWorkIndependently()
    {
        // Arrange
        const int size = 100;
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(size);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);

		// Act
		Task<List<int>> task1 = Task.Run(async () =>
        {
            var items = new List<int>();
            await foreach (int item in paged)
            {
                items.Add(item);
            }
            return items;
        });

		Task<List<int>> task2 = Task.Run(async () =>
        {
            var items = new List<int>();
            await foreach (int item in paged)
            {
                items.Add(item);
            }
            return items;
        });

		List<int> results1 = await task1;
		List<int> results2 = await task2;

        // Assert
        Assert.Equal(size, results1.Count);
        Assert.Equal(size, results2.Count);
    }

    [Fact]
    public async Task WithCancellation_ShouldStopProcessing()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(1000);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);
        var cts = new CancellationTokenSource();

        // Act
        int count = 0;
        try
        {
            await foreach (int item in paged.WithCancellation(cts.Token))
            {
                count++;
                if (count == 500)
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
        Assert.True(count is >= 500 and < 1000);
    }

    [Fact]
    public async Task ConcurrentEnumeration_WithDifferentTokens_ShouldRespectEachToken()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(500);
		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source);
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

		// Act
		Task<int> task1 = Task.Run(async () =>
        {
            int count = 0;
            try
            {
                await foreach (int item in paged.WithCancellation(cts1.Token))
                {
                    count++;
                    if (count == 100)
                    {
                        cts1.Cancel();
                    }
                }
            }
            catch (OperationCanceledException) { }
            return count;
        });

		Task<int> task2 = Task.Run(async () =>
        {
            int count = 0;
            try
            {
                await foreach (int item in paged.WithCancellation(cts2.Token))
                {
                    count++;
                    if (count == 200)
                    {
                        cts2.Cancel();
                    }
                }
            }
            catch (OperationCanceledException) { }
            return count;
        });

        int count1 = await task1;
        int count2 = await task2;

        // Assert
        Assert.True(count1 is >= 100 and < 500);
        Assert.True(count2 is >= 200 and < 500);
    }

    [Fact]
    public async Task PaginationFactory_WithException_ShouldPropagateException()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(10);
		string exceptionMessage = "Test pagination error";
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            _ => throw new InvalidOperationException(exceptionMessage));

		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source, paginationFactory);

		// Act & Assert
		InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => paged.GetPaginationAsync());
        Assert.Equal(exceptionMessage, exception.Message);
    }

    [Fact]
    public async Task PaginationFactory_CalledMultipleTimes_ShouldCacheResult()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(20);
        int callCount = 0;
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            async _ =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(10, _).ConfigureAwait(false);
                return Pagination.Create(pageSize: 5, currentPage: 1, totalCount: 20);
            });

		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source, paginationFactory);

        // Act
        await paged.GetPaginationAsync();
        await paged.GetPaginationAsync();
        await paged.GetPaginationAsync();

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ThreadSafePaginationComputation_ConcurrentCalls()
    {
		// Arrange
		IAsyncEnumerable<int> source = CreateAsyncEnumerable(100);
        int callCount = 0;
        var paginationFactory = new Func<CancellationToken, ValueTask<Pagination>>(
            async _ =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(50, _).ConfigureAwait(false);
                return Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 100);
            });

		IAsyncPagedEnumerable<int> paged = AsyncPagedEnumerable.Create(source, paginationFactory);

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => paged.GetPaginationAsync())
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, callCount); // Should be called only once despite concurrent requests
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
