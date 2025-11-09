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

public class ElementAccessExtensionsTests
{
    private static IAsyncPagedEnumerable<int> CreatePaged(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task FirstPagedAsync_ReturnsFirst()
    {
        var paged = CreatePaged(3,4,5);
        (await paged.FirstPagedAsync()).Should().Be(3);
    }

    [Fact]
    public async Task FirstPagedAsync_WithPredicate_FindsFirstMatch()
    {
        var paged = CreatePaged(1,2,3,4);
        (await paged.FirstPagedAsync(i => i % 2 == 0)).Should().Be(2);
    }

    [Fact]
    public async Task FirstOrDefaultPagedAsync_ReturnsDefaultOnEmpty()
    {
        var empty = CreatePaged();
        (await empty.FirstOrDefaultPagedAsync()).Should().Be(0);
    }

    [Fact]
    public async Task LastPagedAsync_ReturnsLast()
    {
        var paged = CreatePaged(3,4,5);
        (await paged.LastPagedAsync()).Should().Be(5);
    }

    [Fact]
    public async Task LastOrDefaultPagedAsync_ReturnsDefaultOnEmpty()
    {
        var empty = CreatePaged();
        (await empty.LastOrDefaultPagedAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SinglePagedAsync_ReturnsSoleElement()
    {
        var single = CreatePaged(42);
        (await single.SinglePagedAsync()).Should().Be(42);
    }

    [Fact]
    public async Task SingleOrDefaultPagedAsync_ReturnsDefaultOnEmpty()
    {
        var empty = CreatePaged();
        (await empty.SingleOrDefaultPagedAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ElementAtPagedAsync_ReturnsAtIndex()
    {
        var paged = CreatePaged(10, 20, 30);
        (await paged.ElementAtPagedAsync(1)).Should().Be(20);
    }

    [Fact]
    public async Task ElementAtOrDefaultPagedAsync_ReturnsDefaultWhenOutOfRange()
    {
        var paged = CreatePaged(10,20);
        (await paged.ElementAtOrDefaultPagedAsync(5)).Should().Be(0);
    }
}
