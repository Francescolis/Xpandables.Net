using FluentAssertions;

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

public class AsyncPagedEnumerableTests
{
    [Fact]
    public async Task PageContext_Before_Computation_Throws()
    {
        var enumerable = new AsyncPagedEnumerable<int>(
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
        var enumerable = new AsyncPagedEnumerable<int>(
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

        var enumerable = new AsyncPagedEnumerable<int>(query);

        var ctx = await enumerable.GetPaginationAsync();
        ctx.PageSize.Should().Be(5);
        ctx.CurrentPage.Should().Be((10 / 5) + 1); // 3
        ctx.TotalCount.Should().Be(30);
    }

    [Fact]
    public async Task AsyncMapper_Projects_Items()
    {
        // Use local async enumerable method for async mapping
        static async IAsyncEnumerable<int> MapAsync(IAsyncEnumerable<int> source, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var x in source.WithCancellation(ct))
            {
                await Task.Delay(1, ct);
                yield return x * 10;
            }
        }

        var mapped = MapAsync(Enumerable.Range(1, 4).ToAsyncEnumerable());

        var enumerable = new AsyncPagedEnumerable<int>(
            mapped,
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(2, 1, totalCount: 4)));

        _ = await enumerable.GetPaginationAsync();

        var result = new List<int>();
        await foreach (var item in enumerable)
            result.Add(item);

        result.Should().Equal(10, 20, 30, 40);
    }

    [Fact]
    public async Task SyncMapper_Projects_Items()
    {
        // Use Select for sync mapping
        var source = Enumerable.Range(1, 3).Select(x => x + 100).ToAsync();

        var enumerable = source.ToAsyncPagedEnumerable(
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(3, 1, totalCount: 3)));

        _ = await enumerable.GetPaginationAsync();

        var collected = await enumerable.ToListAsync();
        collected.Should().Equal(101, 102, 103);
    }

    [Fact]
    public async Task Enumerator_Cast_Allows_Strategy_Change()
    {
        var enumerable = new AsyncPagedEnumerable<int>(
            Enumerable.Range(1, 3).ToAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(3, 1, totalCount: 3)));

        _ = await enumerable.GetPaginationAsync();

        var e = enumerable.GetAsyncEnumerator() as IAsyncPagedEnumerator<int>;
        e.Should().NotBeNull();

        e!.WithStrategy(PaginationStrategy.None);
        while (await e.MoveNextAsync()) { /* iterate */ }

        e.Pagination.TotalCount.Should().Be(3);
        await e.DisposeAsync();
    }
}