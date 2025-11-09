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

using FluentAssertions;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.UnitTests.AsyncPaged;

public class JsonDeserializerExtensionsTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions _arrayOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [Fact]
    public async Task DeserializeAsyncPagedEnumerable_Stream_RootArray_Works()
    {
        // Arrange
        var items = Enumerable.Range(1, 5).Select(i => new Item { Id = i, Name = $"N{i}" }).ToList();
        var json = JsonSerializer.SerializeToUtf8Bytes(items, _arrayOptions);

        using var ms = new MemoryStream(json, writable: false);

        // Act
        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<Item>(ms, _options);
        var list = new List<Item?>();
        await foreach (var it in paged)
        {
            list.Add(it);
        }

        // Assert
        list.Should().HaveCount(5);
        list[0]!.Id.Should().Be(1);
        list[^1]!.Name.Should().Be("N5");

        var pagination = await paged.GetPaginationAsync();
        pagination.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task DeserializeAsyncPagedEnumerable_PipeReader_RootArray_Works()
    {
        // Arrange
        var items = Enumerable.Range(1, 3).Select(i => new Item { Id = i }).ToList();
        var json = JsonSerializer.SerializeToUtf8Bytes(items, _arrayOptions);

        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(json);
        await pipe.Writer.CompleteAsync();

        // Act
        var paged = JsonSerializer.DeserializeAsyncPagedEnumerable<Item>(pipe.Reader, _options);
        var list = new List<Item?>();
        await foreach (var it in paged)
        {
            list.Add(it);
        }

        // Assert
        list.Should().HaveCount(3);
        (await paged.GetPaginationAsync()).TotalCount.Should().Be(3);

        await pipe.Reader.CompleteAsync();
    }

    private sealed class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
