using FluentAssertions;
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.UnitTests.AsyncPaged;

public class ProjectionExtensionsTests
{
    private static IAsyncPagedEnumerable<int> Create(params int[] items)
        => new AsyncPagedEnumerable<int>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length,1,totalCount:items.Length)));

    [Fact]
    public async Task SelectPaged_ProjectsElements()
    {
        var paged = Create(1,2,3);
        var projected = await paged.SelectPaged(x => x * 10).ToListAsync();
        projected.Should().Equal(10,20,30);
    }

    [Fact]
    public async Task SelectPagedAsync_ValueTaskSelector()
    {
        var paged = Create(1,2);
        var projected = await paged.SelectPagedAsync(x => new ValueTask<int>(x + 1)).ToListAsync();
        projected.Should().Equal(2,3);
    }

    [Fact]
    public async Task SelectPagedAsync_CancellationAwareSelector()
    {
        var paged = Create(1,2,3);
        var projected = await paged.SelectPagedAsync((x, ct) => new ValueTask<int>(x * 2)).ToListAsync();
        projected.Should().Equal(2,4,6);
    }
}
