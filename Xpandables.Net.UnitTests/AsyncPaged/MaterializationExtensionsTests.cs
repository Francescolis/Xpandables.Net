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

public class MaterializationExtensionsTests
{
    private static IAsyncPagedEnumerable<int> CreatePaged(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length,1,totalCount:items.Length)));

    [Fact]
    public async Task PrecomputePaginationAsync_ComputesPagination()
    {
        var paged = CreatePaged(1,2,3);
        paged.Pagination.TotalCount.Should().BeNull(); // lazy before compute
        await paged.PrecomputePaginationAsync();
        (await paged.GetPaginationAsync()).TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task MaterializeAsync_FullyMaterializes()
    {
        var paged = CreatePaged(5,6,7);
        var materialized = await paged.MaterializeAsync();
        var list = await materialized.ToListAsync();
        list.Should().Equal(5,6,7);
        (await materialized.GetPaginationAsync()).TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task ToMaterializedAsyncPagedEnumerable_FromAsyncEnumerable()
    {
        var source = new[] {10,20}.ToAsyncEnumerable();
        var paged = await source.ToMaterializedAsyncPagedEnumerable();
        (await paged.GetPaginationAsync()).TotalCount.Should().Be(2);
        var arr = await paged.ToListAsync();
        arr.Should().Equal(10,20);
    }
}
