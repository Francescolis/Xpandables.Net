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
        // For async mapping, we can use SelectAwait from System.Linq.Async
        var src = Enumerable.Range(1, 3).ToAsyncEnumerable().SelectAwait(async x =>
        {
            await Task.Delay(1);
            return x + 5;
        });
        var enumerator = AsyncPagedEnumerator.Create(
            src.GetAsyncEnumerator(),
            initialContext: initial,
            cancellationToken: default);

        enumerator.WithStrategy(PaginationStrategy.PerItem);

        var values = new List<int>();
        while (await enumerator.MoveNextAsync())
            values.Add(enumerator.Current);

        values.Should().Equal(6, 7, 8);
        enumerator.Pagination.TotalCount.Should().Be(3);
    }
}}