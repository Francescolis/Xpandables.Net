using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using BenchmarkDotNet.Attributes;

using Xpandables.Net.Async;

namespace Xpandables.Net.Benchmarking;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[HideColumns("Job", "Error", "RatioSD")]
public partial class JsonStreamingBenchmarks
{
    [Params(1_000, 100_000)]
    public int Count;

    private HttpContent _arrayRootContent = default!;
    private JsonSerializerOptions _options = default!;

    [JsonSerializable(typeof(PayloadItem[]))]
    [JsonSerializable(typeof(PayloadItem))]
    public partial class PayloadItemContext : JsonSerializerContext { }

    public record PayloadItem(int Id, string Name, bool Flag);

    [GlobalSetup]
    public void Setup()
    {
        var items = Enumerable.Range(1, Count)
            .Select(i => new PayloadItem(i, "Name" + i, (i & 1) == 0))
            .ToArray();

        // array-root json to avoid extra parsing cost differences
        var json = JsonSerializer.Serialize(items);
        _arrayRootContent = new StringContent(json, Encoding.UTF8, "application/json");

        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(PayloadItemContext.Default)
        };
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        // reset content stream for next iteration
        if (_arrayRootContent is StringContent sc)
        {
            // StringContent can't be reset; recreate each iteration to ensure fresh stream
            sc.Dispose();
            var items = Enumerable.Range(1, Count)
                .Select(i => new PayloadItem(i, "Name" + i, (i & 1) == 0))
                .ToArray();
            var json = JsonSerializer.Serialize(items);
            _arrayRootContent = new StringContent(json, Encoding.UTF8, "application/json");
        }
    }

    private static async Task<long> ConsumeAsync<T>(IAsyncEnumerable<T> source)
    {
        long n = 0;
        await foreach (var _ in source)
            n++;
        return n;
    }

    [Benchmark(Baseline = true)]
    public async Task DeserializeAsyncEnumerable_ArrayRoot()
    {
        var stream = _arrayRootContent.ReadFromJsonAsAsyncEnumerable<PayloadItem>(_options);
        long n = 0;
        await foreach (var _ in stream)
            n++;
        GC.KeepAlive(n);
    }

    [Benchmark]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_ArrayRoot()
    {
        var paged = _arrayRootContent.ReadFromJsonAsAsyncPagedEnumerable<PayloadItem>(
            _options);
        //var ctx = await paged.GetPaginationAsync();
        var ctx = paged.Pagination;
        long n = 0;
        await foreach (var _ in paged)
            n++;
        GC.KeepAlive(ctx);
        GC.KeepAlive(n);
    }
}
