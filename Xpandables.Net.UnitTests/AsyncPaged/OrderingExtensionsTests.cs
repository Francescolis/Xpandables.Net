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

public class OrderingExtensionsTests
{
    private static IAsyncPagedEnumerable<int> CreatePaged(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task OrderByPaged_SortsAscending()
    {
        var paged = CreatePaged(5,1,3);
        var sorted = await paged.OrderByPaged(x => x).ToListAsync();
        sorted.Should().Equal(1,3,5);
    }

    [Fact]
    public async Task OrderByDescendingPaged_SortsDescending()
    {
        var paged = CreatePaged(5,1,3);
        var sorted = await paged.OrderByDescendingPaged(x => x).ToListAsync();
        sorted.Should().Equal(5,3,1);
    }

    [Fact]
    public async Task ReversePaged_ReversesOrder()
    {
        var paged = CreatePaged(1,2,3);
        var reversed = await paged.ReversePaged().ToListAsync();
        reversed.Should().Equal(3,2,1);
    }
}
