using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using Xpandables.Net.AsyncPaged;

namespace Xpandables.Net.Benchmarking;

[MemoryDiagnoser]
[HideColumns("Job", "Error", "RatioSD")]
public class StrategyOverheadBenchmarks
{
    [Params(20_000)]
    public int Count;

    private int[] _data = [];

    [GlobalSetup]
    public void Setup() => _data = [.. Enumerable.Range(1, Count)];

    private AsyncPagedEnumerable<int> CreatePaged(PaginationStrategy strategy)
    {
        var paged = new AsyncPagedEnumerable<int>(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(pageSize: 128, currentPage: 1, totalCount: Count))
        );
        var e = paged.GetAsyncEnumerator() as IAsyncPagedEnumerator<int>;
        e!.WithStrategy(strategy);
        return paged;
    }

    private IAsyncEnumerable<int> PlainAsync()
    {
        return Core();
        async IAsyncEnumerable<int> Core([EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var v in _data)
            {
                yield return v;
                await Task.CompletedTask;
            }
        }
    }

    [Benchmark(Baseline = true)]
    public async Task Strategy_None()
    {
        var paged = CreatePaged(PaginationStrategy.None);
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Strategy_PerPage()
    {
        var paged = CreatePaged(PaginationStrategy.PerPage);
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Strategy_PerItem()
    {
        var paged = CreatePaged(PaginationStrategy.PerItem);
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }
}
