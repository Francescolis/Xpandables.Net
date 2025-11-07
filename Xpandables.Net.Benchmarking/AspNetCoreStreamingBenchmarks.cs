/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using BenchmarkDotNet.Attributes;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Controllers;
using Xpandables.Net.AsyncPaged.Extensions;
using Xpandables.Net.AsyncPaged.Minimals;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks comparing ASP.NET Core integration performance for:
/// - Controller-based output formatting with AsyncPagedEnumerableJsonOutputFormatter
/// - Minimal API with AsyncPagedEnumerableResult
/// - Standard JSON serialization (baseline)
/// </summary>
[MemoryDiagnoser]
[ThreadingDiagnoser]
[HideColumns("Job", "Error", "RatioSD")]
public partial class AspNetCoreStreamingBenchmarks
{
    [Params(1_000, 50_000)]
    public int Count;

    private List<DataItem> _items = default!;
    private DefaultHttpContext _httpContext = default!;
    private JsonSerializerOptions _options = default!;
    private AsyncPagedEnumerableJsonOutputFormatter _formatter = default!;
    private IServiceProvider _serviceProvider = default!;

    [JsonSerializable(typeof(DataItem[]))]
    [JsonSerializable(typeof(DataItem))]
    [JsonSerializable(typeof(Pagination))]
    public partial class DataItemContext : JsonSerializerContext { }

    public record DataItem(int Id, string Name, DateTime Timestamp, decimal Value);

    [GlobalSetup]
    public void Setup()
    {
        _items = [.. Enumerable.Range(1, Count)
            .Select(i => new DataItem(
                i,
                $"Item_{i}",
                DateTime.UtcNow.AddSeconds(i),
                i * 1.5m))];

        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                DataItemContext.Default,
                PaginationSourceGenerationContext.Default)
        };

        _options.MakeReadOnly(true);

        _formatter = new AsyncPagedEnumerableJsonOutputFormatter(_options);

        var services = new ServiceCollection();
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
        {
            opts.SerializerOptions.PropertyNameCaseInsensitive = _options.PropertyNameCaseInsensitive;
            opts.SerializerOptions.TypeInfoResolver = _options.TypeInfoResolver;
            opts.SerializerOptions.MakeReadOnly(true);
        });
        _serviceProvider = services.BuildServiceProvider();

        SetupHttpContext();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        SetupHttpContext();
    }

    private void SetupHttpContext()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        _httpContext.RequestServices = _serviceProvider;
    }

    #region Controller-based Benchmarks

    [Benchmark(Baseline = true, Description = "Baseline: Standard JSON array serialization")]
    public async Task<long> Controller_StandardJson_Array()
    {
        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        // Don't use httpContext.RequestAborted in benchmarks - it causes issues
        await JsonSerializer.SerializeAsync(
            ms,
            [.. _items],
            DataItemContext.Default.DataItemArray);

        return ms.Length;
    }

    [Benchmark(Description = "Controller: AsyncPagedEnumerableJsonOutputFormatter")]
    public async Task<long> Controller_AsyncPagedFormatter()
    {
        var paged = CreateAsyncPagedEnumerable(_items);

        var context = new OutputFormatterWriteContext(
            _httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(IAsyncPagedEnumerable<DataItem>),
            paged);

        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        await _formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        return ms.Length;
    }

    [Benchmark(Description = "Controller: Direct Stream serialization")]
    public async Task<long> Controller_DirectStreamSerialization()
    {
        var paged = CreateAsyncPagedEnumerable(_items);

        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        await JsonSerializer.SerializeAsyncPaged(
            ms,
            paged,
            DataItemContext.Default.DataItem);

        return ms.Length;
    }

    #endregion

    #region Minimal API Benchmarks

    [Benchmark(Description = "MinimalAPI: AsyncPagedEnumerableResult with JsonTypeInfo")]
    public async Task<long> MinimalAPI_AsyncPagedResult_JsonTypeInfo()
    {
        var paged = CreateAsyncPagedEnumerable(_items);
        var result = new AsyncPagedEnumerableResult<DataItem>(
            paged,
            DataItemContext.Default.DataItem);

        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        await result.ExecuteAsync(_httpContext);

        return ms.Length;
    }

    [Benchmark(Description = "MinimalAPI: AsyncPagedEnumerableResult with JsonOptions")]
    public async Task<long> MinimalAPI_AsyncPagedResult_JsonOptions()
    {
        var paged = CreateAsyncPagedEnumerable(_items);
        var result = new AsyncPagedEnumerableResult<DataItem>(
            paged,
            _options);

        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        await result.ExecuteAsync(_httpContext);

        return ms.Length;
    }

    [Benchmark(Description = "MinimalAPI: AsyncPagedEnumerableResult auto-resolving")]
    public async Task<long> MinimalAPI_AsyncPagedResult_AutoResolve()
    {
        var paged = CreateAsyncPagedEnumerable(_items);
        var result = new AsyncPagedEnumerableResult<DataItem>(paged);

        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        await result.ExecuteAsync(_httpContext);

        return ms.Length;
    }

    [Benchmark(Description = "MinimalAPI: PipeWriter with BodyWriter")]
    public async Task<long> MinimalAPI_PipeWriter()
    {
        var paged = CreateAsyncPagedEnumerable(_items);

        var ms = (MemoryStream)_httpContext.Response.Body;
        ms.SetLength(0);
        ms.Position = 0;

        // Use PipeWriter from Response.BodyWriter
        var pipeWriter = _httpContext.Response.BodyWriter;

        await JsonSerializer.SerializeAsyncPaged(
            pipeWriter,
            paged,
            DataItemContext.Default.DataItem);

        // Don't flush explicitly - let the serializer handle it
        // This matches the pattern in MinimalAPI results

        return ms.Length;
    }

    #endregion

    #region Memory Allocation Comparison

    [Benchmark(Description = "Memory: IAsyncEnumerable enumeration")]
    public async Task<int> Memory_IAsyncEnumerable_Enumeration()
    {
        var asyncEnumerable = ToAsyncEnumerable(_items);
        int count = 0;
        await foreach (var _ in asyncEnumerable)
        {
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Memory: IAsyncPagedEnumerable enumeration")]
    public async Task<int> Memory_IAsyncPagedEnumerable_Enumeration()
    {
        var paged = CreateAsyncPagedEnumerable(_items);
        var pagination = await paged.GetPaginationAsync();
        int count = 0;
        await foreach (var _ in paged)
        {
            count++;
        }
        GC.KeepAlive(pagination);
        return count;
    }

    #endregion

    #region Helper Methods

    private IAsyncPagedEnumerable<DataItem> CreateAsyncPagedEnumerable(List<DataItem> items)
    {
        var pagination = Pagination.Create(
            pageSize: items.Count,
            currentPage: 1,
            totalCount: items.Count);

        return new AsyncPagedEnumerable<DataItem>(
            ToAsyncEnumerable(items),
            _ => new ValueTask<Pagination>(pagination));
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            // PERFORMANCE: Remove await to reduce async overhead in benchmarks
            // This makes the enumeration synchronous but maintains the IAsyncEnumerable interface
        }

        // Ensure compiler treats this as async
        await Task.CompletedTask;
    }

    #endregion
}
