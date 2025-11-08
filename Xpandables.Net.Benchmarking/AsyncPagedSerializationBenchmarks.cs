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
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using BenchmarkDotNet.Attributes;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.Benchmarking;

[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class AsyncPagedSerializationBenchmarks
{
    private readonly SamplePagedEnumerable _pagedEnumerable = new();
    private readonly PipeWriter _pipeWriter = new Pipe().Writer;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = false };
    private readonly JsonTypeInfo<string> _typeInfo = PaginationSourceGenerationContext.Default.String;
    private readonly JsonSerializerContext _context = PaginationSourceGenerationContext.Default;

    [Benchmark(Baseline = true)]
    public async Task SerializeWithJsonTypeInfo()
    {
        await JsonSerializerExtensions.SerializeAsyncPaged(
            _pipeWriter, _pagedEnumerable, _typeInfo);
    }

    [Benchmark]
    public async Task SerializeWithJsonSerializerOptions()
    {
        await JsonSerializerExtensions.SerializeAsyncPaged(
            _pipeWriter, _pagedEnumerable, _options);
    }

    [Benchmark]
    public async Task SerializeWithJsonSerializerContext()
    {
        await JsonSerializerExtensions.SerializeAsyncPaged(
            _pipeWriter, (IAsyncPagedEnumerable)_pagedEnumerable, _context);
    }
}
