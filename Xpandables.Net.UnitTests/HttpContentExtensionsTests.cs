/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Async;

using Xunit;

namespace Xpandables.Net.UnitTests;

public record HttpTestItem(int Id, string Name);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(HttpTestItem))]
internal partial class HttpTestJsonContext : JsonSerializerContext
{
}

public sealed class HttpContentExtensionsTests
{
    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_WithPaginationAndItems_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "pagination": {
                "pageSize": 10,
                "currentPage": 1,
                "totalCount": 3,
                "continuationToken": null
            },
            "items": [
                { "id": 1, "name": "Item1" },
                { "id": 2, "name": "Item2" },
                { "id": 3, "name": "Item3" }
            ]
        }
        """;
        
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var pagedEnumerable = content.ReadFromJsonAsAsyncPagedEnumerable(
            HttpTestJsonContext.Default.HttpTestItem);

        // Assert - Pagination is immediately available
        var pagination = pagedEnumerable.Pagination;
        Assert.Equal(10, pagination.PageSize);
        Assert.Equal(1, pagination.CurrentPage);
        Assert.Equal(3, pagination.TotalCount);

        // Assert - GetPaginationAsync returns the same value
        var paginationAsync = await pagedEnumerable.GetPaginationAsync();
        Assert.Equal(pagination, paginationAsync);

        // Assert - Items can be enumerated
        var items = await pagedEnumerable.ToListAsync();
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0].Id);
        Assert.Equal("Item1", items[0].Name);
        Assert.Equal(2, items[1].Id);
        Assert.Equal("Item2", items[1].Name);
        Assert.Equal(3, items[2].Id);
        Assert.Equal("Item3", items[2].Name);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_WithTopLevelArray_ShouldDeserializeWithEmptyPagination()
    {
        // Arrange
        var json = """
        [
            { "id": 1, "name": "Item1" },
            { "id": 2, "name": "Item2" }
        ]
        """;
        
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var pagedEnumerable = content.ReadFromJsonAsAsyncPagedEnumerable(
            HttpTestJsonContext.Default.HttpTestItem);

        // Assert - Pagination should be empty
        var pagination = await pagedEnumerable.GetPaginationAsync();
        Assert.Equal(Pagination.Empty, pagination);

        // Assert - Items can be enumerated
        var items = await pagedEnumerable.ToListAsync();
        Assert.Equal(2, items.Count);
        Assert.Equal(1, items[0].Id);
        Assert.Equal(2, items[1].Id);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_WithObjectButNoItems_ShouldReturnSingleValue()
    {
        // Arrange
        var json = """{ "id": 42, "name": "SingleItem" }""";
        
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var pagedEnumerable = content.ReadFromJsonAsAsyncPagedEnumerable(
            HttpTestJsonContext.Default.HttpTestItem);

        // Assert
        var items = await pagedEnumerable.ToListAsync();
        Assert.Single(items);
        Assert.Equal(42, items[0].Id);
        Assert.Equal("SingleItem", items[0].Name);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_WithJsonSerializerOptions_ShouldWork()
    {
        // Arrange
        var json = """
        {
            "pagination": { "pageSize": 5, "currentPage": 2, "totalCount": 10, "continuationToken": null },
            "items": [
                { "id": 6, "name": "Item6" },
                { "id": 7, "name": "Item7" }
            ]
        }
        """;
        
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = HttpTestJsonContext.Default
        };

        // Act
        var pagedEnumerable = content.ReadFromJsonAsAsyncPagedEnumerable<HttpTestItem>(options);

        // Assert
        var pagination = await pagedEnumerable.GetPaginationAsync();
        Assert.Equal(5, pagination.PageSize);
        Assert.Equal(2, pagination.CurrentPage);
        Assert.Equal(10, pagination.TotalCount);

        var items = await pagedEnumerable.ToListAsync();
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task ReadFromJsonAsAsyncPagedEnumerable_MultipleEnumerations_ShouldWorkCorrectly()
    {
        // Arrange
        var json = """
        {
            "pagination": { "pageSize": 2, "currentPage": 1, "totalCount": 2, "continuationToken": null },
            "items": [
                { "id": 1, "name": "Item1" },
                { "id": 2, "name": "Item2" }
            ]
        }
        """;
        
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var pagedEnumerable = content.ReadFromJsonAsAsyncPagedEnumerable(
            HttpTestJsonContext.Default.HttpTestItem);

        // First enumeration
        var items1 = await pagedEnumerable.ToListAsync();
        
        // Second enumeration (should work on the cached JsonDocument)
        var items2 = await pagedEnumerable.ToListAsync();

        // Assert
        Assert.Equal(2, items1.Count);
        Assert.Equal(2, items2.Count);
        Assert.Equal(items1[0].Id, items2[0].Id);
        Assert.Equal(items1[1].Id, items2[1].Id);
    }
}
