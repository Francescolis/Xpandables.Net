using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Xpandables.Net.Async;

namespace Xpandables.Net.UnitTests;

/// <summary>
/// Tests for HttpContent JSON ReadFromJsonAsAsyncPagedEnumerable extensions.
/// </summary>
public class HttpContentExtensionsJsonTests
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(HttpContentTestsJsonContext.Default)
    };

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_ArrayRoot_ShouldEnumerateItemsAndProvidePageContext()
    {
        // Arrange
        var items = new[]
        {
            new JsonTestItem(1, "First", true),
            new JsonTestItem(2, "Second", false),
            new JsonTestItem(3, "Third", true)
        };

        var json = JsonSerializer.Serialize(items, DefaultOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable(HttpContentTestsJsonContext.Default.JsonTestItem);
        var ctx = await paged.GetPaginationAsync();

        // Assert page context
        ctx.TotalCount.Should().Be(items.Length);
        ctx.PageSize.Should().Be(0);
        ctx.CurrentPage.Should().Be(0);

        // Assert enumeration
        var list = new List<JsonTestItem>();
        await foreach (var it in paged)
            list.Add(it);

        list.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_ObjectWithDataArray_ShouldEnumerateItemsAndProvidePageContext()
    {
        // Arrange
        var payload = new
        {
            pagination = Pagination.Create(5, 2, null, 42),
            items = new[]
            {
                new JsonTestItem(10, "A", true),
                new JsonTestItem(11, "B", false)
            }
        };

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.TypeInfoResolverChain.Add(HttpContentTestsJsonContext.Default);

        var json = JsonSerializer.Serialize(payload);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(options);
        var ctx = await paged.GetPaginationAsync();

        // Assert pagination comes from payload.pagination
        ctx.PageSize.Should().Be(5);
        ctx.CurrentPage.Should().Be(2);
        ctx.TotalCount.Should().Be(42);
        ctx.ContinuationToken.Should().BeNull();

        // Assert items enumeration
        var list = await ToListAsync(paged);
        list.Should().HaveCount(2);
        list[0].Id.Should().Be(10);
        list[1].Id.Should().Be(11);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_ObjectWithoutPageContext_WithDataArray_ShouldUseDataLengthForTotal()
    {
        // Arrange
        var payload = new
        {
            items = new[]
            {
                new JsonTestItem(1, "X", true),
                new JsonTestItem(2, "Y", false),
                new JsonTestItem(3, "Z", true)
            }
        };

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.TypeInfoResolverChain.Add(HttpContentTestsJsonContext.Default);

        var json = JsonSerializer.Serialize(payload);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable(HttpContentTestsJsonContext.Default.JsonTestItem);
        var ctx = await paged.GetPaginationAsync();

        // Assert page context inferred from items length
        ctx.TotalCount.Should().Be(3);
        ctx.PageSize.Should().Be(0);
        ctx.CurrentPage.Should().Be(0);

        // Assert enumeration
        var list = await ToListAsync(paged);
        list.Should().HaveCount(3);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_EmptyContent_ShouldReturnEmptySequenceAndZeroTotal()
    {
        // Arrange
        using var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable(HttpContentTestsJsonContext.Default.JsonTestItem);
        var ctx = await paged.GetPaginationAsync();

        // Assert
        ctx.TotalCount.Should().Be(0);
        var list = await ToListAsync(paged);
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_CaseInsensitiveProperties_ShouldWork()
    {
        // Arrange (note different casing)
        var json = """
        {
            "pagination": { "PageSize": 7, "CurrentPage": 3, "TotalCount": 9, "ContinuationToken": null },
            "items": [ {"Id": 1, "Name": "n1", "IsActive": true} ]
        }
        """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(HttpContentTestsJsonContext.Default, PaginationSourceGenerationContext.Default)
        };

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(options);
        var ctx = await paged.GetPaginationAsync();

        // Assert
        ctx.PageSize.Should().Be(7);
        ctx.CurrentPage.Should().Be(3);
        ctx.TotalCount.Should().Be(9);

        var list = await ToListAsync(paged);
        list.Should().HaveCount(1);
        list[0].Name.Should().Be("n1");
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_ObjectWithoutDataArray_ShouldReturnEmptySequence()
    {
        // Arrange
        var json = """
        {
            "pagination": { "pageSize": 5, "currentPage": 1, "totalCount": 0 }
        }
        """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable(HttpContentTestsJsonContext.Default.JsonTestItem);
        _ = await paged.GetPaginationAsync();

        // Assert
        var list = await ToListAsync(paged);
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerableStreaming_PaginationFirst_ShouldWork()
    {
        // Arrange
        var json = """
        {
            "pagination": { "pageSize": 10, "currentPage": 2, "totalCount": 50, "continuationToken": null },
            "items": [ 
                {"Id": 1, "Name": "Item1", "IsActive": true},
                {"Id": 2, "Name": "Item2", "IsActive": false}
            ]
        }
        """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(HttpContentTestsJsonContext.Default, PaginationSourceGenerationContext.Default)
        };

        // Act - Get pagination FIRST
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(options);
        var ctx = await paged.GetPaginationAsync();

        // Assert pagination
        ctx.PageSize.Should().Be(10);
        ctx.CurrentPage.Should().Be(2);
        ctx.TotalCount.Should().Be(50);

        // Then enumerate
        var list = await ToListAsync(paged);
        list.Should().HaveCount(2);
        list[0].Name.Should().Be("Item1");
        list[1].Name.Should().Be("Item2");
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerableStreaming_EnumerationFirst_ShouldWork()
    {
        // Arrange
        var json = """
        {
            "pagination": { "pageSize": 10, "currentPage": 1, "totalCount": 25, "continuationToken": null },
            "items": [ 
                {"Id": 100, "Name": "First", "IsActive": true},
                {"Id": 200, "Name": "Second", "IsActive": false},
                {"Id": 300, "Name": "Third", "IsActive": true}
            ]
        }
        """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(HttpContentTestsJsonContext.Default, PaginationSourceGenerationContext.Default)
        };

        // Act - Enumerate FIRST
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(options);
        var list = await ToListAsync(paged);

        // Assert items
        list.Should().HaveCount(3);
        list[0].Id.Should().Be(100);
        list[1].Id.Should().Be(200);
        list[2].Id.Should().Be(300);

        // Then get pagination (should still work)
        var ctx = await paged.GetPaginationAsync();
        ctx.PageSize.Should().Be(10);
        ctx.CurrentPage.Should().Be(1);
        ctx.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerableStreaming_ArrayRoot_ShouldWork()
    {
        // Arrange
        var items = new[]
        {
            new JsonTestItem(1, "A", true),
            new JsonTestItem(2, "B", false),
            new JsonTestItem(3, "C", true)
        };

        var json = JsonSerializer.Serialize(items, DefaultOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Use streaming with array root (no property name)
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(
            HttpContentTestsJsonContext.Default.JsonTestItem);

        var list = await ToListAsync(paged);

        // Assert
        list.Should().HaveCount(3);
        list.Should().BeEquivalentTo(items);

        var ctx = await paged.GetPaginationAsync();
        ctx.Should().NotBeNull();
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerableStreaming_EmptyContent_ShouldReturnEmpty()
    {
        // Arrange
        using var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(HttpContentTestsJsonContext.Default, PaginationSourceGenerationContext.Default)
        };

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(options);
        var list = await ToListAsync(paged);

        // Assert
        list.Should().BeEmpty();

        var ctx = await paged.GetPaginationAsync();
        ctx.TotalCount.Should().Be(0);
    }

    private static async Task<List<JsonTestItem>> ToListAsync(IAsyncEnumerable<JsonTestItem> source)
    {
        var list = new List<JsonTestItem>();
        await foreach (var item in source)
            list.Add(item);
        return list;
    }
}


internal record JsonTestItem(int Id, string Name, bool IsActive);

[JsonSerializable(typeof(JsonTestItem[]))]
[JsonSerializable(typeof(JsonTestItem))]
internal partial class HttpContentTestsJsonContext : JsonSerializerContext { }
