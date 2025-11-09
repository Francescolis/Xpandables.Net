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

public class JoinExtensionsTests
{
    private sealed record Left(int Id, string Name);
    private sealed record Right(int LId, string Extra);

    private static IAsyncPagedEnumerable<Left> Lefts(params Left[] items)
        => new AsyncPagedEnumerable<Left>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task JoinPaged_InnerJoinMatchesOnKey()
    {
        var outer = Lefts(new(1, "A"), new(2, "B"), new(3, "C"));
        var inner = new[] { new Right(1, "a1"), new Right(3, "c1"), new Right(3, "c2") }.ToAsyncEnumerable();

        var joined = outer.JoinPaged(inner, o => o.Id, i => i.LId, (o, i) => new { o.Id, o.Name, i.Extra });
        var list = await joined.ToListAsync();

        list.Should().BeEquivalentTo(
        [
            new { Id = 1, Name = "A", Extra = "a1" },
            new { Id = 3, Name = "C", Extra = "c1" },
            new { Id = 3, Name = "C", Extra = "c2" }
        ]);
    }

    [Fact]
    public async Task GroupJoinPaged_GroupsInnerMatches()
    {
        var outer = Lefts(new(1, "A"), new(2, "B"));
        var inner = new[] { new Right(1, "a1"), new Right(1, "a2"), new Right(2, "b1") }.ToAsyncEnumerable();

        var grouped = outer.GroupJoinPaged(inner, o => o.Id, i => i.LId, (o, inners) => new { o.Id, Count = inners.Count() });
        var list = await grouped.ToListAsync();

        list.Should().BeEquivalentTo(
        [
            new { Id = 1, Count = 2 },
            new { Id = 2, Count = 1 }
        ]);
    }
}
