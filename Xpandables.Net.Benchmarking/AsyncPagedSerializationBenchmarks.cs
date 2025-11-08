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
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks comparing IAsyncPagedEnumerable serialization with standard JsonSerializer.
/// Tests various dataset sizes and serialization approaches.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AsyncPagedSerializationBenchmarks
{
    private List<TestProduct> _smallDataset = null!;
    private List<TestProduct> _mediumDataset = null!;
    private List<TestProduct> _largeDataset = null!;

    private IAsyncPagedEnumerable<TestProduct> _smallPaged = null!;
    private IAsyncPagedEnumerable<TestProduct> _mediumPaged = null!;
    private IAsyncPagedEnumerable<TestProduct> _largePaged = null!;

    private JsonSerializerOptions _standardOptions = null!;
    private MemoryStream _stream = null!;

    [Params(10, 100, 1000, 10000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _smallDataset = GenerateTestData(10);
        _mediumDataset = GenerateTestData(100);
        _largeDataset = GenerateTestData(1000);

        _smallPaged = CreatePagedEnumerable(_smallDataset, pageSize: 10, currentPage: 1);
        _mediumPaged = CreatePagedEnumerable(_mediumDataset, pageSize: 20, currentPage: 1);
        _largePaged = CreatePagedEnumerable(_largeDataset, pageSize: 50, currentPage: 1);

        _standardOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _stream = new MemoryStream(1024 * 1024); // 1MB buffer
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stream?.Dispose();
    }

    #region Small Dataset (10 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Small-Serialize")]
    public async Task<int> SmallDataset_StandardJsonSerializer()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = _smallDataset,
            Pagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: _smallDataset.Count)
        };

        await JsonSerializer.SerializeAsync(_stream, wrapper, _standardOptions).ConfigureAwait(false);
        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Small-Serialize")]
    public async Task<int> SmallDataset_AsyncPagedEnumerable_WithOptions()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _smallPaged,
            _standardOptions).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Small-Serialize")]
    public async Task<int> SmallDataset_AsyncPagedEnumerable_WithTypeInfo()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _smallPaged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    #endregion

    #region Medium Dataset (100 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Medium-Serialize")]
    public async Task<int> MediumDataset_StandardJsonSerializer()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = _mediumDataset,
            Pagination = Pagination.Create(pageSize: 20, currentPage: 1, totalCount: _mediumDataset.Count)
        };

        await JsonSerializer.SerializeAsync(_stream, wrapper, _standardOptions).ConfigureAwait(false);
        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Medium-Serialize")]
    public async Task<int> MediumDataset_AsyncPagedEnumerable_WithOptions()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _mediumPaged,
            _standardOptions).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Medium-Serialize")]
    public async Task<int> MediumDataset_AsyncPagedEnumerable_WithTypeInfo()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _mediumPaged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    #endregion

    #region Large Dataset (1000 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Large-Serialize")]
    public async Task<int> LargeDataset_StandardJsonSerializer()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = _largeDataset,
            Pagination = Pagination.Create(pageSize: 50, currentPage: 1, totalCount: _largeDataset.Count)
        };

        await JsonSerializer.SerializeAsync(_stream, wrapper, _standardOptions).ConfigureAwait(false);
        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Large-Serialize")]
    public async Task<int> LargeDataset_AsyncPagedEnumerable_WithOptions()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _largePaged,
            _standardOptions).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Large-Serialize")]
    public async Task<int> LargeDataset_AsyncPagedEnumerable_WithTypeInfo()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _largePaged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    #endregion

    #region Memory Efficiency Tests

    [Benchmark]
    [BenchmarkCategory("Memory-Efficiency")]
    public async Task<int> StreamingPaged_10K_Items()
    {
        var data = GenerateTestData(10000);
        var paged = CreatePagedEnumerable(data, pageSize: 100, currentPage: 1);

        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            paged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Memory-Efficiency")]
    public async Task<int> StandardList_10K_Items()
    {
        var data = GenerateTestData(10000);
        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = data,
            Pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: data.Count)
        };

        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsync(
            _stream,
            wrapper,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        return (int)_stream.Position;
    }

    #endregion

    #region Helper Methods

    private static List<TestProduct> GenerateTestData(int count)
    {
        var products = new List<TestProduct>(count);
        for (int i = 0; i < count; i++)
        {
            products.Add(new TestProduct
            {
                Id = i + 1,
                Name = $"Product {i + 1}",
                Description = $"Description for product {i + 1} with some additional text to simulate real data",
                Price = 10.99m + (i * 0.5m),
                Category = $"Category {i % 10}",
                InStock = i % 3 != 0,
                CreatedDate = DateTime.UtcNow.AddDays(-i)
            });
        }
        return products;
    }

    private static IAsyncPagedEnumerable<TestProduct> CreatePagedEnumerable(
        List<TestProduct> items,
        int pageSize,
        int currentPage)
    {
        return new AsyncPagedEnumerable<TestProduct>(
            items.ToAsyncEnumerable(),
            _ => ValueTask.FromResult(
                Pagination.Create(
                    pageSize: pageSize,
                    currentPage: currentPage,
                    totalCount: items.Count)));
    }

    #endregion
}

#region Test Models

/// <summary>
/// Test product model for benchmarking.
/// </summary>
public sealed class TestProduct
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public required string Category { get; set; }
    public bool InStock { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Wrapper class to simulate standard paged response format.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public sealed class PagedResponseWrapper<T>
{
    public required List<T> Items { get; set; }
    public required Pagination Pagination { get; set; }
}

/// <summary>
/// Source generation context for AOT-friendly serialization.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TestProduct))]
[JsonSerializable(typeof(PagedResponseWrapper<TestProduct>))]
[JsonSerializable(typeof(List<TestProduct>))]
[JsonSerializable(typeof(Pagination))]
public partial class TestProductSourceGenerationContext : JsonSerializerContext
{
}

#endregion
