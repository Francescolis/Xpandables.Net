using BenchmarkDotNet.Attributes;

using Xpandables.Net.Async;

namespace Xpandables.Net.Benchmarking;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[HideColumns("Job", "Error", "RatioSD")]
public class PaginationBenchmarks
{
    // Small / Medium / Large
    [Params(5_000, 100_000, 2_000_000)]
    public int Count;

    private int[] _data = [];

    [GlobalSetup]
    public void Setup()
    {
        _data = [.. Enumerable.Range(1, Count)];
    }

    private IAsyncEnumerable<int> PlainAsync()
    {
        return Core();
        async IAsyncEnumerable<int> Core([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var v in _data)
            {
                if ((v & 1023) == 0) // occasional cancellation check
                    ct.ThrowIfCancellationRequested();
                yield return v; // no artificial delay to measure pure overhead
                await Task.CompletedTask;
            }
        }
    }

    [Benchmark(Baseline = true)]
    public async Task Plain_IAsyncEnumerable()
    {
        long sum = 0;
        await foreach (var i in PlainAsync())
            sum += i;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Paged_PreComputed_PerPage()
    {
        var paged = new AsyncPagedEnumerable<int, int>(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(pageSize: 256, currentPage: 1, totalCount: Count))
        );
        await paged.GetPageContextAsync();
        long sum = 0;
        await foreach (var i in paged)
            sum += i;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Paged_PerItem_Strategy()
    {
        var paged = new AsyncPagedEnumerable<int, int>(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(pageSize: 0, currentPage: 0, totalCount: null))
        );
        var enumerator = paged.GetAsyncEnumerator() as IAsyncPagedEnumerator<int>;
        enumerator!.WithPageContextStrategy(PaginationStrategy.PerItem);
        long sum = 0;
        while (await enumerator.MoveNextAsync())
            sum += enumerator.Current;
        await enumerator.DisposeAsync();
        GC.KeepAlive(sum);
    }
}
