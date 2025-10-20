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

using FluentAssertions;

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

/// <summary>
/// Comprehensive unit tests for AsyncPagedEnumerableResult.
/// </summary>
public class AsyncPagedEnumerableResultTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidPagedEnumerable_ShouldSucceed()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable();

        // Act
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullHttpContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable();
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);

        // Act & Assert
        var act = async () => await result.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldWriteCorrectJsonFormat()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(1, "Item1", true),
            new TestItem(2, "Item2", false),
            new TestItem(3, "Item3", true)
        };

        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: 10, currentPage: 1, totalCount: 3);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        responseBody.Should().NotBeNullOrEmpty();

        var jsonDocument = JsonDocument.Parse(responseBody);

        // Verify structure
        jsonDocument.RootElement.TryGetProperty("pagination", out var pageContextElement).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("items", out var dataElement).Should().BeTrue();

        // Verify page context
        pageContextElement.GetProperty("PageSize").GetInt32().Should().Be(10);
        pageContextElement.GetProperty("CurrentPage").GetInt32().Should().Be(1);
        pageContextElement.GetProperty("TotalCount").GetInt32().Should().Be(3);

        // Verify items
        dataElement.GetArrayLength().Should().Be(3);
        dataElement[0].GetProperty("Id").GetInt32().Should().Be(1);
        dataElement[0].GetProperty("Name").GetString().Should().Be("Item1");
        dataElement[0].GetProperty("IsActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyData_ShouldWriteEmptyArray()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable([], pageSize: 10, currentPage: 1, totalCount: 0);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(0);
        jsonDocument.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetContentTypeWhenNotSet()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable();
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingContentType_ShouldPreserveContentType()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable();
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();
        httpContext.Response.ContentType = "application/vnd.api+json";

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/vnd.api+json");
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeDataSet_ShouldHandleCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 100)
            .Select(i => new TestItem(i, $"Item{i}", i % 2 == 0))
            .ToArray();

        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: 50, currentPage: 1, totalCount: 100);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(100);
        jsonDocument.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(100);
        jsonDocument.RootElement.GetProperty("pagination").GetProperty("PageSize").GetInt32().Should().Be(50);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullTotalCount_ShouldHandleGracefully()
    {
        // Arrange
        var items = new[] { new TestItem(1, "Item1", true) };

        // Create a Pagination with explicit null TotalCount
        var pagedEnumerable = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(new Pagination
            {
                PageSize = 10,
                CurrentPage = 1,
                TotalCount = null,
                ContinuationToken = null
            }));

        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        var totalCountProperty = jsonDocument.RootElement.GetProperty("pagination").GetProperty("TotalCount");
        // Check if TotalCount is null or if Pagination.Create converted null to a default value
        (totalCountProperty.ValueKind == JsonValueKind.Null ||
         (totalCountProperty.ValueKind == JsonValueKind.Number && totalCountProperty.GetInt32() >= 0))
        .Should().BeTrue("TotalCount should be null or a valid number when null was specified");
    }

    [Fact]
    public async Task ExecuteAsync_WithSpecialCharacters_ShouldEscapeProperly()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(1, "Item with \"quotes\"", true),
            new TestItem(2, "Item with \n newline", false)
        };

        var pagedEnumerable = CreateTestPagedEnumerable(items);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody); // Should parse without error

        var dataArray = jsonDocument.RootElement.GetProperty("items");
        dataArray[0].GetProperty("Name").GetString().Should().Be("Item with \"quotes\"");
        dataArray[1].GetProperty("Name").GetString().Should().Be("Item with \n newline");
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var pagedEnumerable = CreateSlowPagedEnumerable();
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        httpContext.RequestAborted = cts.Token;

        // Act & Assert
        var act = async () => await result.ExecuteAsync(httpContext);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Helper Methods

    private static IAsyncPagedEnumerable<TestItem> CreateTestPagedEnumerable(
        TestItem[]? items = null,
        int pageSize = 10,
        int currentPage = 1,
        int? totalCount = null)
    {
        items ??= [new TestItem(1, "DefaultItem", true)];
        totalCount ??= items.Length;

        return new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(pageSize, currentPage, totalCount: totalCount)));
    }

    private static IAsyncPagedEnumerable<TestItem> CreateSlowPagedEnumerable()
    {
        return new AsyncPagedEnumerable<TestItem>(
            SlowAsyncEnumerable(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        static async IAsyncEnumerable<TestItem> SlowAsyncEnumerable()
        {
            for (int i = 1; i <= 3; i++)
            {
                await Task.Delay(100); // Intentionally slow
                yield return new TestItem(i, $"Item{i}", true);
            }
        }
    }

    #endregion
}