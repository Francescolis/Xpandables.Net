using FluentAssertions;

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

public class MappingEnumeratorTests
{
    [Fact]
    public async Task Sync_Mapping_Works_With_Strategy_PerPage()
    {
        var initial = Pagination.Create(pageSize: 2, currentPage: 1, totalCount: 4);
        var src = Enumerable.Range(1, 4).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            src.GetAsyncEnumerator(),
            syncMapper: x => x * 2,
            cancellationToken: default,
            initialContext: initial);

        enumerator.WithPageContextStrategy(PaginationStrategy.PerPage);

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
        var src = Enumerable.Range(1, 3).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            src.GetAsyncEnumerator(),
            async (x, ct) =>
            {
                await Task.Delay(1, ct);
                return x + 5;
            },
            cancellationToken: default,
            initialContext: initial);

        enumerator.WithPageContextStrategy(PaginationStrategy.PerItem);

        var values = new List<int>();
        while (await enumerator.MoveNextAsync())
            values.Add(enumerator.Current);

        values.Should().Equal(6, 7, 8);
        enumerator.Pagination.TotalCount.Should().Be(3);
    }
}