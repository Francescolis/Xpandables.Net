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
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking.Benchmarks;

[MemoryDiagnoser]
public class DeserializationBenchmarks
{
    [Params(1_000, 10_000)]
    public int Count;

    private byte[] _jsonArrayBytes = default!;
    private JsonSerializerOptions _options = default!;

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
            .ToList();

        _jsonArrayBytes = JsonSerializer.SerializeToUtf8Bytes(items, _options);
    }

    [Benchmark(Baseline = true)]
    public List<Item>? Default_JsonSerializer_Deserialize_List()
    {
        using var ms = new MemoryStream(_jsonArrayBytes, writable: false);
        var list = JsonSerializer.Deserialize<List<Item>>(ms, _options);
        return list; // materialized
    }

    [Benchmark]
    public async Task AsyncPaged_Deserialize_Stream_ToList()
    {
        using var ms = new MemoryStream(_jsonArrayBytes, writable: false);
        IAsyncPagedEnumerable<Item?> asyncPaged = JsonSerializer.DeserializeAsyncPagedEnumerable<Item>(ms, _options);
        var result = new List<Item?>(Count);
        await foreach (var item in asyncPaged)
        {
            result.Add(item);
        }
        _ = result.Count; // prevent elimination
    }

    [Benchmark]
    public async Task AsyncPaged_Deserialize_PipeReader_ToList()
    {
        // Use Pipe to simulate high-throughput server scenario
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(_jsonArrayBytes);
        await pipe.Writer.CompleteAsync();

        IAsyncPagedEnumerable<Item?> asyncPaged = JsonSerializer.DeserializeAsyncPagedEnumerable<Item>(pipe.Reader, _options);
        var result = new List<Item?>(Count);
        await foreach (var item in asyncPaged)
        {
            result.Add(item);
        }
        _ = result.Count;
        await pipe.Reader.CompleteAsync();
    }

    public sealed class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime Created { get; set; }
        public decimal? Amount { get; set; }
    }
}
