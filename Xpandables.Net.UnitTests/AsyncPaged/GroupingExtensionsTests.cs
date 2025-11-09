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

public class GroupingExtensionsTests
{
    private sealed record Item(int Id, string Category, int Value);

    private static IAsyncPagedEnumerable<Item> Create(params Item[] items)
        => new AsyncPagedEnumerable<Item>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task GroupByPaged_KeySelector_GroupsCorrectly()
    {
        var paged = Create(new(1, "A", 10), new(2, "B", 20), new(3, "A", 5));
        var groups = await paged.GroupByPaged(x => x.Category).ToListPagedAsync();

        groups.Should().HaveCount(2);
        groups.First(g => g.Key == "A").Should().BeEquivalentTo([new Item(1, "A", 10), new Item(3, "A", 5)]);
        groups.First(g => g.Key == "B").Should().BeEquivalentTo([new Item(2, "B", 20)]);
    }

    [Fact]
    public async Task GroupByPaged_KeyAndElementSelector_ProjectsElements()
    {
        var paged = Create(new(1, "A", 10), new(2, "B", 20), new(3, "A", 5));
        var groups = await paged.GroupByPaged(x => x.Category, x => x.Value).ToListPagedAsync();

        groups.Should().HaveCount(2);
        groups.First(g => g.Key == "A").Should().BeEquivalentTo([10, 5]);
        groups.First(g => g.Key == "B").Should().BeEquivalentTo([20]);
    }

    [Fact]
    public async Task GroupByPaged_ResultSelector_ProducesResults()
    {
        var paged = Create(new(1, "A", 10), new(2, "B", 20), new(3, "A", 5));
        var results = await paged.GroupByPaged(x => x.Category, (key, values) => new { Key = key, Sum = values.Sum(v => v.Value) }).ToListPagedAsync();

        results.Should().BeEquivalentTo(
        [
            new { Key = "A", Sum = 15 },
            new { Key = "B", Sum = 20 }
        ]);
    }

    [Fact]
    public async Task ToLookupPagedAsync_CreatesLookup()
    {
        var paged = Create(new(1, "A", 10), new(2, "B", 20), new(3, "A", 5));
        var lookup = await paged.ToLookupPagedAsync(x => x.Category);

        lookup.Contains("A").Should().BeTrue();
        lookup["A"].Should().BeEquivalentTo([new Item(1, "A", 10), new Item(3, "A", 5)]);
    }
}
