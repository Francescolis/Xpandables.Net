using FluentAssertions;

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

public class AsyncPagedEnumerableTests
{
    [Fact]
    public async Task PageContext_Before_Computation_Throws()
    {
        var enumerable = new AsyncPagedEnumerable<int, int>(
            Enumerable.Range(1, 5).ToAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(5, 1, totalCount: 5))
        );

        // Accessing context before computation used to throw via property; now call method and assert state is computed.
        var ctx = await enumerable.GetPaginationAsync();
        ctx.PageSize.Should().Be(5);
        ctx.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetPageContextAsync_Computes_Once_Concurrent()
    {
        int calls = 0;
        var enumerable = new AsyncPagedEnumerable<int, int>(
            Enumerable.Range(1, 10).ToAsync(),
            async ct =>
            {
                Interlocked.Increment(ref calls);
                await Task.Delay(10, ct);
                return Pagination.Create(pageSize: 5, currentPage: 1, totalCount: 10);
            });

        await Parallel.ForEachAsync(
            Enumerable.Range(0, 25),
            async (_, ct) => await enumerable.GetPaginationAsync(ct));

        calls.Should().Be(1);
        var computed = await enumerable.GetPaginationAsync();
        computed.TotalCount.Should().Be(10);
        computed.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Queryable_Computation_Extracts_Skip_Take()
    {
        var items = Enumerable.Range(1, 30).ToList();
        var query = items.AsQueryable().Skip(10).Take(5);

        var enumerable = new AsyncPagedEnumerable<int, int>(query);

        var ctx = await enumerable.GetPaginationAsync();
        ctx.PageSize.Should().Be(5);
        ctx.CurrentPage.Should().Be((10 / 5) + 1); // 3
        ctx.TotalCount.Should().Be(30);
    }

    [Fact]
    public async Task AsyncMapper_Projects_Items()
    {
        var enumerable = new AsyncPagedEnumerable<int, int>(
            Enumerable.Range(1, 4).ToAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(2, 1, totalCount: 4)),
            mapper: async (x, ct) =>
            {
                await Task.Delay(1, ct);
                return x * 10;
            });

        _ = await enumerable.GetPaginationAsync();

        var result = new List<int>();
        await foreach (var item in enumerable)
            result.Add(item);

        result.Should().Equal(10, 20, 30, 40);
    }

    [Fact]
    public async Task SyncMapper_Projects_Items()
    {
        var enumerable = AsyncPagedEnumerable.ToAsyncPagedEnumerable(
            Enumerable.Range(1, 3).ToAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(3, 1, totalCount: 3)),
            mapper: x => x + 100);

        _ = await enumerable.GetPaginationAsync();

        var collected = await enumerable.ToListAsync();
        collected.Should().Equal(101, 102, 103);
    }

    [Fact]
    public async Task Enumerator_Cast_Allows_Strategy_Change()
    {
        var enumerable = new AsyncPagedEnumerable<int, int>(
            Enumerable.Range(1, 3).ToAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(3, 1, totalCount: 3)));

        _ = await enumerable.GetPaginationAsync();

        var e = enumerable.GetAsyncEnumerator() as IAsyncPagedEnumerator<int>;
        e.Should().NotBeNull();

        e!.WithPageContextStrategy(PaginationStrategy.None);
        while (await e.MoveNextAsync()) { /* iterate */ }

        e.Pagination.TotalCount.Should().Be(3);
        await e.DisposeAsync();
    }
}