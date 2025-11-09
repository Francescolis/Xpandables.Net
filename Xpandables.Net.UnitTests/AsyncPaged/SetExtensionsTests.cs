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

public class SetExtensionsTests
{
    private static IAsyncPagedEnumerable<int> P(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task UnionPaged_UnifiesWithoutDuplicates()
    {
        var a = P(1, 2, 3);
        var b = new[] { 3, 4, 5 }.ToAsyncEnumerable();
        var result = await a.UnionPaged(b).ToListAsync();
        result.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task IntersectPaged_ReturnsCommonElements()
    {
        var a = P(1, 2, 3, 4);
        var b = new[] { 3, 4, 5 }.ToAsyncEnumerable();
        var result = await a.IntersectPaged(b).ToListAsync();
        result.Should().Equal(3, 4);
    }

    [Fact]
    public async Task ExceptPaged_RemovesElementsPresentInOther()
    {
        var a = P(1, 2, 3, 4);
        var b = new[] { 2, 3 }.ToAsyncEnumerable();
        var result = await a.ExceptPaged(b).ToListAsync();
        result.Should().Equal(1, 4);
    }

    [Fact]
    public async Task Concat_Prepend_Append_Work()
    {
        var a = P(2, 3);
        var b = new[] { 4 }.ToAsyncEnumerable();
        var concat = await a.ConcatPaged(b).ToListAsync();
        concat.Should().Equal(2, 3, 4);

        var prepend = await a.PrependPaged(1).ToListAsync();
        prepend.Should().Equal(1, 2, 3);

        var append = await a.AppendPaged(9).ToListAsync();
        append.Should().Equal(2, 3, 9);
    }

    [Fact]
    public async Task ZipPaged_ZipsSequences()
    {
        var a = P(1, 2, 3);
        var b = new[] { 10, 20 }.ToAsyncEnumerable();
        var zipped = await a.ZipPaged(b).ToListAsync();
        zipped.Should().Equal([(1, 10), (2, 20)]);
    }

    [Fact]
    public async Task DefaultIfEmptyPaged_ReturnsDefaultWhenEmpty()
    {
        var empty = P();
        var result = await empty.DefaultIfEmptyPaged(42).ToListAsync();
        result.Should().Equal(42);
    }
}
