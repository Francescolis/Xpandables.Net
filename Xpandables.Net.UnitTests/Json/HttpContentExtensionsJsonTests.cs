/*******************************************************************************
 * Copyright (C) 2025 Francis-Black EWANE
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
using System.Net.Abstractions;
using System.Net.Async;
using System.Net.Async.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

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
        var ctx = await paged.GetPageContextAsync();

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
            pageContext = PageContext.Create(5, 2, null, 42),
            data = new[]
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
        var ctx = await paged.GetPageContextAsync();

        // Assert page context comes from payload.pageContext
        ctx.PageSize.Should().Be(5);
        ctx.CurrentPage.Should().Be(2);
        ctx.TotalCount.Should().Be(42);
        ctx.ContinuationToken.Should().BeNull();

        // Assert data enumeration
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
            data = new[]
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
        var ctx = await paged.GetPageContextAsync();

        // Assert page context inferred from data length
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
        var ctx = await paged.GetPageContextAsync();

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
            "PageContext": { "PageSize": 7, "CurrentPage": 3, "TotalCount": 9, "ContinuationToken": null },
            "Data": [ {"Id": 1, "Name": "n1", "IsActive": true} ]
        }
        """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(HttpContentTestsJsonContext.Default)
        };

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable<JsonTestItem>(options);
        var ctx = await paged.GetPageContextAsync();

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
            "pageContext": { "pageSize": 5, "currentPage": 1, "totalCount": 0 }
        }
        """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var paged = content.ReadFromJsonAsAsyncPagedEnumerable(HttpContentTestsJsonContext.Default.JsonTestItem);
        _ = await paged.GetPageContextAsync();

        // Assert
        var list = await ToListAsync(paged);
        list.Should().BeEmpty();
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
