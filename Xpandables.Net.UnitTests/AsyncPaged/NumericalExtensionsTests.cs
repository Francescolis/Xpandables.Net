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

public class NumericalExtensionsTests
{
    private sealed record Item(int A, int? B, long L, long? LN, double D, double? DN, decimal M, decimal? MN);

    private static IAsyncPagedEnumerable<Item> Create(params Item[] items)
        => new AsyncPagedEnumerable<Item>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length,1,totalCount:items.Length)));

    [Fact]
    public async Task SumPagedAsync_ComputesSums_ForAllNumericTypes()
    {
        var paged = Create(new(1, 1, 2, 2, 0.5, 0.5, 1m, 1m), new(2, null, 3, null, 1.5, null, 2m, null));

        (await paged.SumPagedAsync(x => x.A)).Should().Be(3);
        (await paged.SumPagedAsync(x => x.B)).Should().Be(1);
        (await paged.SumPagedAsync(x => x.L)).Should().Be(5);
        (await paged.SumPagedAsync(x => x.LN)).Should().Be(2);
        (await paged.SumPagedAsync(x => x.D)).Should().BeApproximately(2.0, 1e-12);
        (await paged.SumPagedAsync(x => x.DN)).Should().BeApproximately(0.5, 1e-12);
        (await paged.SumPagedAsync(x => x.M)).Should().Be(3m);
        (await paged.SumPagedAsync(x => x.MN)).Should().Be(1m);
    }

    [Fact]
    public async Task AveragePagedAsync_ComputesAverages_ForAllNumericTypes()
    {
        var paged = Create(new(1, 1, 2, 2, 1.0, 1.0, 1m, 1m), new(3, null, 6, null, 3.0, null, 3m, null));

        (await paged.AveragePagedAsync(x => x.A)).Should().Be(2.0);
        (await paged.AveragePagedAsync(x => x.B)).Should().Be(1.0);
        (await paged.AveragePagedAsync(x => x.L)).Should().Be(4.0);
        (await paged.AveragePagedAsync(x => x.LN)).Should().Be(2.0);
        (await paged.AveragePagedAsync(x => x.D)).Should().Be(2.0);
        (await paged.AveragePagedAsync(x => x.DN)).Should().Be(1.0);
        (await paged.AveragePagedAsync(x => x.M)).Should().Be(2.0m);
        (await paged.AveragePagedAsync(x => x.MN)).Should().Be(1.0m);
    }
}
