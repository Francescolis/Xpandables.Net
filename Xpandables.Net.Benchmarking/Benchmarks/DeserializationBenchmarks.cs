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
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking.Benchmarks;

using System.IO.Pipelines;
using System.Text.Json;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

[MemoryDiagnoser]
[Config(typeof(AsyncStreamingConfig))]
public class DeserializationBenchmarks
{
    [Params(1_000, 10_000)]
    public int Count;

    private byte[] _jsonBytes = default!;
    private JsonSerializerOptions _options = default!;
    private long _sink;

    [GlobalSetup]
    public void Setup()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var items = Enumerable.Range(1, Count)
            .Select(i => new Item
            {
                Id = i,
                Name = $"Item-{i}",
                Created = DateTime.UtcNow.AddMinutes(-i),
                Amount = i % 5 == 0 ? null : (decimal?)(i * 1.25m)
            })
            .ToArray();

        _jsonBytes = JsonSerializer.SerializeToUtf8Bytes(items, _options);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _jsonBytes = null!;
        _options = null!;
    }

    [Benchmark(Baseline = true)]
    public async Task Stream_Default_DeserializeAsyncEnumerable()
    {
        using var ms = new MemoryStream(_jsonBytes);
        long sum = 0;

        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<Item>(ms, _options))
        {
            if (item is not null)
                sum += item.Id;
        }

        _sink = sum;
    }

    [Benchmark]
    public async Task Stream_AsyncPaged_DeserializeAsyncPagedEnumerable()
    {
        using var ms = new MemoryStream(_jsonBytes);
        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<Item>(ms, _options);

        long sum = 0;
        await foreach (var item in paged)
        {
            if (item is not null)
                sum += item.Id;
        }

        var pagination = await paged.GetPaginationAsync();
        _sink = sum + (pagination.TotalCount ?? 0);
    }

    [Benchmark]
    public async Task PipeReader_Default_DeserializeAsyncEnumerable()
    {
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(_jsonBytes);
        await pipe.Writer.FlushAsync(); // ✅ ensures reader sees data
        await pipe.Writer.CompleteAsync();

        long sum = 0;
        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<Item>(pipe.Reader, _options))
        {
            if (item is not null)
                sum += item.Id;
        }

        await pipe.Reader.CompleteAsync();
        _sink = sum;
    }

    [Benchmark]
    public async Task PipeReader_AsyncPaged_DeserializeAsyncPagedEnumerable()
    {
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(_jsonBytes);
        await pipe.Writer.FlushAsync(); // ✅ ensures reader sees data
        await pipe.Writer.CompleteAsync();

        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<Item>(pipe.Reader, _options);

        long sum = 0;
        await foreach (var item in paged)
        {
            if (item is not null)
                sum += item.Id;
        }

        var pagination = await paged.GetPaginationAsync();
        await pipe.Reader.CompleteAsync();
        _sink = sum + (pagination.TotalCount ?? 0);
    }

    public sealed class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime Created { get; set; }
        public decimal? Amount { get; set; }
    }

    private class AsyncStreamingConfig : ManualConfig
    {
        public AsyncStreamingConfig()
        {
            AddJob(Job.Default
                .WithInvocationCount(1)
                .WithIterationCount(5)
                .WithWarmupCount(1)
                .WithUnrollFactor(1));
        }
    }
}