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

public class TransformationExtensionsTests
{
    private sealed record Wrapper(int Id, int[] Values);

    private static IAsyncPagedEnumerable<Wrapper> Create(params Wrapper[] items)
        => new AsyncPagedEnumerable<Wrapper>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task SelectManyPaged_FlattensInnerSyncCollections()
    {
        var paged = Create(new Wrapper(1, [1, 2]), new Wrapper(2, [3]));
        var flattened = await paged.SelectManyPaged(w => w.Values).ToListAsync();
        flattened.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task SelectManyPaged_WithResultSelector_ProjectsCorrectly()
    {
        var paged = Create(new Wrapper(1, [1, 2]));
        var projected = await paged.SelectManyPaged(w => w.Values, (w, v) => (w.Id, v)).ToListAsync();
        projected.Should().Equal([(1, 1), (1, 2)]);
    }

    [Fact]
    public async Task SelectManyPagedAsync_FlattensAsyncInnerCollections()
    {
        var paged = Create(new Wrapper(1, [1, 2]));
        IAsyncPagedEnumerable<int> result = paged.SelectManyPagedAsync(w => ToAsync(w.Values));
        var list = await result.ToListAsync();
        list.Should().Equal(1, 2);
    }

    private static async IAsyncEnumerable<int> ToAsync(IEnumerable<int> values)
    {
        foreach (var v in values)
        {
            yield return v;
            await Task.Yield();
        }
    }
}
