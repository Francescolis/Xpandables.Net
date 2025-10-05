/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
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

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

public class AsyncPagedEnumerableWindowingExtensionsTests
{
    private record TestItem(int Value, double Price, string Name);

    private static IAsyncPagedEnumerable<TestItem> CreateTestData()
    {
        var items = new[]
        {
            new TestItem(10, 100.0, "A"),
            new TestItem(20, 200.0, "B"),
            new TestItem(30, 300.0, "C"),
            new TestItem(40, 400.0, "D"),
            new TestItem(50, 500.0, "E")
        };
        return new AsyncPagedEnumerable<TestItem, TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(5, 1, totalCount: 5)));
    }

    private static IAsyncPagedEnumerable<TestItem> CreateEmptyTestData()
    {
        return new AsyncPagedEnumerable<TestItem, TestItem>(
            AsyncEnumerable.Empty<TestItem>(),
            ct => ValueTask.FromResult(Pagination.Create(0, 0, totalCount: 0)));
    }

    private static IAsyncPagedEnumerable<TestItem> CreateSingleItemData()
    {
        var items = new[] { new TestItem(42, 420.0, "Single") };
        return new AsyncPagedEnumerable<TestItem, TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(1, 1, totalCount: 1)));
    }

    #region Window Tests

    [Fact]
    public async Task WindowPaged_WithValidWindowSize_ReturnsCorrectWindows()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var windows = await source.WindowPaged(3).ToListPagedAsync();

        // Assert
        windows.Should().HaveCount(3);

        windows[0].Should().BeEquivalentTo([
            new TestItem(10, 100.0, "A"),
            new TestItem(20, 200.0, "B"),
            new TestItem(30, 300.0, "C")
        ]);

        windows[1].Should().BeEquivalentTo([
            new TestItem(20, 200.0, "B"),
            new TestItem(30, 300.0, "C"),
            new TestItem(40, 400.0, "D")
        ]);

        windows[2].Should().BeEquivalentTo([
            new TestItem(30, 300.0, "C"),
            new TestItem(40, 400.0, "D"),
            new TestItem(50, 500.0, "E")
        ]);
    }

    [Fact]
    public async Task WindowPaged_WithWindowSizeLargerThanSource_ReturnsPartialWindows()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var windows = await source.WindowPaged(10).ToListPagedAsync();

        // Assert
        windows.Should().HaveCount(1);
        windows[0].Should().HaveCount(5);
    }

    [Fact]
    public async Task WindowPaged_WithWindowSizeOne_ReturnsIndividualItems()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var windows = await source.WindowPaged(1).ToListPagedAsync();

        // Assert
        windows.Should().HaveCount(5);
        windows.Select(w => w.Single().Value).Should().BeEquivalentTo([10, 20, 30, 40, 50]);
    }

    [Fact]
    public async Task WindowPaged_WithEmptySource_ReturnsEmpty()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var windows = await source.WindowPaged(3).ToListPagedAsync();

        // Assert
        windows.Should().BeEmpty();
    }

    [Fact]
    public async Task WindowPaged_WithZeroWindowSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await source.WindowPaged(0).ToListPagedAsync());
    }

    [Fact]
    public async Task WindowPaged_WithNegativeWindowSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await source.WindowPaged(-1).ToListPagedAsync());
    }

    #endregion

    #region WindowedSum Tests

    [Fact]
    public async Task WindowedSumPaged_WithIntSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var sums = await source.WindowedSumPaged(3, x => x.Value).ToListPagedAsync();

        // Assert
        sums.Should().BeEquivalentTo([60, 90, 120]); // [10+20+30, 20+30+40, 30+40+50]
    }

    [Fact]
    public async Task WindowedSumPaged_WithLongSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var sums = await source.WindowedSumPaged(2, x => (long)x.Value).ToListPagedAsync();

        // Assert
        sums.Should().BeEquivalentTo([30L, 50L, 70L, 90L]); // [10+20, 20+30, 30+40, 40+50]
    }

    [Fact]
    public async Task WindowedSumPaged_WithDoubleSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var sums = await source.WindowedSumPaged(2, x => x.Price).ToListPagedAsync();

        // Assert
        sums.Should().BeEquivalentTo([300.0, 500.0, 700.0, 900.0]); // [100+200, 200+300, 300+400, 400+500]
    }

    [Fact]
    public async Task WindowedSumPaged_WithSingleElement_ReturnsElement()
    {
        // Arrange
        var source = CreateSingleItemData();

        // Act
        var sums = await source.WindowedSumPaged(1, x => x.Value).ToListPagedAsync();

        // Assert
        sums.Should().BeEquivalentTo([42]);
    }

    #endregion

    #region WindowedAverage Tests

    [Fact]
    public async Task WindowedAveragePaged_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var averages = await source.WindowedAveragePaged(3, x => x.Price).ToListPagedAsync();

        // Assert
        averages.Should().BeEquivalentTo([200.0, 300.0, 400.0]); // [600/3, 900/3, 1200/3]
    }

    [Fact]
    public async Task WindowedAveragePaged_WithSingleElement_ReturnsElement()
    {
        // Arrange
        var source = CreateSingleItemData();

        // Act
        var averages = await source.WindowedAveragePaged(1, x => x.Price).ToListPagedAsync();

        // Assert
        averages.Should().BeEquivalentTo([420.0]);
    }

    #endregion

    #region WindowedMin/Max Tests

    [Fact]
    public async Task WindowedMinPaged_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var mins = await source.WindowedMinPaged(3, x => x.Value).ToListPagedAsync();

        // Assert
        mins.Should().BeEquivalentTo([10, 20, 30]); // [min(10,20,30), min(20,30,40), min(30,40,50)]
    }

    [Fact]
    public async Task WindowedMaxPaged_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var maxs = await source.WindowedMaxPaged(3, x => x.Value).ToListPagedAsync();

        // Assert
        maxs.Should().BeEquivalentTo([30, 40, 50]); // [max(10,20,30), max(20,30,40), max(30,40,50)]
    }

    [Fact]
    public async Task WindowedMinPaged_WithStrings_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var mins = await source.WindowedMinPaged(2, x => x.Name).ToListPagedAsync();

        // Assert
        mins.Should().BeEquivalentTo(["A", "B", "C", "D"]); // [min(A,B), min(B,C), min(C,D), min(D,E)]
    }

    #endregion

    #region Pairwise Tests

    [Fact]
    public async Task PairwisePaged_WithoutSelector_ReturnsPairs()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var pairs = await source.PairwisePaged().ToListPagedAsync();

        // Assert
        pairs.Should().HaveCount(4);
        pairs[0].Should().Be((new TestItem(10, 100.0, "A"), new TestItem(20, 200.0, "B")));
        pairs[1].Should().Be((new TestItem(20, 200.0, "B"), new TestItem(30, 300.0, "C")));
        pairs[2].Should().Be((new TestItem(30, 300.0, "C"), new TestItem(40, 400.0, "D")));
        pairs[3].Should().Be((new TestItem(40, 400.0, "D"), new TestItem(50, 500.0, "E")));
    }

    [Fact]
    public async Task PairwisePaged_WithSelector_AppliesFunction()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var differences = await source.PairwisePaged((prev, curr) => curr.Value - prev.Value).ToListPagedAsync();

        // Assert
        differences.Should().BeEquivalentTo([10, 10, 10, 10]); // Each consecutive pair has difference of 10
    }

    [Fact]
    public async Task PairwisePaged_WithSingleElement_ReturnsEmpty()
    {
        // Arrange
        var source = CreateSingleItemData();

        // Act
        var pairs = await source.PairwisePaged().ToListPagedAsync();

        // Assert
        pairs.Should().BeEmpty();
    }

    [Fact]
    public async Task PairwisePaged_WithEmptySource_ReturnsEmpty()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var pairs = await source.PairwisePaged().ToListPagedAsync();

        // Assert
        pairs.Should().BeEmpty();
    }

    #endregion

    #region Scan Tests

    [Fact]
    public async Task ScanPaged_WithSeed_ReturnsRunningAccumulation()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var running = await source.ScanPaged(0, (acc, item) => acc + item.Value).ToListPagedAsync();

        // Assert
        running.Should().BeEquivalentTo([0, 10, 30, 60, 100, 150]); // [seed, 0+10, 10+20, 30+30, 60+40, 100+50]
    }

    [Fact]
    public async Task ScanPaged_WithoutSeed_ReturnsRunningAccumulation()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var running = await source.ScanPaged((prev, curr) => new TestItem(prev.Value + curr.Value, prev.Price + curr.Price, prev.Name + curr.Name)).ToListPagedAsync();

        // Assert
        running.Should().HaveCount(5);
        running[0].Value.Should().Be(10);   // First element
        running[1].Value.Should().Be(30);   // 10 + 20
        running[2].Value.Should().Be(60);   // 30 + 30
        running[3].Value.Should().Be(100);  // 60 + 40
        running[4].Value.Should().Be(150);  // 100 + 50
    }

    [Fact]
    public async Task ScanPaged_WithEmptySource_ReturnsOnlySeed()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var running = await source.ScanPaged(42, (acc, item) => acc + item.Value).ToListPagedAsync();

        // Assert
        running.Should().BeEquivalentTo([42]);
    }

    [Fact]
    public async Task ScanPaged_WithoutSeedEmptySource_ReturnsEmpty()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var running = await source.ScanPaged((prev, curr) => prev).ToListPagedAsync();

        // Assert
        running.Should().BeEmpty();
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public async Task WindowingMethods_WithNullSource_ThrowArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerable<TestItem> source = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.WindowPaged(3).ToListPagedAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.WindowedSumPaged(3, x => x.Value).ToListPagedAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.PairwisePaged().ToListPagedAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.ScanPaged(0, (acc, x) => acc + x.Value).ToListPagedAsync());
    }

    [Fact]
    public async Task WindowingMethods_WithNullSelector_ThrowArgumentNullException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.WindowedSumPaged(3, (Func<TestItem, int>)null!).ToListPagedAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.PairwisePaged((Func<TestItem, TestItem, TestItem>)null!).ToListPagedAsync());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.ScanPaged(0, (Func<int, TestItem, int>)null!).ToListPagedAsync());
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task WindowingMethods_WithCancellation_ThrowOperationCanceledException()
    {
        // Arrange
        var source = CreateTestData();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.WindowPaged(3).ToListPagedAsync(cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.WindowedSumPaged(3, x => x.Value).ToListPagedAsync(cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.PairwisePaged().ToListPagedAsync(cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.ScanPaged(0, (acc, x) => acc + x.Value).ToListPagedAsync(cts.Token));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task WindowingOperations_PreservePageContext()
    {
        // Arrange
        var source = CreateTestData();
        var originalPageContext = await source.GetPaginationAsync();

        // Act
        var windowed = source.WindowPaged(3);
        var windowedContext = await windowed.GetPaginationAsync();

        // Assert
        windowedContext.PageSize.Should().Be(originalPageContext.PageSize);
        windowedContext.CurrentPage.Should().Be(originalPageContext.CurrentPage);
        windowedContext.TotalCount.Should().Be(originalPageContext.TotalCount);
    }

    [Fact]
    public async Task WindowingOperations_ChainedWithOtherOperations_WorkCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source
            .WherePaged(x => x.Value > 15)
            .WindowedSumPaged(2, x => x.Value)
            .ToListPagedAsync();

        // Assert
        result.Should().BeEquivalentTo([50, 70, 90]); // Filtered: [20,30,40,50], Windows: [20+30, 30+40, 40+50]
    }

    [Fact]
    public async Task ScanPaged_WithComplexAccumulation_WorksCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.ScanPaged(
            new { Sum = 0, Count = 0 },
            (acc, item) => new { Sum = acc.Sum + item.Value, Count = acc.Count + 1 }
        ).ToListPagedAsync();

        // Assert
        result.Should().HaveCount(6); // Including seed
        result[^1].Sum.Should().Be(150); // Final sum
        result[^1].Count.Should().Be(5); // Final count
    }

    [Fact]
    public async Task WindowingOperations_WithLargeDataset_PerformEfficiently()
    {
        // Arrange
        var items = Enumerable.Range(1, 1000)
            .Select(i => new TestItem(i, i * 1.5, $"Item{i}"))
            .ToArray();
        var source = new AsyncPagedEnumerable<TestItem, TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(1000, 1, totalCount: 1000)));

        // Act
        var windowedSums = await source.WindowedSumPaged(10, x => x.Value).ToListPagedAsync();
        var pairwiseDiffs = await source.PairwisePaged((prev, curr) => curr.Value - prev.Value).ToListPagedAsync();

        // Assert
        windowedSums.Should().HaveCount(991); // 1000 - 10 + 1
        pairwiseDiffs.Should().HaveCount(999); // 1000 - 1
        pairwiseDiffs.All(diff => diff == 1).Should().BeTrue(); // All differences should be 1
    }

    [Fact]
    public async Task WindowPaged_WithVaryingWindowSizes_HandlesEdgeCases()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        var windows1 = await source.WindowPaged(1).ToListPagedAsync();
        windows1.Should().HaveCount(5);

        var windows5 = await source.WindowPaged(5).ToListPagedAsync();
        windows5.Should().HaveCount(1);
        windows5[0].Should().HaveCount(5);

        var windows6 = await source.WindowPaged(6).ToListPagedAsync();
        windows6.Should().HaveCount(1);
        windows6[0].Should().HaveCount(5); // Window larger than source
    }

    #endregion
}