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

public class WindowingExtensionsTests
{
    private static IAsyncPagedEnumerable<int> P(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task WindowPaged_ProducesSlidingWindows()
    {
        var windows = await P(1, 2, 3, 4).WindowPaged(3).ToListAsync();
        // Compare element by element since collection equality of arrays may fail due to reference equality
        windows.Count.Should().Be(2);
        windows[0].Should().Equal(1, 2, 3);
        windows[1].Should().Equal(2, 3, 4);
    }

    [Fact]
    public async Task WindowedSumPaged_Int_ComputesSums()
    {
        var sums = await P(1, 2, 3, 4).WindowedSumPaged(2, x => x).ToListAsync();
        sums.Should().Equal(3, 5, 7);
    }

    [Fact]
    public async Task WindowedAveragePaged_ComputesAverages()
    {
        var avgs = await P(1, 2, 3, 4).WindowedAveragePaged(2, x => (double)x).ToListAsync();
        avgs.Should().Equal(1.5, 2.5, 3.5);
    }

    [Fact]
    public async Task WindowedMinMaxPaged_ComputesMinAndMax()
    {
        var mins = await P(4, 1, 3, 2).WindowedMinPaged(2, x => x).ToListAsync();
        var maxs = await P(4, 1, 3, 2).WindowedMaxPaged(2, x => x).ToListAsync();
        mins.Should().Equal(1, 1, 2);
        maxs.Should().Equal(4, 3, 3);
    }

    [Fact]
    public async Task PairwisePaged_YieldsConsecutivePairs()
    {
        var pairs = await P(1, 2, 3).PairwisePaged().ToListAsync();
        pairs.Should().Equal([(1, 2), (2, 3)]);
    }

    [Fact]
    public async Task ScanPaged_ComputesRunningValues()
    {
        var running = await P(1, 2, 3).ScanPaged(0, (acc, x) => acc + x).ToListAsync();
        running.Should().Equal(0, 1, 3, 6);

        var running2 = await P(1, 2, 3).ScanPaged((acc, x) => acc + x).ToListAsync();
        running2.Should().Equal(1, 3, 6);
    }
}
