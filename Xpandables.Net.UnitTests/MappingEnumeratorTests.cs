using FluentAssertions;

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;
using System.Linq;

namespace Xpandables.Net.UnitTests;

public class MappingEnumeratorTests
{
    [Fact]
    public async Task Sync_Mapping_Works_With_Strategy_PerPage()
    {
        var initial = Pagination.Create(pageSize: 2, currentPage: 1, totalCount: 4);
        var src = Enumerable.Range(1, 4).Select(x => x * 2).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            src.GetAsyncEnumerator(),
            initialContext: initial,
            cancellationToken: default);

        enumerator.WithStrategy(PaginationStrategy.PerPage);

        var list = new List<int>();
        while (await enumerator.MoveNextAsync())
            list.Add(enumerator.Current);

        list.Should().Equal(2, 4, 6, 8);
        enumerator.Pagination.CurrentPage.Should().Be(2);
    }

    [Fact]
    public async Task Async_Mapping_Works_With_Strategy_PerItem()
    {
        var initial = Pagination.Create(pageSize: 0, currentPage: 0, totalCount: null);

        // For async mapping, create a local async enumerable method
        async IAsyncEnumerable<int> MapAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var x in Enumerable.Range(1, 3))
            {
                await Task.Delay(1, ct);
                yield return x + 5;
            }
        }

        var enumerator = AsyncPagedEnumerator.Create(
            MapAsync().GetAsyncEnumerator(),
            initialContext: initial,
            cancellationToken: default);

        enumerator.WithStrategy(PaginationStrategy.PerItem);

        var values = new List<int>();
        while (await enumerator.MoveNextAsync())
            values.Add(enumerator.Current);

        values.Should().Equal(6, 7, 8);
        enumerator.Pagination.TotalCount.Should().Be(3);
    }
}