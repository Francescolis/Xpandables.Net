using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking.Benchmarks;

[MemoryDiagnoser]
public class SerializationBenchmarks
{
    [Params(1_000, 10_000)]
    public int Count;

    private List<Item> _items = default!;
    private IAsyncPagedEnumerable<Item> _asyncPaged = default!;
    private Pagination _pagination;
    private JsonSerializerOptions _options = default!;

    [GlobalSetup]
    public void Setup()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _items = [.. Enumerable.Range(1, Count)
            .Select(i => new Item
            {
                Id = i,
                Name = $"Item-{i}",
                Created = DateTime.UtcNow.AddMinutes(-i),
                Amount = i % 5 == 0 ? null : (decimal?)(i * 1.25m)
            })];

        _pagination = Pagination.Create(
            pageSize: 100,
            currentPage: 1,
            totalCount: _items.Count);

        _asyncPaged = new AsyncPagedEnumerable<Item>(
            GetAsyncSource(_items),
            _ => new ValueTask<Pagination>(_pagination));
    }

    private static async IAsyncEnumerable<Item> GetAsyncSource(IEnumerable<Item> items)
    {
        // simulate async producer
        foreach (var it in items)
        {
            // await Task.Yield(); // keep disabled to minimize scheduler noise
            yield return it;
        }
        await Task.CompletedTask;
    }

    // Baseline: default System.Text.Json serialization of an envelope { pagination, items }
    [Benchmark(Baseline = true)]
    public async Task Default_JsonSerializer_SerializeAsync_Stream()
    {
        using var ms = new MemoryStream(capacity: Math.Max(Count * 64, 4 * 1024));
        var envelope = new PagedEnvelope<Item>(_pagination, _items);
        await JsonSerializer.SerializeAsync(ms, envelope, _options);
        _ = ms.Length; // prevent elimination
    }

    // Test: extension SerializeAsyncPaged writing the same envelope shape using the async-paged source
    [Benchmark]
    public async Task AsyncPaged_SerializeAsync_Stream()
    {
        using var ms = new MemoryStream(capacity: Math.Max(Count * 64, 4 * 1024));
        await JsonSerializer.SerializeAsyncPaged(ms, _asyncPaged, _options);
        _ = ms.Length; // prevent elimination
    }

    // Optional: PipeWriter variant for the async-paged path (closer to high-throughput servers)
    [Benchmark]
    public async Task AsyncPaged_SerializeAsync_PipeWriter()
    {
        var pipe = new Pipe();
        await JsonSerializer.SerializeAsyncPaged(pipe.Writer, _asyncPaged, _options);
        await pipe.Writer.CompleteAsync();
        ReadResult read = await pipe.Reader.ReadAsync();
        _ = read.Buffer.Length; // consume
        pipe.Reader.AdvanceTo(read.Buffer.End);
        await pipe.Reader.CompleteAsync();
    }

    public sealed class PagedEnvelope<T>(Pagination pagination, IReadOnlyList<T> items)
    {
        [JsonPropertyName("pagination")] public Pagination Pagination { get; init; } = pagination;
        [JsonPropertyName("items")] public IReadOnlyList<T> Items { get; init; } = items;
    }

    public sealed class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime Created { get; set; }
        public decimal? Amount { get; set; }
    }
}
