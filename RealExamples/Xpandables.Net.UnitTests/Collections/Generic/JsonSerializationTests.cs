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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Collections.Generic;

/// <summary>
/// Unit tests for JSON serialization and deserialization of paged enumerables.
/// </summary>
public sealed class JsonSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializationTests() => _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = TestDataJsonContext.Default
    };

    [Fact]
    public async Task SerializeAsyncPaged_WithStream_ShouldProduceValidJson()
    {
        // Arrange
        var items = Enumerable.Range(0, 10)
            .Select(i => new TestItem { Id = i, Name = $"Item_{i}" })
            .ToList();
		IAsyncEnumerable<TestItem> source = items.ToAsyncEnumerable();
        var pagination = Pagination.Create(pageSize: 5, currentPage: 1, totalCount: 10);
		IAsyncPagedEnumerable<TestItem> paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        await using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, paged, _options);

        // Assert
        stream.Position = 0;
		string json = await new StreamReader(stream).ReadToEndAsync();

        json.Should()
            .NotBeEmpty("JSON output should not be empty")
            .And.Contain("\"pagination\"", "JSON should contain pagination property")
            .And.Contain("\"items\"", "JSON should contain items array");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithPipeWriter_ShouldProduceValidJson()
    {
        // Arrange
        var items = Enumerable.Range(0, 5)
            .Select(i => new TestItem { Id = i, Name = $"Item_{i}" })
            .ToList();
		IAsyncEnumerable<TestItem> source = items.ToAsyncEnumerable();
        var pagination = Pagination.Create(pageSize: 5, currentPage: 1, totalCount: 5);
		IAsyncPagedEnumerable<TestItem> paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        var pipe = new Pipe();

        // Act
        _ = Task.Run(async () =>
        {
            await JsonSerializer.SerializeAsyncPaged(pipe.Writer, paged, _options);
            await pipe.Writer.CompleteAsync();
        });

        // Assert
        var reader = new StreamReader(pipe.Reader.AsStream());
		string json = await reader.ReadToEndAsync();

        json.Should()
            .NotBeEmpty("JSON output should not be empty")
            .And.Contain("\"pagination\"", "JSON should contain pagination property")
            .And.Contain("\"items\"", "JSON should contain items array");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithLargeDataset_ShouldHandleEfficiently()
    {
        // Arrange
        const int itemCount = 1000;
        var items = Enumerable.Range(0, itemCount)
            .Select(i => new TestItem { Id = i, Name = $"Item_{i}" })
            .ToList();
		IAsyncEnumerable<TestItem> source = items.ToAsyncEnumerable();
        var pagination = Pagination.Create(pageSize: 100, currentPage: 1, totalCount: itemCount);
		IAsyncPagedEnumerable<TestItem> paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(pagination));

        await using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, paged, _options);

        // Assert
        stream.Length.Should()
            .BeGreaterThan(0, "serialized JSON should have content");
    }

    [Fact]
    public async Task DeserializeAsyncPagedEnumerable_FromJson_ShouldRestorePaginationAndItems()
    {
		// Arrange
		string json = """
        {
            "pagination": {
                "pageSize": 5,
                "currentPage": 1,
                "totalCount": 10,
                "continuationToken": null
            },
            "items": [
                {"id": 0, "name": "Item_0"},
                {"id": 1, "name": "Item_1"},
                {"id": 2, "name": "Item_2"}
            ]
        }
        """;
		Pipe pipe = CreatePipeWithJson(json);

		JsonTypeInfo<TestItem> typeInfo = TestDataJsonContext.Default.TestItem;
		IAsyncPagedEnumerable<TestItem?> paged = JsonSerializer.DeserializeAsyncPagedEnumerable<TestItem>(
            pipe.Reader,
            typeInfo,
            cancellationToken: CancellationToken.None);

		// Act
		List<TestItem?> items = await paged.ToListAsync();
		Pagination pagination = await paged.GetPaginationAsync();

        // Assert
        items.Should()
            .HaveCount(3, "deserialized items should match JSON array")
            .And.AllSatisfy(item =>
            {
                item.Should().NotBeNull();
                item.Id.Should().BeGreaterThanOrEqualTo(0);
                item.Name.Should().NotBeNullOrEmpty();
            });

        items[0]!.Id.Should().Be(0, "first item should have id 0");
        items[0]!.Name.Should().Be("Item_0", "first item should have correct name");
        items[1]!.Id.Should().Be(1, "second item should have id 1");
        items[1]!.Name.Should().Be("Item_1", "second item should have correct name");
        items[2]!.Id.Should().Be(2, "third item should have id 2");
        items[2]!.Name.Should().Be("Item_2", "third item should have correct name");

        pagination.PageSize.Should().Be(5, "pagination pageSize should match");
        pagination.CurrentPage.Should().Be(1, "pagination currentPage should match");
        pagination.TotalCount.Should().Be(10, "pagination totalCount should match");
    }

    [Fact]
    public async Task DeserializeAsyncPagedEnumerable_EmptyItems_ShouldReturnEmptyList()
    {
		// Arrange
		string json = """
        {
            "pagination": {
                "pageSize": 0,
                "currentPage": 0,
                "totalCount": 0,
                "continuationToken": null
            },
            "items": []
        }
        """;
		Pipe pipe = CreatePipeWithJson(json);

		JsonTypeInfo<TestItem> typeInfo = TestDataJsonContext.Default.TestItem;
		IAsyncPagedEnumerable<TestItem?> paged = JsonSerializer.DeserializeAsyncPagedEnumerable<TestItem>(
            pipe.Reader,
            typeInfo,
            cancellationToken: CancellationToken.None);

		// Act
		List<TestItem?> items = await paged.ToListAsync();

        // Assert
        items.Should()
            .BeEmpty("deserialized items should be empty");
    }

    [Fact]
    public async Task RoundTrip_SerializeDeserialize_ShouldPreserveData()
    {
        // Arrange
        var originalItems = Enumerable.Range(0, 20)
            .Select(i => new TestItem { Id = i, Name = $"Item_{i}" })
            .ToList();
        var originalPagination = Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 20);

        // Act - Serialize
        await using var serializeStream = new MemoryStream();
		IAsyncEnumerable<TestItem> source = originalItems.ToAsyncEnumerable();
		IAsyncPagedEnumerable<TestItem> paged = AsyncPagedEnumerable.Create(source, _ => ValueTask.FromResult(originalPagination));
        await JsonSerializer.SerializeAsyncPaged(serializeStream, paged, _options);

        // Act - Deserialize
        serializeStream.Position = 0;
		string deserializedJson = await new StreamReader(serializeStream).ReadToEndAsync();
		Pipe pipe = CreatePipeWithJson(deserializedJson);

		JsonTypeInfo<TestItem> typeInfo = TestDataJsonContext.Default.TestItem;
		IAsyncPagedEnumerable<TestItem?> deserializedPaged = JsonSerializer.DeserializeAsyncPagedEnumerable<TestItem>(
            pipe.Reader,
            typeInfo,
            cancellationToken: CancellationToken.None);

		List<TestItem?> deserializedItems = await deserializedPaged.ToListAsync();
		Pagination deserializedPagination = await deserializedPaged.GetPaginationAsync();

        // Assert
        deserializedItems.Should()
            .HaveCount(originalItems.Count, "round-trip should preserve item count")
            .And.BeEquivalentTo(originalItems, "round-trip should preserve item data");

        deserializedPagination.PageSize.Should()
            .Be(originalPagination.PageSize, "round-trip should preserve pageSize");
        deserializedPagination.CurrentPage.Should()
            .Be(originalPagination.CurrentPage, "round-trip should preserve currentPage");
        deserializedPagination.TotalCount.Should()
            .Be(originalPagination.TotalCount, "round-trip should preserve totalCount");
    }

    /// <summary>
    /// Helper method to create a pipe pre-filled with JSON data.
    /// </summary>
    private static Pipe CreatePipeWithJson(string json)
    {
		byte[] bytes = Encoding.UTF8.GetBytes(json);
        var pipe = new Pipe();

		Memory<byte> buffer = pipe.Writer.GetMemory(bytes.Length);
        bytes.CopyTo(buffer);
        pipe.Writer.Advance(bytes.Length);

        // Complete the writer to signal no more data
        pipe.Writer.Complete();

        return pipe;
    }

    private static async Task<string> ReadAllFromPipeAsync(PipeReader reader)
    {
        var sb = new StringBuilder();
        try
        {
            while (true)
            {
				ReadResult result = await reader.ReadAsync();
				System.Buffers.ReadOnlySequence<byte> buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    foreach (ReadOnlyMemory<byte> segment in buffer)
                    {
                        sb.Append(Encoding.UTF8.GetString(segment.Span));
                    }
                    reader.AdvanceTo(buffer.End);
                }

                if (result.IsCompleted)
				{
					break;
				}
			}
        }
        finally
        {
            await reader.CompleteAsync();
        }

        return sb.ToString();
    }
}

/// <summary>
/// Test data models and serialization context for AOT compatibility.
/// </summary>
public class TestItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not TestItem item)
		{
			return false;
		}

		return Id == item.Id && Name == item.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}

/// <summary>
/// Source-generated JSON context for AOT and performance.
/// </summary>
[JsonSerializable(typeof(TestItem))]
[JsonSerializable(typeof(List<TestItem>))]
[JsonSerializable(typeof(IAsyncEnumerable<TestItem>))]
[JsonSerializable(typeof(Pagination))]
internal partial class TestDataJsonContext : JsonSerializerContext
{
}
