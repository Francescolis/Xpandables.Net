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
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Xpandables.Net.Collections.Extensions;
using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks comparing IAsyncEnumerable and IAsyncPagedEnumerable serialization/deserialization performance.
/// </summary>
/// <remarks>
/// This benchmark evaluates:
/// - Serialization: JsonSerializer.SerializeAsync (IAsyncEnumerable) vs SerializeAsyncPaged (IAsyncPagedEnumerable)
/// - Deserialization: JsonSerializer.DeserializeAsyncEnumerable vs DeserializeAsyncPagedEnumerable (from HttpContent)
/// Tests use realistic data volumes (small, medium, large) and measure memory allocations.
/// </remarks>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[RankColumn]
public class AsyncPagedEnumerableBenchmark
{
    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = SampleDataJsonContext.Default
    };

    private List<SampleData> _smallDataSet = [];
    private List<SampleData> _mediumDataSet = [];
    private List<SampleData> _largeDataSet = [];

    [Params(100, 1000, 10000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _smallDataSet = GenerateSampleData(100);
        _mediumDataSet = GenerateSampleData(1000);
        _largeDataSet = GenerateSampleData(10000);
    }

    #region Serialization Benchmarks

    /// <summary>
    /// Benchmarks standard JsonSerializer.SerializeAsync with IAsyncEnumerable.
    /// </summary>
    [Benchmark(Baseline = true, Description = "Framework IAsyncEnumerable Serialization")]
    public async Task<long> SerializeAsync_IAsyncEnumerable()
    {
        List<SampleData> data = GetDataSetBySize();
        await using MemoryStream stream = new();

        await JsonSerializer.SerializeAsync(
            stream,
            data.ToAsyncEnumerable(),
            SampleDataJsonContext.Default.IAsyncEnumerableSampleData);

        return stream.Length;
    }

    /// <summary>
    /// Benchmarks custom JsonSerializer.SerializeAsyncPaged with IAsyncPagedEnumerable.
    /// </summary>
    [Benchmark(Description = "Custom IAsyncPagedEnumerable Serialization")]
    public async Task<long> SerializeAsync_IAsyncPagedEnumerable()
    {
        List<SampleData> data = GetDataSetBySize();
        await using MemoryStream stream = new();

        IAsyncPagedEnumerable<SampleData> pagedEnumerable = CreatePagedEnumerable(data);

        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            SampleDataJsonContext.Default.SampleData);

        return stream.Length;
    }

    #endregion

    #region Deserialization Benchmarks

    /// <summary>
    /// Benchmarks standard JsonSerializer.DeserializeAsyncEnumerable from HttpContent.
    /// </summary>
    [Benchmark(Description = "Framework IAsyncEnumerable Deserialization (HttpContent)")]
    public async Task<int> DeserializeAsync_IAsyncEnumerable_FromHttpContent()
    {
        HttpContent content = await CreateJsonHttpContent_IAsyncEnumerable();

        IAsyncEnumerable<SampleData?> enumerable = content.ReadFromJsonAsAsyncEnumerable(
            SampleDataJsonContext.Default.SampleData);

        int count = 0;
        await foreach (SampleData? item in enumerable.ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Benchmarks custom DeserializeAsyncPagedEnumerable from HttpContent.
    /// </summary>
    [Benchmark(Description = "Custom IAsyncPagedEnumerable Deserialization (HttpContent)")]
    public async Task<int> DeserializeAsync_IAsyncPagedEnumerable_FromHttpContent()
    {
        HttpContent content = await CreateJsonHttpContent_IAsyncPagedEnumerable();

        IAsyncPagedEnumerable<SampleData?> pagedEnumerable = content
            .ReadFromJsonAsAsyncPagedEnumerable(SampleDataJsonContext.Default.SampleData);

        int count = 0;
        await foreach (SampleData? item in pagedEnumerable)
        {
            count++;
        }

        // Access pagination metadata to ensure it's computed
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();
        _ = pagination.TotalCount;

        return count;
    }

    #endregion

    #region Helper Methods

    private List<SampleData> GetDataSetBySize() => ItemCount switch
    {
        100 => _smallDataSet,
        1000 => _mediumDataSet,
        10000 => _largeDataSet,
        _ => GenerateSampleData(ItemCount)
    };

    private static List<SampleData> GenerateSampleData(int count)
    {
        List<SampleData> data = new(count);
        for (int i = 0; i < count; i++)
        {
            data.Add(new SampleData
            {
                Id = Guid.NewGuid(),
                Name = $"Item_{i}",
                Value = Random.Shared.Next(1, 10000),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Shared.Next(0, 365)),
                IsActive = Random.Shared.Next(0, 2) == 1,
                Tags = [$"tag{i % 10}", $"category{i % 5}", $"type{i % 3}"]
            });
        }
        return data;
    }

    private IAsyncPagedEnumerable<SampleData> CreatePagedEnumerable(List<SampleData> data)
    {
        int pageSize = Math.Min(100, data.Count);
        int currentPage = 1;
        int totalCount = data.Count;

        Pagination pagination = Pagination.Create(
            pageSize: pageSize,
            currentPage: currentPage,
            totalCount: totalCount);

        return AsyncPagedEnumerable.Create(
            data.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(pagination));
    }

    private async Task<HttpContent> CreateJsonHttpContent_IAsyncEnumerable()
    {
        List<SampleData> data = GetDataSetBySize();
        await using MemoryStream stream = new();

        await JsonSerializer.SerializeAsync(
            stream,
            data,
            SampleDataJsonContext.Default.ListSampleData);

        stream.Position = 0;
        byte[] bytes = stream.ToArray();

        return new ByteArrayContent(bytes)
        {
            Headers = { ContentType = new("application/json") }
        };
    }

    private async Task<HttpContent> CreateJsonHttpContent_IAsyncPagedEnumerable()
    {
        List<SampleData> data = GetDataSetBySize();
        await using MemoryStream stream = new();

        IAsyncPagedEnumerable<SampleData> pagedEnumerable = CreatePagedEnumerable(data);

        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            SampleDataJsonContext.Default.SampleData);

        stream.Position = 0;
        byte[] bytes = stream.ToArray();

        return new ByteArrayContent(bytes)
        {
            Headers = { ContentType = new("application/json") }
        };
    }

    #endregion
}

/// <summary>
/// Sample data model for benchmarking.
/// </summary>
public sealed record SampleData
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int Value { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required bool IsActive { get; init; }
    public required string[] Tags { get; init; }
}

/// <summary>
/// Source-generated JSON serialization context for AOT and performance.
/// </summary>
[JsonSerializable(typeof(SampleData))]
[JsonSerializable(typeof(List<SampleData>))]
[JsonSerializable(typeof(IAsyncEnumerable<SampleData>))]
internal partial class SampleDataJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Extension methods to convert collections to IAsyncEnumerable.
/// </summary>
file static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        this IEnumerable<T> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (T item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return item;
        }
    }
}
