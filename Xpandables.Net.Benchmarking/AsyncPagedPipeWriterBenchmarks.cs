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
using System.IO.Pipelines;
using System.Text.Json;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking;

/// <summary>
/// Benchmarks comparing PipeWriter-based serialization vs Stream-based serialization.
/// Focuses on high-performance scenarios where System.IO.Pipelines provides benefits.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AsyncPagedPipeWriterBenchmarks
{
    private List<TestProduct> _smallDataset = null!;
    private List<TestProduct> _mediumDataset = null!;
    private List<TestProduct> _largeDataset = null!;

    private IAsyncPagedEnumerable<TestProduct> _smallPaged = null!;
    private IAsyncPagedEnumerable<TestProduct> _mediumPaged = null!;
    private IAsyncPagedEnumerable<TestProduct> _largePaged = null!;

    private Pipe _pipe = null!;
    private MemoryStream _stream = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallDataset = GenerateTestData(10);
        _mediumDataset = GenerateTestData(100);
        _largeDataset = GenerateTestData(1000);

        _smallPaged = CreatePagedEnumerable(_smallDataset, pageSize: 10, currentPage: 1);
        _mediumPaged = CreatePagedEnumerable(_mediumDataset, pageSize: 20, currentPage: 1);
        _largePaged = CreatePagedEnumerable(_largeDataset, pageSize: 50, currentPage: 1);

        _pipe = new Pipe(new PipeOptions(
            pauseWriterThreshold: 1024 * 1024, // 1MB
            resumeWriterThreshold: 512 * 1024  // 512KB
        ));

        _stream = new MemoryStream(1024 * 1024); // 1MB buffer
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stream?.Dispose();
    }

    #region PipeWriter vs Stream - Small Dataset

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Small-Transport")]
    public async Task<long> SmallDataset_Stream_AsyncPaged()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _smallPaged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return _stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Small-Transport")]
    public async Task<long> SmallDataset_PipeWriter_AsyncPaged()
    {
        _pipe = new Pipe();

        var writeTask = Task.Run(async () =>
        {
            await JsonSerializer.SerializeAsyncPaged(
                _pipe.Writer,
                _smallPaged,
                TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

            await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        });

        long totalBytes = 0;
        await foreach (var result in _pipe.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            totalBytes += result.Buffer.Length;
            _pipe.Reader.AdvanceTo(result.Buffer.End);
        }

        await writeTask.ConfigureAwait(false);
        await _pipe.Reader.CompleteAsync().ConfigureAwait(false);

        return totalBytes;
    }

    #endregion

    #region PipeWriter vs Stream - Medium Dataset

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Medium-Transport")]
    public async Task<long> MediumDataset_Stream_AsyncPaged()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _mediumPaged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return _stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Medium-Transport")]
    public async Task<long> MediumDataset_PipeWriter_AsyncPaged()
    {
        _pipe = new Pipe();

        var writeTask = Task.Run(async () =>
        {
            await JsonSerializer.SerializeAsyncPaged(
                _pipe.Writer,
                _mediumPaged,
                TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

            await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        });

        long totalBytes = 0;
        await foreach (var result in _pipe.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            totalBytes += result.Buffer.Length;
            _pipe.Reader.AdvanceTo(result.Buffer.End);
        }

        await writeTask.ConfigureAwait(false);
        await _pipe.Reader.CompleteAsync().ConfigureAwait(false);

        return totalBytes;
    }

    #endregion

    #region PipeWriter vs Stream - Large Dataset

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Large-Transport")]
    public async Task<long> LargeDataset_Stream_AsyncPaged()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        await JsonSerializer.SerializeAsyncPaged(
            _stream,
            _largePaged,
            TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

        return _stream.Position;
    }

    [Benchmark]
    [BenchmarkCategory("Large-Transport")]
    public async Task<long> LargeDataset_PipeWriter_AsyncPaged()
    {
        _pipe = new Pipe();

        var writeTask = Task.Run(async () =>
        {
            await JsonSerializer.SerializeAsyncPaged(
                _pipe.Writer,
                _largePaged,
                TestProductSourceGenerationContext.Default.TestProduct).ConfigureAwait(false);

            await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        });

        long totalBytes = 0;
        await foreach (var result in _pipe.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            totalBytes += result.Buffer.Length;
            _pipe.Reader.AdvanceTo(result.Buffer.End);
        }

        await writeTask.ConfigureAwait(false);
        await _pipe.Reader.CompleteAsync().ConfigureAwait(false);

        return totalBytes;
    }

    #endregion

    #region Standard JsonSerializer - PipeWriter Comparison

    [Benchmark]
    [BenchmarkCategory("Comparison-Medium")]
    public async Task<long> MediumDataset_PipeWriter_Standard()
    {
        _pipe = new Pipe();

        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = _mediumDataset,
            Pagination = Pagination.Create(pageSize: 20, currentPage: 1, totalCount: _mediumDataset.Count)
        };

        var writeTask = Task.Run(async () =>
        {
            await JsonSerializer.SerializeAsync(
                _pipe.Writer.AsStream(),
                wrapper,
                TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

            await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        });

        long totalBytes = 0;
        await foreach (var result in _pipe.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            totalBytes += result.Buffer.Length;
            _pipe.Reader.AdvanceTo(result.Buffer.End);
        }

        await writeTask.ConfigureAwait(false);
        await _pipe.Reader.CompleteAsync().ConfigureAwait(false);

        return totalBytes;
    }

    [Benchmark]
    [BenchmarkCategory("Comparison-Medium")]
    public async Task<long> MediumDataset_Stream_Standard()
    {
        _stream.Position = 0;
        _stream.SetLength(0);

        var wrapper = new PagedResponseWrapper<TestProduct>
        {
            Items = _mediumDataset,
            Pagination = Pagination.Create(pageSize: 20, currentPage: 1, totalCount: _mediumDataset.Count)
        };

        await JsonSerializer.SerializeAsync(
            _stream,
            wrapper,
            TestProductSourceGenerationContext.Default.PagedResponseWrapperTestProduct).ConfigureAwait(false);

        return _stream.Position;
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
                Description = $"Description for product {i + 1} with additional text",
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

/// <summary>
/// Extension methods for PipeReader to support async enumeration.
/// </summary>
internal static class PipeReaderExtensions
{
    public static async IAsyncEnumerable<ReadResult> ReadAllAsync(this PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync().ConfigureAwait(false);

            if (result.IsCanceled)
            {
                break;
            }

            yield return result;

            if (result.IsCompleted)
            {
                break;
            }
        }
    }
}
