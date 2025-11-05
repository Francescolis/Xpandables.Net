using System.IO.Pipelines;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using BenchmarkDotNet.Attributes;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks comparing serialization and deserialization performance of:
/// - IAsyncEnumerable (baseline)
/// - IAsyncPagedEnumerable (Stream and PipeWriter)
/// - HttpContent extensions for both approaches
/// </summary>
[MemoryDiagnoser]
[ThreadingDiagnoser]
[HideColumns("Job", "Error", "RatioSD")]
public partial class JsonStreamingBenchmarks
{
    [Params(1_000, 100_000)]
    public int Count;

    private HttpContent _arrayRootContent = default!;
    private HttpContent _pagedContent = default!;
    private JsonSerializerOptions _options = default!;
    private List<PayloadItem> _items = default!;

    [JsonSerializable(typeof(PayloadItem[]))]
    [JsonSerializable(typeof(PayloadItem))]
    [JsonSerializable(typeof(Pagination))]
    public partial class PayloadItemContext : JsonSerializerContext { }

    public record PayloadItem(int Id, string Name, bool Flag);

    [GlobalSetup]
    public void Setup()
    {
        _items = Enumerable.Range(1, Count)
            .Select(i => new PayloadItem(i, "Name" + i, (i & 1) == 0))
            .ToList();

        // Setup for standard array JSON
        var json = JsonSerializer.Serialize(_items);
        _arrayRootContent = new StringContent(json, Encoding.UTF8, "application/json");

        // Setup for paged JSON format: { "pagination": {...}, "items": [...] }
        var pagedJson = CreatePagedJson(_items);
        _pagedContent = new StringContent(pagedJson, Encoding.UTF8, "application/json");

        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                PayloadItemContext.Default,
                PaginationSourceGenerationContext.Default)
        };
    }

    private static string CreatePagedJson(List<PayloadItem> items)
    {
        var pagination = Pagination.Create(
            pageSize: items.Count,
            currentPage: 1,
            totalCount: items.Count);

        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("pagination"u8);
            JsonSerializer.Serialize(writer, pagination, PaginationSourceGenerationContext.Default.Pagination);
            writer.WritePropertyName("items"u8);
            JsonSerializer.Serialize(writer, items, PayloadItemContext.Default.PayloadItemArray);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        // Recreate content for each iteration to ensure fresh stream
        _arrayRootContent?.Dispose();
        _pagedContent?.Dispose();

        var json = JsonSerializer.Serialize(_items);
        _arrayRootContent = new StringContent(json, Encoding.UTF8, "application/json");

        var pagedJson = CreatePagedJson(_items);
        _pagedContent = new StringContent(pagedJson, Encoding.UTF8, "application/json");
    }

    #region Deserialization Benchmarks

    [Benchmark(Baseline = true, Description = "Baseline: IAsyncEnumerable from HttpContent")]
    public async Task<long> Deserialize_IAsyncEnumerable_HttpContent()
    {
        var stream = _arrayRootContent.ReadFromJsonAsAsyncEnumerable<PayloadItem>(_options);
        long count = 0;
        await foreach (var _ in stream)
            count++;
        return count;
    }

    [Benchmark(Description = "IAsyncPagedEnumerable from HttpContent (array)")]
    public async Task<long> Deserialize_IAsyncPagedEnumerable_HttpContent_Array()
    {
        var paged = await _arrayRootContent.ReadFromJsonAsAsyncPagedEnumerable<PayloadItem>(_options);
        var pagination = paged.Pagination;
        long count = 0;
        await foreach (var _ in paged)
            count++;
        GC.KeepAlive(pagination);
        return count;
    }

    [Benchmark(Description = "IAsyncPagedEnumerable from HttpContent (paged format)")]
    public async Task<long> Deserialize_IAsyncPagedEnumerable_HttpContent_Paged()
    {
        var stream = await _pagedContent.ReadAsStreamAsync();
        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<PayloadItem>(
            stream,
            PayloadItemContext.Default.PayloadItem);
        var pagination = await paged.GetPaginationAsync();
        long count = 0;
        await foreach (var _ in paged)
            count++;
        GC.KeepAlive(pagination);
        return count;
    }

    [Benchmark(Description = "IAsyncPagedEnumerable from Stream")]
    public async Task<long> Deserialize_IAsyncPagedEnumerable_Stream()
    {
        var stream = await _arrayRootContent.ReadAsStreamAsync();
        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<PayloadItem>(
            stream,
            PayloadItemContext.Default.PayloadItem);
        var pagination = paged.Pagination;
        long count = 0;
        await foreach (var _ in paged)
            count++;
        GC.KeepAlive(pagination);
        return count;
    }

    [Benchmark(Description = "IAsyncPagedEnumerable from PipeReader")]
    public async Task<long> Deserialize_IAsyncPagedEnumerable_PipeReader()
    {
        var stream = await _arrayRootContent.ReadAsStreamAsync();
        var pipe = PipeReader.Create(stream);
        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<PayloadItem>(
            pipe,
            PayloadItemContext.Default.PayloadItem);
        var pagination = paged.Pagination;
        long count = 0;
        await foreach (var _ in paged)
            count++;
        GC.KeepAlive(pagination);
        return count;
    }

    #endregion

    #region Serialization Benchmarks

    [Benchmark(Description = "IAsyncEnumerable to Stream (baseline)")]
    public async Task<long> Serialize_IAsyncEnumerable_Stream()
    {
        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, _items, PayloadItemContext.Default.PayloadItemArray);
        return ms.Length;
    }

    [Benchmark(Description = "IAsyncPagedEnumerable to Stream")]
    public async Task<long> Serialize_IAsyncPagedEnumerable_Stream()
    {
        var paged = CreateAsyncPagedEnumerable(_items);
        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsyncPaged(
            ms,
            paged,
            PayloadItemContext.Default.PayloadItem);
        return ms.Length;
    }

    [Benchmark(Description = "IAsyncPagedEnumerable to PipeWriter")]
    public async Task<long> Serialize_IAsyncPagedEnumerable_PipeWriter()
    {
        var paged = CreateAsyncPagedEnumerable(_items);
        var pipe = new Pipe();
        var writeTask = JsonSerializer.SerializeAsyncPaged(
            pipe.Writer,
            paged,
            PayloadItemContext.Default.PayloadItem);

        // Complete writing
        await writeTask;
        await pipe.Writer.CompleteAsync();

        // Read to measure size
        long totalBytes = 0;
        while (true)
        {
            var result = await pipe.Reader.ReadAsync();
            totalBytes += result.Buffer.Length;
            pipe.Reader.AdvanceTo(result.Buffer.End);
            if (result.IsCompleted)
                break;
        }
        await pipe.Reader.CompleteAsync();
        return totalBytes;
    }

    #endregion

    #region Helper Methods

    private IAsyncPagedEnumerable<PayloadItem> CreateAsyncPagedEnumerable(List<PayloadItem> items)
    {
        var pagination = Pagination.Create(
            pageSize: items.Count,
            currentPage: 1,
            totalCount: items.Count);

        return new AsyncPagedEnumerable<PayloadItem>(
            ToAsyncEnumerable(items),
            _ => new ValueTask<Pagination>(pagination));
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield(); // Simulate async behavior
        }
    }

    #endregion
}
