using System.Buffers;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Collections.Extensions;

namespace Xpandables.Net.UnitTests.AsyncPaged;

public class JsonSerializerExtensionsTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    [Fact]
    public async Task SerializeAsyncPaged_Stream_WritesExpectedEnvelope()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => new Item { Id = i, Name = $"N{i}" }).ToList();
        var pagination = Pagination.Create(pageSize: 5, currentPage: 2, totalCount: items.Count);
        IAsyncPagedEnumerable<Item> paged = new AsyncPagedEnumerable<Item>(GetAsync(items), _ => new(pagination));

        using var ms = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(ms, paged, _options);
        ms.Position = 0;
        using var doc = await JsonDocument.ParseAsync(ms);

        // Assert
        var root = doc.RootElement;
        root.TryGetProperty("pagination", out var p).Should().BeTrue();
        // Pagination is serialized via source-generated context (PascalCase)
        p.GetProperty("PageSize").GetInt32().Should().Be(5);
        p.GetProperty("CurrentPage").GetInt32().Should().Be(2);
        p.GetProperty("TotalCount").GetInt32().Should().Be(items.Count);

        var itemsEl = root.GetProperty("items");
        itemsEl.GetArrayLength().Should().Be(items.Count);
        // Items are serialized with the provided options (camelCase)
        itemsEl[0].GetProperty("id").GetInt32().Should().Be(1);
        itemsEl[itemsEl.GetArrayLength() - 1].GetProperty("name").GetString().Should().Be("N10");
    }

    [Fact]
    public async Task SerializeAsyncPaged_PipeWriter_WritesUtf8Json()
    {
        // Arrange
        var items = Enumerable.Range(1, 3).Select(i => new Item { Id = i }).ToList();
        var pagination = Pagination.Create(pageSize: 3, currentPage: 1, totalCount: 3);
        IAsyncPagedEnumerable<Item> paged = new AsyncPagedEnumerable<Item>(GetAsync(items), _ => new(pagination));

        var pipe = new System.IO.Pipelines.Pipe();

        // Act
        await JsonSerializer.SerializeAsyncPaged(pipe.Writer, paged, _options);
        await pipe.Writer.CompleteAsync();
        var read = await pipe.Reader.ReadAsync();

        // Assert
        read.Buffer.Length.Should().BeGreaterThan(0);
        var text = System.Text.Encoding.UTF8.GetString(read.Buffer.ToArray());
        text.Should().Contain("\"pagination\"").And.Contain("\"items\"");
        pipe.Reader.AdvanceTo(read.Buffer.End);
        await pipe.Reader.CompleteAsync();
    }

    private static async IAsyncEnumerable<Item> GetAsync(IEnumerable<Item> src)
    {
        foreach (var i in src)
        {
            yield return i;
            await Task.Yield();
        }
    }

    private sealed class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
