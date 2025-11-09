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

public class PaginationExtensionsTests
{
    private static IAsyncPagedEnumerable<int> CreatePaged(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length,1,totalCount:items.Length)));

    [Fact]
    public async Task TakePaged_ReturnsFirstN()
    {
        var paged = CreatePaged(1,2,3,4);
        var taken = await paged.TakePaged(2).ToListAsync();
        taken.Should().Equal(1,2);
    }

    [Fact]
    public async Task SkipPaged_SkipsFirstN()
    {
        var paged = CreatePaged(1,2,3,4);
        var skipped = await paged.SkipPaged(2).ToListAsync();
        skipped.Should().Equal(3,4);
    }

    [Fact]
    public async Task TakeWhilePaged_TakesWhileConditionTrue()
    {
        var paged = CreatePaged(1,2,3,0,4);
        var taken = await paged.TakeWhilePaged(x => x > 0).ToListAsync();
        taken.Should().Equal(1,2,3);
    }

    [Fact]
    public async Task SkipWhilePaged_SkipsWhileConditionTrue()
    {
        var paged = CreatePaged(0,0,1,2);
        var result = await paged.SkipWhilePaged(x => x == 0).ToListAsync();
        result.Should().Equal(1,2);
    }

    [Fact]
    public async Task TakeLastPaged_ReturnsLastN()
    {
        var paged = CreatePaged(1,2,3,4,5);
        var last = await paged.TakeLastPaged(2).ToListAsync();
        last.Should().Equal(4,5);
    }

    [Fact]
    public async Task SkipLastPaged_SkipsLastN()
    {
        var paged = CreatePaged(1,2,3,4,5);
        var result = await paged.SkipLastPaged(2).ToListAsync();
        result.Should().Equal(1,2,3);
    }

    [Fact]
    public async Task ChunkPaged_SplitsIntoChunks()
    {
        var paged = CreatePaged(1,2,3,4,5);
        var chunks = await paged.ChunkPaged(2).ToListAsync();
        chunks.Should().HaveCount(3);
        chunks[0].Should().Equal(1,2);
        chunks[1].Should().Equal(3,4);
        chunks[2].Should().Equal(5);
    }

    [Fact]
    public async Task DistinctPaged_RemovesDuplicates()
    {
        var paged = CreatePaged(1,1,2,2,3);
        var distinct = await paged.DistinctPaged().ToListAsync();
        distinct.Should().Equal(1,2,3);
    }

    [Fact]
    public async Task DistinctByPaged_RemovesDuplicatesByKey()
    {
        var items = new[]{ (Id:1, K:10), (Id:2, K:10), (Id:3, K:20) };
        var paged = new AsyncPagedEnumerable<(int Id,int K)>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length,1,totalCount:items.Length)));
        var distinct = await paged.DistinctByPaged(x => x.K).ToListAsync();
        distinct.Should().HaveCount(2);
    }

    [Fact]
    public async Task WherePaged_FiltersByPredicate()
    {
        var paged = CreatePaged(1,2,3,4);
        var filtered = await paged.WherePaged(x => x % 2 == 0).ToListAsync();
        filtered.Should().Equal(2,4);
    }

    [Fact]
    public async Task WherePaged_WithIndex_Filters()
    {
        var paged = CreatePaged(10,20,30);
        var filtered = await paged.WherePaged((x,i) => i == 1).ToListAsync();
        filtered.Should().Equal(20);
    }
}
