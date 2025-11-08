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
using System.Text.Json;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Xpandables.Net.AsyncPaged;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks comparing deserialization performance of paged responses.
/// Tests standard JsonSerializer deserialization against manual async enumerable creation.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AsyncPagedDeserializationBenchmarks
{
    private byte[] _smallDatasetJson = null!;
    private byte[] _mediumDatasetJson = null!;
    private byte[] _largeDatasetJson = null!;

    private JsonSerializerOptions _standardOptions = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _standardOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Generate and serialize test data
        _smallDatasetJson = await GenerateSerializedData(10, 10, 1);
        _mediumDatasetJson = await GenerateSerializedData(100, 20, 1);
        _largeDatasetJson = await GenerateSerializedData(1000, 50, 1);
    }

    #region Small Dataset (10 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Small-Deserialize")]
    public async Task<int> SmallDataset_StandardJsonDeserializer()
    {
        using var stream = new MemoryStream(_smallDatasetJson);
        var result = await JsonSerializer.DeserializeAsync<PagedResponseWrapper<TestProduct>>(
            stream,
            _standardOptions).ConfigureAwait(false);

        return result?.Items.Count ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Small-Deserialize")]
    public async Task<int> SmallDataset_StandardJsonDeserializer_SourceGen()
    {
        using var stream = new MemoryStream(_smallDatasetJson);
        var result = await JsonSerializer.DeserializeAsync(
            stream,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        return result?.Items.Count ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Small-Deserialize")]
    public async Task<int> SmallDataset_ManualAsyncEnumerable()
    {
        using var stream = new MemoryStream(_smallDatasetJson);
        var pagedResult = await DeserializeToAsyncPagedEnumerable(stream).ConfigureAwait(false);

        int count = 0;
        await foreach (var item in pagedResult.ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }

    #endregion

    #region Medium Dataset (100 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Medium-Deserialize")]
    public async Task<int> MediumDataset_StandardJsonDeserializer()
    {
        using var stream = new MemoryStream(_mediumDatasetJson);
        var result = await JsonSerializer.DeserializeAsync<PagedResponseWrapper<TestProduct>>(
            stream,
            _standardOptions).ConfigureAwait(false);

        return result?.Items.Count ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Medium-Deserialize")]
    public async Task<int> MediumDataset_StandardJsonDeserializer_SourceGen()
    {
        using var stream = new MemoryStream(_mediumDatasetJson);
        var result = await JsonSerializer.DeserializeAsync(
            stream,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        return result?.Items.Count ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Medium-Deserialize")]
    public async Task<int> MediumDataset_ManualAsyncEnumerable()
    {
        using var stream = new MemoryStream(_mediumDatasetJson);
        var pagedResult = await DeserializeToAsyncPagedEnumerable(stream).ConfigureAwait(false);

        int count = 0;
        await foreach (var item in pagedResult.ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }

    #endregion

    #region Large Dataset (1000 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Large-Deserialize")]
    public async Task<int> LargeDataset_StandardJsonDeserializer()
    {
        using var stream = new MemoryStream(_largeDatasetJson);
        var result = await JsonSerializer.DeserializeAsync<PagedResponseWrapper<TestProduct>>(
            stream,
            _standardOptions).ConfigureAwait(false);

        return result?.Items.Count ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Large-Deserialize")]
    public async Task<int> LargeDataset_StandardJsonDeserializer_SourceGen()
    {
        using var stream = new MemoryStream(_largeDatasetJson);
        var result = await JsonSerializer.DeserializeAsync(
            stream,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        return result?.Items.Count ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Large-Deserialize")]
    public async Task<int> LargeDataset_ManualAsyncEnumerable()
    {
        using var stream = new MemoryStream(_largeDatasetJson);
        var pagedResult = await DeserializeToAsyncPagedEnumerable(stream).ConfigureAwait(false);

        int count = 0;
        await foreach (var item in pagedResult.ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }

    #endregion

    #region Memory Efficiency Comparison

    [Benchmark]
    [BenchmarkCategory("Memory-Efficiency")]
    public async Task<int> LargeDataset_FullMaterialization()
    {
        using var stream = new MemoryStream(_largeDatasetJson);
        var result = await JsonSerializer.DeserializeAsync(
            stream,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        // Materialize entire list
        return result?.Items.Sum(p => p.Id) ?? 0;
    }

    [Benchmark]
    [BenchmarkCategory("Memory-Efficiency")]
    public async Task<int> LargeDataset_StreamingConsumption()
    {
        using var stream = new MemoryStream(_largeDatasetJson);
        var pagedResult = await DeserializeToAsyncPagedEnumerable(stream).ConfigureAwait(false);

        // Stream items without full materialization
        int sum = 0;
        await foreach (var item in pagedResult.ConfigureAwait(false))
        {
            sum += item.Id;
        }

        return sum;
    }

    #endregion

    #region Helper Methods

    private async Task<byte[]> GenerateSerializedData(int itemCount, int pageSize, int currentPage)
    {
        var items = new List<TestProduct>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(new TestProduct
            {
                Id = i + 1,
                Name = $"Product {i + 1}",
                Description = $"Description for product {i + 1} with some additional text",
                Price = 10.99m + (i * 0.5m),
                Category = $"Category {i % 10}",
                InStock = i % 3 != 0,
                CreatedDate = DateTime.UtcNow.AddDays(-i)
            });
        }

        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = items,
            Pagination = Pagination.Create(pageSize, currentPage, totalCount: itemCount)
        };

        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(
            stream,
            wrapper,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        return stream.ToArray();
    }

    private async Task<IAsyncPagedEnumerable<TestProduct>> DeserializeToAsyncPagedEnumerable(Stream stream)
    {
        var wrapper = await JsonSerializer.DeserializeAsync(
            stream,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        if (wrapper is null)
        {
            return new AsyncPagedEnumerable<TestProduct>(
                AsyncEnumerable.Empty<TestProduct>(),
                _ => ValueTask.FromResult(Pagination.Empty));
        }

        return new AsyncPagedEnumerable<TestProduct>(
            wrapper.Items.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(wrapper.Pagination));
    }

    #endregion
}
