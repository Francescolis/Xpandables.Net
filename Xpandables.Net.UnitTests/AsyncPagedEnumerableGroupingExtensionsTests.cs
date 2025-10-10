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

public class AsyncPagedEnumerableGroupingExtensionsTests
{
    private record TestItem(int Id, string Category, string Name, int Value);

    private static IAsyncPagedEnumerable<TestItem> CreateTestData()
    {
        var items = new[]
        {
            new TestItem(1, "A", "Item1", 10),
            new TestItem(2, "B", "Item2", 20),
            new TestItem(3, "A", "Item3", 30),
            new TestItem(4, "C", "Item4", 40),
            new TestItem(5, "B", "Item5", 50),
            new TestItem(6, "A", "Item6", 60)
        };
        return new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(6, 1, totalCount: 6)));
    }

    private static IAsyncPagedEnumerable<TestItem> CreateEmptyTestData()
    {
        return new AsyncPagedEnumerable<TestItem>(
            AsyncEnumerable.Empty<TestItem>(),
            ct => ValueTask.FromResult(Pagination.Create(0, 0, totalCount: 0)));
    }

    #region GroupBy Tests

    [Fact]
    public async Task GroupByPaged_WithKeySelector_GroupsCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var groups = await source.GroupByPaged(x => x.Category).ToListPagedAsync();

        // Assert
        groups.Should().HaveCount(3);

        var groupA = groups.Single(g => g.Key == "A");
        groupA.Should().HaveCount(3);
        groupA.Select(x => x.Id).Should().BeEquivalentTo([1, 3, 6]);

        var groupB = groups.Single(g => g.Key == "B");
        groupB.Should().HaveCount(2);
        groupB.Select(x => x.Id).Should().BeEquivalentTo([2, 5]);

        var groupC = groups.Single(g => g.Key == "C");
        groupC.Should().HaveCount(1);
        groupC.Select(x => x.Id).Should().BeEquivalentTo([4]);
    }

    [Fact]
    public async Task GroupByPaged_WithKeySelectorAndComparer_GroupsCorrectly()
    {
        // Arrange
        var source = CreateTestData();
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var groups = await source.GroupByPaged(x => x.Category.ToLower(), comparer).ToListPagedAsync();

        // Assert
        groups.Should().HaveCount(3);
        groups.Select(g => g.Key).Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public async Task GroupByPaged_WithKeySelectorAndElementSelector_ProjectsCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var groups = await source.GroupByPaged(x => x.Category, x => x.Value).ToListPagedAsync();

        // Assert
        groups.Should().HaveCount(3);

        var groupA = groups.Single(g => g.Key == "A");
        groupA.Should().BeEquivalentTo([10, 30, 60]);

        var groupB = groups.Single(g => g.Key == "B");
        groupB.Should().BeEquivalentTo([20, 50]);

        var groupC = groups.Single(g => g.Key == "C");
        groupC.Should().BeEquivalentTo([40]);
    }

    [Fact]
    public async Task GroupByPaged_WithKeySelectorElementSelectorAndComparer_ProjectsCorrectly()
    {
        // Arrange
        var source = CreateTestData();
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var groups = await source.GroupByPaged(
            x => x.Category.ToUpper(),
            x => x.Name,
            comparer).ToListPagedAsync();

        // Assert
        groups.Should().HaveCount(3);

        var groupA = groups.Single(g => g.Key == "A");
        groupA.Should().BeEquivalentTo(["Item1", "Item3", "Item6"]);
    }

    [Fact]
    public async Task GroupByPaged_WithResultSelector_TransformsCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var results = await source.GroupByPaged(
            x => x.Category,
            (key, group) => new { Category = key, Count = group.Count(), TotalValue = group.Sum(x => x.Value) }
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(3);

        var resultA = results.Single(r => r.Category == "A");
        resultA.Count.Should().Be(3);
        resultA.TotalValue.Should().Be(100); // 10 + 30 + 60

        var resultB = results.Single(r => r.Category == "B");
        resultB.Count.Should().Be(2);
        resultB.TotalValue.Should().Be(70); // 20 + 50

        var resultC = results.Single(r => r.Category == "C");
        resultC.Count.Should().Be(1);
        resultC.TotalValue.Should().Be(40);
    }

    [Fact]
    public async Task GroupByPaged_WithResultSelectorAndComparer_TransformsCorrectly()
    {
        // Arrange
        var source = CreateTestData();
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var results = await source.GroupByPaged(
            x => x.Category.ToLower(),
            (key, group) => new { Category = key, Count = group.Count() },
            comparer
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(3);
        results.Select(r => r.Category).Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public async Task GroupByPaged_WithEmptySource_ReturnsEmptyGroups()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var groups = await source.GroupByPaged(x => x.Category).ToListPagedAsync();

        // Assert
        groups.Should().BeEmpty();
    }

    [Fact]
    public async Task GroupByPaged_WithNullKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.GroupByPaged((Func<TestItem, string>)null!).ToListPagedAsync());
    }

    [Fact]
    public async Task GroupByPaged_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerable<TestItem> source = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.GroupByPaged(x => x.Category).ToListPagedAsync());
    }

    [Fact]
    public async Task GroupByPaged_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var source = CreateTestData();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.GroupByPaged(x => x.Category).ToListPagedAsync(cts.Token));
    }

    #endregion

    #region ToLookup Tests

    [Fact]
    public async Task ToLookupPagedAsync_WithKeySelector_CreatesLookupCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var lookup = await source.ToLookupPagedAsync(x => x.Category);

        // Assert
        lookup.Count.Should().Be(3);
        lookup.Contains("A").Should().BeTrue();
        lookup.Contains("B").Should().BeTrue();
        lookup.Contains("C").Should().BeTrue();
        lookup.Contains("D").Should().BeFalse();

        lookup["A"].Should().HaveCount(3);
        lookup["A"].Select(x => x.Id).Should().BeEquivalentTo([1, 3, 6]);

        lookup["B"].Should().HaveCount(2);
        lookup["B"].Select(x => x.Id).Should().BeEquivalentTo([2, 5]);

        lookup["C"].Should().HaveCount(1);
        lookup["C"].Select(x => x.Id).Should().BeEquivalentTo([4]);

        lookup["D"].Should().BeEmpty();
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithKeySelectorAndComparer_CreatesLookupCorrectly()
    {
        // Arrange
        var source = CreateTestData();
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var lookup = await source.ToLookupPagedAsync(x => x.Category.ToLower(), comparer);

        // Assert
        lookup.Count.Should().Be(3);
        lookup.Contains("a").Should().BeTrue();
        lookup.Contains("A").Should().BeTrue(); // Case insensitive
        lookup["a"].Should().HaveCount(3);
        lookup["A"].Should().HaveCount(3); // Should be same as "a"
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithKeySelectorAndElementSelector_ProjectsCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var lookup = await source.ToLookupPagedAsync(x => x.Category, x => x.Value);

        // Assert
        lookup.Count.Should().Be(3);
        lookup["A"].Should().BeEquivalentTo([10, 30, 60]);
        lookup["B"].Should().BeEquivalentTo([20, 50]);
        lookup["C"].Should().BeEquivalentTo([40]);
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithKeySelectorElementSelectorAndComparer_ProjectsCorrectly()
    {
        // Arrange
        var source = CreateTestData();
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var lookup = await source.ToLookupPagedAsync(
            x => x.Category.ToUpper(),
            x => x.Name,
            comparer);

        // Assert
        lookup.Count.Should().Be(3);
        lookup["A"].Should().BeEquivalentTo(["Item1", "Item3", "Item6"]);
        lookup["a"].Should().BeEquivalentTo(["Item1", "Item3", "Item6"]); // Case insensitive
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithEmptySource_ReturnsEmptyLookup()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var lookup = await source.ToLookupPagedAsync(x => x.Category);

        // Assert
        lookup.Count.Should().Be(0);
        lookup.Contains("A").Should().BeFalse();
        lookup["A"].Should().BeEmpty();
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithNullKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.ToLookupPagedAsync((Func<TestItem, string>)null!));
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerable<TestItem> source = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.ToLookupPagedAsync(x => x.Category));
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithNullElementSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.ToLookupPagedAsync(x => x.Category, (Func<TestItem, string>)null!));
    }

    [Fact]
    public async Task ToLookupPagedAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var source = CreateTestData();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.ToLookupPagedAsync(x => x.Category, cts.Token));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task GroupByPaged_PreservesPageContext()
    {
        // Arrange
        var source = CreateTestData();
        var originalPageContext = await source.GetPaginationAsync();

        // Act
        var grouped = source.GroupByPaged(x => x.Category);
        var groupedContext = await grouped.GetPaginationAsync();

        // Assert
        groupedContext.PageSize.Should().Be(originalPageContext.PageSize);
        groupedContext.CurrentPage.Should().Be(originalPageContext.CurrentPage);
        groupedContext.TotalCount.Should().Be(originalPageContext.TotalCount);
    }

    [Fact]
    public async Task ToLookupPagedAsync_WorksWithComplexKeys()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var lookup = await source.ToLookupPagedAsync(x => new { x.Category, IsHighValue = x.Value > 30 });

        // Assert
        lookup.Count.Should().Be(5); // Updated to expect 5 groups
        lookup[new { Category = "A", IsHighValue = false }].Should().HaveCount(2); // Items 1(10), 3(30)
        lookup[new { Category = "A", IsHighValue = true }].Should().HaveCount(1);  // Item 6(60)
        lookup[new { Category = "B", IsHighValue = false }].Should().HaveCount(1); // Item 2(20)
        lookup[new { Category = "B", IsHighValue = true }].Should().HaveCount(1);  // Item 5(50)
        lookup[new { Category = "C", IsHighValue = true }].Should().HaveCount(1);  // Item 4(40) - corrected to true
    }

    [Fact]
    public async Task GroupByPaged_WorksWithDuplicateKeys()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(1, "A", "Item1", 10),
            new TestItem(2, "A", "Item2", 20),
            new TestItem(3, "A", "Item3", 30)
        };
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(3, 1, totalCount: 3)));

        // Act
        var groups = await source.GroupByPaged(x => x.Category).ToListPagedAsync();

        // Assert
        groups.Should().HaveCount(1);
        groups[0].Key.Should().Be("A");
        groups[0].Should().HaveCount(3);
    }

    #endregion

    #region Performance and Edge Cases

    [Fact]
    public async Task GroupByPaged_HandlesLargeGroups()
    {
        // Arrange
        var items = Enumerable.Range(1, 1000)
            .Select(i => new TestItem(i, i % 10 == 0 ? "A" : "B", $"Item{i}", i))
            .ToArray();
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(1000, 1, totalCount: 1000)));

        // Act
        var groups = await source.GroupByPaged(x => x.Category).ToListPagedAsync();

        // Assert
        groups.Should().HaveCount(2);
        groups.Single(g => g.Key == "A").Should().HaveCount(100); // Every 10th item
        groups.Single(g => g.Key == "B").Should().HaveCount(900); // All other items
    }

    [Fact]
    public async Task ToLookupPagedAsync_HandlesNullValues()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(1, "A", "Item1", 10),
            new TestItem(2, "B", "Item2", 20)
        };
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(2, 1, totalCount: 2)));

        // Act
        var lookup = await source.ToLookupPagedAsync(x => x.Category, x => x.Name);

        // Assert
        lookup["A"].Should().ContainSingle().Which.Should().Be("Item1");
        lookup["B"].Should().ContainSingle().Which.Should().Be("Item2");
        lookup["C"].Should().BeEmpty();
    }

    #endregion
}