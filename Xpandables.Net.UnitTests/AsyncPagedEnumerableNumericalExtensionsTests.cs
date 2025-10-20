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

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.Collections.Generic.Extensions;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

public class AsyncPagedEnumerableNumericalExtensionsTests
{
    private record TestItem(int IntValue, int? NullableIntValue, long LongValue, long? NullableLongValue,
        double DoubleValue, double? NullableDoubleValue, decimal DecimalValue, decimal? NullableDecimalValue);

    private static IAsyncPagedEnumerable<TestItem> CreateTestData()
    {
        var items = new[]
        {
            new TestItem(10, 10, 100L, 100L, 10.5, 10.5, 10.25m, 10.25m),
            new TestItem(20, null, 200L, null, 20.5, null, 20.50m, null),
            new TestItem(30, 30, 300L, 300L, 30.5, 30.5, 30.75m, 30.75m),
            new TestItem(40, 40, 400L, 400L, 40.5, 40.5, 40.00m, 40.00m)
        };
        return new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(4, 1, totalCount: 4)));
    }

    private static IAsyncPagedEnumerable<TestItem> CreateEmptyTestData()
    {
        return new AsyncPagedEnumerable<TestItem>(
            AsyncEnumerable.Empty<TestItem>(),
            ct => ValueTask.FromResult(Pagination.Create(0, 0, totalCount: 0)));
    }

    private static IAsyncPagedEnumerable<TestItem> CreateAllNullTestData()
    {
        var items = new[]
        {
            new TestItem(0, null, 0L, null, 0.0, null, 0m, null),
            new TestItem(0, null, 0L, null, 0.0, null, 0m, null)
        };
        return new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(2, 1, totalCount: 2)));
    }

    #region Sum Tests

    [Fact]
    public async Task SumPagedAsync_WithIntSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.IntValue);

        // Assert
        result.Should().Be(100); // 10 + 20 + 30 + 40
    }

    [Fact]
    public async Task SumPagedAsync_WithNullableIntSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.NullableIntValue);

        // Assert
        result.Should().Be(80); // 10 + null + 30 + 40 = 80
    }

    [Fact]
    public async Task SumPagedAsync_WithNullableIntAllNulls_ReturnsZero()
    {
        // Arrange
        var source = CreateAllNullTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.NullableIntValue);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task SumPagedAsync_WithLongSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.LongValue);

        // Assert
        result.Should().Be(1000L); // 100 + 200 + 300 + 400
    }

    [Fact]
    public async Task SumPagedAsync_WithNullableLongSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.NullableLongValue);

        // Assert
        result.Should().Be(800L); // 100 + null + 300 + 400 = 800
    }

    [Fact]
    public async Task SumPagedAsync_WithDoubleSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.DoubleValue);

        // Assert
        result.Should().BeApproximately(102.0, 0.001); // 10.5 + 20.5 + 30.5 + 40.5
    }

    [Fact]
    public async Task SumPagedAsync_WithNullableDoubleSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.NullableDoubleValue);

        // Assert
        result.Should().BeApproximately(81.5, 0.001); // 10.5 + null + 30.5 + 40.5
    }

    [Fact]
    public async Task SumPagedAsync_WithDecimalSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.DecimalValue);

        // Assert
        result.Should().Be(101.5m); // 10.25 + 20.50 + 30.75 + 40.00
    }

    [Fact]
    public async Task SumPagedAsync_WithNullableDecimalSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.SumPagedAsync(x => x.NullableDecimalValue);

        // Assert
        result.Should().Be(81.0m); // 10.25 + null + 30.75 + 40.00
    }

    [Fact]
    public async Task SumPagedAsync_WithEmptySource_ReturnsZero()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var intResult = await source.SumPagedAsync(x => x.IntValue);
        var longResult = await source.SumPagedAsync(x => x.LongValue);
        var doubleResult = await source.SumPagedAsync(x => x.DoubleValue);
        var decimalResult = await source.SumPagedAsync(x => x.DecimalValue);

        // Assert
        intResult.Should().Be(0);
        longResult.Should().Be(0L);
        doubleResult.Should().Be(0.0);
        decimalResult.Should().Be(0m);
    }

    [Fact]
    public async Task SumPagedAsync_WithOverflow_ThrowsOverflowException()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(int.MaxValue, null, 0L, null, 0.0, null, 0m, null),
            new TestItem(1, null, 0L, null, 0.0, null, 0m, null)
        };
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(2, 1, totalCount: 2)));

        // Act & Assert
        await Assert.ThrowsAsync<OverflowException>(async () =>
            await source.SumPagedAsync(x => x.IntValue));
    }

    #endregion

    #region Average Tests

    [Fact]
    public async Task AveragePagedAsync_WithIntSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.IntValue);

        // Assert
        result.Should().BeApproximately(25.0, 0.001); // (10 + 20 + 30 + 40) / 4
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullableIntSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.NullableIntValue);

        // Assert
        result.Should().BeApproximately(26.667, 0.001); // (10 + 30 + 40) / 3
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullableIntAllNulls_ReturnsNull()
    {
        // Arrange
        var source = CreateAllNullTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.NullableIntValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AveragePagedAsync_WithLongSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.LongValue);

        // Assert
        result.Should().BeApproximately(250.0, 0.001); // (100 + 200 + 300 + 400) / 4
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullableLongSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.NullableLongValue);

        // Assert
        result.Should().BeApproximately(266.667, 0.001); // (100 + 300 + 400) / 3
    }

    [Fact]
    public async Task AveragePagedAsync_WithDoubleSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.DoubleValue);

        // Assert
        result.Should().BeApproximately(25.5, 0.001); // (10.5 + 20.5 + 30.5 + 40.5) / 4
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullableDoubleSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.NullableDoubleValue);

        // Assert
        result.Should().BeApproximately(27.167, 0.001); // (10.5 + 30.5 + 40.5) / 3
    }

    [Fact]
    public async Task AveragePagedAsync_WithDecimalSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.DecimalValue);

        // Assert
        result.Should().Be(25.375m); // (10.25 + 20.50 + 30.75 + 40.00) / 4
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullableDecimalSelector_CalculatesCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source.AveragePagedAsync(x => x.NullableDecimalValue);

        // Assert
        result.Should().Be(27m); // (10.25 + 30.75 + 40.00) / 3
    }

    [Fact]
    public async Task AveragePagedAsync_WithEmptySource_ThrowsInvalidOperationException()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.AveragePagedAsync(x => x.IntValue));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.AveragePagedAsync(x => x.LongValue));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.AveragePagedAsync(x => x.DoubleValue));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await source.AveragePagedAsync(x => x.DecimalValue));
    }

    [Fact]
    public async Task AveragePagedAsync_WithEmptySourceNullable_ReturnsNull()
    {
        // Arrange
        var source = CreateEmptyTestData();

        // Act
        var intResult = await source.AveragePagedAsync(x => x.NullableIntValue);
        var longResult = await source.AveragePagedAsync(x => x.NullableLongValue);
        var doubleResult = await source.AveragePagedAsync(x => x.NullableDoubleValue);
        var decimalResult = await source.AveragePagedAsync(x => x.NullableDecimalValue);

        // Assert
        intResult.Should().BeNull();
        longResult.Should().BeNull();
        doubleResult.Should().BeNull();
        decimalResult.Should().BeNull();
    }

    [Fact]
    public async Task AveragePagedAsync_WithOverflow_ThrowsOverflowException()
    {
        // Arrange - Create items that will actually cause long overflow during accumulation
        var items = new[]
        {
            new TestItem(0, null, long.MaxValue, null, 0.0, null, 0m, null),
            new TestItem(0, null, long.MaxValue, null, 0.0, null, 0m, null)
        };
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(2, 1, totalCount: 2)));

        // Act & Assert - Test long values that will overflow
        await Assert.ThrowsAsync<OverflowException>(async () =>
            await source.AveragePagedAsync(x => x.LongValue));
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public async Task SumPagedAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerable<TestItem> source = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.SumPagedAsync(x => x.IntValue));
    }

    [Fact]
    public async Task SumPagedAsync_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.SumPagedAsync((Func<TestItem, int>)null!));
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerable<TestItem> source = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.AveragePagedAsync(x => x.IntValue));
    }

    [Fact]
    public async Task AveragePagedAsync_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateTestData();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await source.AveragePagedAsync((Func<TestItem, int>)null!));
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task SumPagedAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var source = CreateTestData();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.SumPagedAsync(x => x.IntValue, cts.Token));
    }

    [Fact]
    public async Task AveragePagedAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var source = CreateTestData();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.AveragePagedAsync(x => x.IntValue, cts.Token));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task SumPagedAsync_ChainedWithOtherOperations_WorksCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source
            .WherePaged(x => x.IntValue > 15)
            .SumPagedAsync(x => x.IntValue);

        // Assert
        result.Should().Be(90); // 20 + 30 + 40
    }

    [Fact]
    public async Task AveragePagedAsync_ChainedWithOtherOperations_WorksCorrectly()
    {
        // Arrange
        var source = CreateTestData();

        // Act
        var result = await source
            .WherePaged(x => x.IntValue > 15)
            .AveragePagedAsync(x => x.DoubleValue);

        // Assert
        result.Should().BeApproximately(30.5, 0.001); // (20.5 + 30.5 + 40.5) / 3
    }

    [Fact]
    public async Task NumericalOperations_WithLargeDataset_PerformEfficiently()
    {
        // Arrange
        var items = Enumerable.Range(1, 10000)
            .Select(i => new TestItem(i, i % 2 == 0 ? i : null, i * 10L, null, i * 0.1, null, i * 0.01m, null))
            .ToArray();
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10000, 1, totalCount: 10000)));

        // Act
        var sum = await source.SumPagedAsync(x => x.IntValue);
        var average = await source.AveragePagedAsync(x => x.IntValue);

        // Assert
        sum.Should().Be(50005000); // Sum of 1 to 10000
        average.Should().BeApproximately(5000.5, 0.1); // Average of 1 to 10000
    }

    [Fact]
    public async Task NumericalOperations_WithMixedNullValues_CalculatesCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(0, 1, 0L, 10L, 0.0, 1.1, 0m, 1.11m),
            new TestItem(0, null, 0L, null, 0.0, null, 0m, null),
            new TestItem(0, 2, 0L, 20L, 0.0, 2.2, 0m, 2.22m),
            new TestItem(0, null, 0L, null, 0.0, null, 0m, null),
            new TestItem(0, 3, 0L, 30L, 0.0, 3.3, 0m, 3.33m)
        };
        var source = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(5, 1, totalCount: 5)));

        // Act
        var intSum = await source.SumPagedAsync(x => x.NullableIntValue);
        var longSum = await source.SumPagedAsync(x => x.NullableLongValue);
        var doubleSum = await source.SumPagedAsync(x => x.NullableDoubleValue);
        var decimalSum = await source.SumPagedAsync(x => x.NullableDecimalValue);

        var intAvg = await source.AveragePagedAsync(x => x.NullableIntValue);
        var longAvg = await source.AveragePagedAsync(x => x.NullableLongValue);
        var doubleAvg = await source.AveragePagedAsync(x => x.NullableDoubleValue);
        var decimalAvg = await source.AveragePagedAsync(x => x.NullableDecimalValue);

        // Assert
        intSum.Should().Be(6); // 1 + 2 + 3
        longSum.Should().Be(60L); // 10 + 20 + 30
        doubleSum.Should().BeApproximately(6.6, 0.001); // 1.1 + 2.2 + 3.3
        decimalSum.Should().Be(6.66m); // 1.11 + 2.22 + 3.33

        intAvg.Should().BeApproximately(2.0, 0.001); // (1 + 2 + 3) / 3
        longAvg.Should().BeApproximately(20.0, 0.001); // (10 + 20 + 30) / 3
        doubleAvg.Should().BeApproximately(2.2, 0.001); // (1.1 + 2.2 + 3.3) / 3
        decimalAvg.Should().Be(2.22m); // (1.11 + 2.22 + 3.33) / 3
    }

    #endregion
}