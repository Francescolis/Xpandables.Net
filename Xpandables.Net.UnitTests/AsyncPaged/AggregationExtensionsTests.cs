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

namespace Xpandables.Net.UnitTests.AsyncPaged;

public class AggregationExtensionsTests
{
    private static IAsyncPagedEnumerable<int> CreatePaged(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task CountPagedAsync_ReturnsItemCount()
    {
        var paged = CreatePaged(1, 2, 3, 4);
        var count = await paged.CountPagedAsync();
        count.Should().Be(4);
    }

    [Fact]
    public async Task CountPagedAsync_WithPredicate_ReturnsFilteredCount()
    {
        var paged = CreatePaged(1, 2, 3, 4, 5);
        var count = await paged.CountPagedAsync(i => i % 2 == 0);
        count.Should().Be(2);
    }

    [Fact]
    public async Task AnyPagedAsync_ReturnsFalseOnEmpty()
    {
        var empty = CreatePaged();
        (await empty.AnyPagedAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task AnyPagedAsync_WithPredicate_ReturnsTrueIfMatch()
    {
        var paged = CreatePaged(1, 3, 5, 8);
        (await paged.AnyPagedAsync(i => i == 8)).Should().BeTrue();
    }

    [Fact]
    public async Task AllPagedAsync_ReturnsTrueWhenAllMatch()
    {
        var paged = CreatePaged(2, 4, 6);
        (await paged.AllPagedAsync(i => i % 2 == 0)).Should().BeTrue();
    }

    [Fact]
    public async Task ContainsPagedAsync_FindsValue()
    {
        var paged = CreatePaged(10, 20, 30);
        (await paged.ContainsPagedAsync(20)).Should().BeTrue();
        (await paged.ContainsPagedAsync(25)).Should().BeFalse();
    }

    [Fact]
    public async Task AggregatePagedAsync_WithoutSeed_SumsSequence()
    {
        var paged = CreatePaged(1, 2, 3);
        var sum = await paged.AggregatePagedAsync((a, b) => a + b);
        sum.Should().Be(6);
    }

    [Fact]
    public async Task AggregatePagedAsync_WithSeed_AppliesAccumulator()
    {
        var paged = CreatePaged(1, 2, 3);
        var product = await paged.AggregatePagedAsync(1, (acc, x) => acc * x);
        product.Should().Be(6);
    }

    [Fact]
    public async Task AggregatePagedAsync_WithResultSelector_ReturnsTransformed()
    {
        var paged = CreatePaged(1, 2, 3);
        var result = await paged.AggregatePagedAsync(0, (acc, x) => acc + x, acc => acc * 2);
        result.Should().Be(12);
    }

    [Fact]
    public async Task MinPagedAsync_ReturnsMinimum()
    {
        var paged = CreatePaged(9, 3, 7);
        (await paged.MinPagedAsync()).Should().Be(3);
    }

    [Fact]
    public async Task MaxPagedAsync_ReturnsMaximum()
    {
        var paged = CreatePaged(9, 3, 7);
        (await paged.MaxPagedAsync()).Should().Be(9);
    }

    private sealed record Sample(int Id, int Value);
    private static IAsyncPagedEnumerable<Sample> CreateSamples(params Sample[] s)
        => new AsyncPagedEnumerable<Sample>(s.ToAsyncEnumerable(), _ => new(Pagination.Create(s.Length, 1, totalCount: s.Length)));

    [Fact]
    public async Task MinByPagedAsync_ReturnsElementWithMinKey()
    {
        var paged = CreateSamples(new(1, 50), new(2, 10), new(3, 30));
        var min = await paged.MinByPagedAsync(x => x.Value);
        min.Id.Should().Be(2);
    }

    [Fact]
    public async Task MaxByPagedAsync_ReturnsElementWithMaxKey()
    {
        var paged = CreateSamples(new(1, 50), new(2, 10), new(3, 70));
        var max = await paged.MaxByPagedAsync(x => x.Value);
        max.Id.Should().Be(3);
    }

    [Fact]
    public async Task ToListPagedAsync_MaterializesAll()
    {
        var paged = CreatePaged(1, 2, 3);
        var list = await paged.ToListPagedAsync();
        list.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task ToArrayPagedAsync_MaterializesAll()
    {
        var paged = CreatePaged(4, 5, 6);
        var array = await paged.ToArrayPagedAsync();
        array.Should().Equal([4, 5, 6]);
    }
}
