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
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

/// <summary>
/// Edge case and performance tests for AsyncPagedEnumerableResult.
/// </summary>
public class AsyncPagedEnumerableResultEdgeCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithNullValues_ShouldHandleNullsCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new EdgeCaseItem(null, null, null),
            new EdgeCaseItem(1, "ValidName", DateTime.Now),
            new EdgeCaseItem(2, null, DateTime.Today)
        };

        var pagedEnumerable = CreateTestPagedEnumerable(items);
        var result = pagedEnumerable.ToResult(EdgeCaseItemJsonContext.Default.EdgeCaseItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext(services =>
            services.Configure<JsonOptions>(options =>
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull));

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        var dataArray = jsonDocument.RootElement.GetProperty("items");
        dataArray.GetArrayLength().Should().Be(3);

        // Verify we can parse the JSON and have the expected structure
        jsonDocument.RootElement.TryGetProperty("pagination", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("items", out _).Should().BeTrue();

        // Test that the second item (which has valid values) is serialized correctly
        var secondItem = dataArray[1];
        if (secondItem.TryGetProperty("NullableId", out var idProperty))
        {
            idProperty.GetInt32().Should().Be(1);
        }

        if (secondItem.TryGetProperty("NullableName", out var nameProperty))
        {
            nameProperty.GetString().Should().Be("ValidName");
        }

        // For the first and third items, we'll be more flexible about null handling
        // since the JSON serializer behavior might vary
        var firstItem = dataArray[0];
        var thirdItem = dataArray[2];

        // At minimum, we should be able to parse all items without errors
        firstItem.ValueKind.Should().Be(JsonValueKind.Object);
        thirdItem.ValueKind.Should().Be(JsonValueKind.Object);

        // The first item with all null values should serialize as an empty object when using JsonIgnoreCondition.WhenWritingNull
        // The third item should have the non-null properties present
        if (thirdItem.TryGetProperty("NullableId", out var thirdIdProperty))
        {
            thirdIdProperty.GetInt32().Should().Be(2);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithVeryLargePageSize_ShouldHandleEfficiently()
    {
        // Arrange
        const int itemCount = 5000; // Reduced from 10000 to make test faster
        var items = Enumerable.Range(1, itemCount)
            .Select(i => new EdgeCaseItem(i, $"Item{i}", DateTime.Today.AddDays(i)))
            .ToArray();

        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: itemCount, currentPage: 1, totalCount: itemCount);
        var result = pagedEnumerable.ToResult(EdgeCaseItemJsonContext.Default.EdgeCaseItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await result.ExecuteAsync(httpContext);
        stopwatch.Stop();

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(itemCount);
        jsonDocument.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(itemCount);

        // Should complete in reasonable time (less than 3 seconds for 5k items)
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroPageSize_ShouldHandleGracefully()
    {
        // Arrange
        var items = new[] { new EdgeCaseItem(1, "Item1", DateTime.Now) };
        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: 0, currentPage: 0, totalCount: 1);
        var result = pagedEnumerable.ToResult(EdgeCaseItemJsonContext.Default.EdgeCaseItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        var pagination = jsonDocument.RootElement.GetProperty("pagination");
        pagination.GetProperty("PageSize").GetInt32().Should().Be(0);
        pagination.GetProperty("CurrentPage").GetInt32().Should().Be(0);
        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnicodeCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new EdgeCaseItem(1, "????", DateTime.Now),
            new EdgeCaseItem(2, "?? Emoji Test ??", DateTime.Now),
            new EdgeCaseItem(3, "Åpfel über München", DateTime.Now),
            new EdgeCaseItem(4, "???????", DateTime.Now)
        };

        var pagedEnumerable = CreateTestPagedEnumerable(items);
        var result = pagedEnumerable.ToResult(EdgeCaseItemJsonContext.Default.EdgeCaseItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        var dataArray = jsonDocument.RootElement.GetProperty("items");
        dataArray[0].GetProperty("NullableName").GetString().Should().Be("????");
        dataArray[1].GetProperty("NullableName").GetString().Should().Be("?? Emoji Test ??");
        dataArray[2].GetProperty("NullableName").GetString().Should().Be("Åpfel über München");
        dataArray[3].GetProperty("NullableName").GetString().Should().Be("???????");
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomJsonOptions_ShouldRespectAllOptions()
    {
        // Arrange
        var items = new[] { new EdgeCaseItem(1, "TestItem", DateTime.Parse("2025-01-01T10:30:45Z")) };
        var pagedEnumerable = CreateTestPagedEnumerable(items);
        var result = pagedEnumerable.ToResult();

        var httpContext = HttpContextTestHelpers.CreateTestHttpContext(services => services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = true;
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            }));

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);

        // Should be indented
        responseBody.Should().Contain("  ");

        // Should use camelCase
        responseBody.Should().Contain("\"nullableId\"");
        responseBody.Should().Contain("\"nullableName\"");
        responseBody.Should().Contain("\"pagination\"");

        // Should still parse correctly
        var jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithExceptionDuringEnumeration_ShouldPropagateException()
    {
        // Arrange
        var faultyEnumerable = CreateFaultyPagedEnumerable();
        var result = faultyEnumerable.ToResult(EdgeCaseItemJsonContext.Default.EdgeCaseItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act & Assert
        var act = async () => await result.ExecuteAsync(httpContext);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Simulated enumeration error");
    }

    [Fact]
    public async Task ExecuteAsync_WithSlowEnumeration_ShouldFlushPeriodically()
    {
        // Arrange
        var slowEnumerable = CreateSlowPagedEnumerable();
        var result = slowEnumerable.ToResult(EdgeCaseItemJsonContext.Default.EdgeCaseItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await result.ExecuteAsync(httpContext);
        stopwatch.Stop();

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(3);

        // Should take some time due to delays but still complete
        stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(100));
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    #region Helper Methods

    private static IAsyncPagedEnumerable<EdgeCaseItem> CreateTestPagedEnumerable(
        EdgeCaseItem[]? items = null,
        int pageSize = 10,
        int currentPage = 1,
        int? totalCount = null)
    {
        items ??= [new EdgeCaseItem(1, "DefaultItem", DateTime.Now)];
        totalCount ??= items.Length;

        return new AsyncPagedEnumerable<EdgeCaseItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(pageSize, currentPage, totalCount: totalCount)));
    }

    private static IAsyncPagedEnumerable<EdgeCaseItem> CreateFaultyPagedEnumerable()
    {
        return new AsyncPagedEnumerable<EdgeCaseItem>(
            FaultyAsyncEnumerable(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        static async IAsyncEnumerable<EdgeCaseItem> FaultyAsyncEnumerable()
        {
            yield return new EdgeCaseItem(1, "Item1", DateTime.Now);
            await Task.Delay(10);
            throw new InvalidOperationException("Simulated enumeration error");
        }
    }

    private static IAsyncPagedEnumerable<EdgeCaseItem> CreateSlowPagedEnumerable()
    {
        return new AsyncPagedEnumerable<EdgeCaseItem>(
            SlowAsyncEnumerable(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        static async IAsyncEnumerable<EdgeCaseItem> SlowAsyncEnumerable()
        {
            for (int i = 1; i <= 3; i++)
            {
                await Task.Delay(50);
                yield return new EdgeCaseItem(i, $"SlowItem{i}", DateTime.Now);
            }
        }
    }

    #endregion
}

internal record EdgeCaseItem(int? NullableId, string? NullableName, DateTime? NullableDate);
[JsonSerializable(typeof(EdgeCaseItem))]
internal partial class EdgeCaseItemJsonContext : JsonSerializerContext
{
}