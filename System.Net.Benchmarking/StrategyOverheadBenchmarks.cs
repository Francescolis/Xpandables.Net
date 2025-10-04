using System.Net.Async;

using BenchmarkDotNet.Attributes;

using Xpandables.Net.Async;

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

    private AsyncPagedEnumerable<int, int> CreatePaged(PageContextStrategy strategy)
    {
        var paged = new AsyncPagedEnumerable<int, int>(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(PageContext.Create(pageSize: 128, currentPage: 1, totalCount: Count))
        );
        var e = paged.GetAsyncEnumerator() as IAsyncPagedEnumerator<int>;
        e!.WithPageContextStrategy(strategy);
        return paged;
    }

    private IAsyncEnumerable<int> PlainAsync()
    {
        return Core();
        async IAsyncEnumerable<int> Core([Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
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
        var paged = CreatePaged(PageContextStrategy.None);
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Strategy_PerPage()
    {
        var paged = CreatePaged(PageContextStrategy.PerPage);
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Strategy_PerItem()
    {
        var paged = CreatePaged(PageContextStrategy.PerItem);
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }
}
