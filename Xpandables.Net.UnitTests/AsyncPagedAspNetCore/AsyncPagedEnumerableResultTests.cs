using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Minimals;

namespace Xpandables.Net.UnitTests.AsyncPagedAspNetCore;

public class AsyncPagedEnumerableResultTests
{
    private sealed class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private static IAsyncPagedEnumerable<Item> CreatePaged(params Item[] items) =>
        new AsyncPagedEnumerable<Item>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    [Fact]
    public async Task ExecuteAsync_WritesExpectedJsonEnvelope_WithRuntimeOptions()
    {
        var items = Enumerable.Range(1, 5).Select(i => new Item { Id = i, Name = $"N{i}" }).ToArray();
        var paged = CreatePaged(items);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var result = new AsyncPagedEnumerableResult<Item>(paged, options);

        var services = new ServiceCollection()
            .AddOptions()
            .Configure<JsonOptions>(_ => { })
            .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        var ms = new MemoryStream();
        context.Response.Body = ms;

        await result.ExecuteAsync(context);
        await context.Response.BodyWriter.FlushAsync();

        ms.Position = 0;
        using var doc = await JsonDocument.ParseAsync(ms);
        var root = doc.RootElement;
        root.TryGetProperty("pagination", out var pagination).Should().BeTrue();
        pagination.GetProperty("PageSize").GetInt32().Should().Be(items.Length);
        pagination.GetProperty("CurrentPage").GetInt32().Should().Be(1);
        pagination.GetProperty("TotalCount").GetInt32().Should().Be(items.Length);

        var itemsEl = root.GetProperty("items");
        itemsEl.GetArrayLength().Should().Be(items.Length);
        itemsEl[0].GetProperty("id").GetInt32().Should().Be(1);
        itemsEl[items.Length - 1].GetProperty("name").GetString().Should().Be("N5");
    }
}
