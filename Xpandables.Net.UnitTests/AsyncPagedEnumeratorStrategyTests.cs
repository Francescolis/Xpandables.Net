using FluentAssertions;

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

public class AsyncPagedEnumeratorStrategyTests
{
    [Fact]
    public async Task Strategy_None_Does_Not_Modify_Context()
    {
        var initial = Pagination.Create(pageSize: 10, currentPage: 7, totalCount: 42);
        var source = Enumerable.Range(1, 5).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            source.GetAsyncEnumerator(),
            cancellationToken: default,
            initialContext: initial);

        enumerator.WithPageContextStrategy(PaginationStrategy.None);

        while (await enumerator.MoveNextAsync()) { /* ignore */ }

        enumerator.Pagination.Should().Be(initial);
    }

    [Fact]
    public async Task Strategy_PerItem_Increments_CurrentPage_And_Finalizes_Total()
    {
        var initial = Pagination.Create(pageSize: 0, currentPage: 0, totalCount: null);
        var source = Enumerable.Range(1, 5).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            source.GetAsyncEnumerator(),
            cancellationToken: default,
            initialContext: initial);

        enumerator.WithPageContextStrategy(PaginationStrategy.PerItem);

        int observedMax = 0;
        while (await enumerator.MoveNextAsync())
        {
            var cp = enumerator.Pagination.CurrentPage;
            cp.Should().BeGreaterThan(0);
            observedMax = cp;
        }

        enumerator.Pagination.CurrentPage.Should().Be(observedMax);
        enumerator.Pagination.TotalCount.Should().Be(5);
        enumerator.Pagination.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task Strategy_PerPage_Computes_CurrentPage_From_PageSize()
    {
        var initial = Pagination.Create(pageSize: 3, currentPage: 1, totalCount: 7);
        var source = Enumerable.Range(1, 7).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            source.GetAsyncEnumerator(),
            cancellationToken: default,
            initialContext: initial);

        enumerator.WithPageContextStrategy(PaginationStrategy.PerPage);

        var pages = new List<int>();
        while (await enumerator.MoveNextAsync())
        {
            pages.Add(enumerator.Pagination.CurrentPage);
        }

        pages.Should().Equal(1, 1, 1, 2, 2, 2, 3);
        enumerator.Pagination.TotalCount.Should().Be(7);
    }

    [Fact]
    public async Task PerItem_Does_Not_Overwrite_PreExisting_Total()
    {
        var initial = Pagination.Create(pageSize: 0, currentPage: 0, totalCount: 999);
        var source = Enumerable.Range(1, 3).ToAsync();
        var enumerator = AsyncPagedEnumerator.Create(
            source.GetAsyncEnumerator(),
            cancellationToken: default,
            initialContext: initial);

        enumerator.WithPageContextStrategy(PaginationStrategy.PerItem);

        while (await enumerator.MoveNextAsync()) { }

        enumerator.Pagination.TotalCount.Should().Be(999);
    }

    [Fact]
    public async Task Empty_Source_Returns_False_Immediately()
    {
        var empty = AsyncPagedEnumerator.Empty<int, int>(Pagination.Empty);
        empty.WithPageContextStrategy(PaginationStrategy.PerItem);

        var moved = await empty.MoveNextAsync();
        moved.Should().BeFalse();
    }
}