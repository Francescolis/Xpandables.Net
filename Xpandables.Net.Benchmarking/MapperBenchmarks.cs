using BenchmarkDotNet.Attributes;

using Xpandables.Net.Async;

namespace Xpandables.Net.Benchmarking;

[MemoryDiagnoser]
[HideColumns("Job", "Error", "RatioSD")]
public class MapperBenchmarks
{
    [Params(10_000)]
    public int Count;

    private int[] _data = [];

    [GlobalSetup]
    public void Setup() => _data = [.. Enumerable.Range(1, Count)];

    private IAsyncEnumerable<int> PlainAsync()
    {
        return Core();
        async IAsyncEnumerable<int> Core([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var v in _data)
            {
                yield return v;
                await Task.CompletedTask;
            }
        }
    }

    [Benchmark(Baseline = true)]
    public async Task Sync_Map()
    {
        var paged = AsyncPagedEnumerable.ToAsyncPagedEnumerable(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(pageSize: 512, currentPage: 1, totalCount: Count)),
            mapper: x => x + 1);
        await paged.GetPaginationAsync();
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Async_Map_MinimalAwait()
    {
        var paged = new AsyncPagedEnumerable<int, int>(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(pageSize: 512, currentPage: 1, totalCount: Count)),
            mapper: async (x, ct) => { await Task.CompletedTask; return x + 1; });
        await paged.GetPaginationAsync();
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }

    [Benchmark]
    public async Task Async_Map_WithWork()
    {
        var paged = new AsyncPagedEnumerable<int, int>(
            PlainAsync(),
            paginationFactory: ct => ValueTask.FromResult(Pagination.Create(pageSize: 512, currentPage: 1, totalCount: Count)),
            mapper: async (x, ct) =>
            {
                // Simulate small async CPU-bound then I/O-like yield
                int r = x * 2 + 3;
                await Task.Yield();
                return r;
            });
        await paged.GetPaginationAsync();
        int sum = 0;
        await foreach (var v in paged)
            sum += v;
        GC.KeepAlive(sum);
    }
}
