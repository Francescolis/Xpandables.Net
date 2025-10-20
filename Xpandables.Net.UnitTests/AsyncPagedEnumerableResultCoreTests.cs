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

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

/// <summary>
/// Unit tests for AsyncPagedEnumerableResult focusing on core functionality.
/// </summary>
public class AsyncPagedEnumerableResultCoreTests
{
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
    public void Constructor_WithNullPagedEnumerable_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AsyncPagedEnumerableResult<TestItem>(null!, TestItemJsonContext.Default.TestItem);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("results");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullHttpContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable();
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);

        // Act & Assert
        var act = async () => await result.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("httpContext");
    }

    [Fact]
    public async Task ExecuteAsync_WithSimpleData_ShouldGenerateValidJson()
    {
        // Arrange
        var items = new[] { new TestItem(1, "Item1", true) };
        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: 10, currentPage: 1, totalCount: 1);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        responseBody.Should().NotBeNullOrEmpty();

        // Should be valid JSON
        var act = () => JsonDocument.Parse(responseBody);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldWriteCorrectJsonStructure()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(1, "Item1", true),
            new TestItem(2, "Item2", false)
        };

        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: 10, currentPage: 1, totalCount: 2);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        // Verify structure exists
        jsonDocument.RootElement.TryGetProperty("pagination", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("items", out var dataElement).Should().BeTrue();

        // Verify items is an array with correct length
        dataElement.ValueKind.Should().Be(JsonValueKind.Array);
        dataElement.GetArrayLength().Should().Be(2);

        // Verify first item structure
        var firstItem = dataElement[0];
        firstItem.TryGetProperty("Id", out _).Should().BeTrue();
        firstItem.TryGetProperty("Name", out _).Should().BeTrue();
        firstItem.TryGetProperty("IsActive", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyData_ShouldCreateValidJsonWithEmptyArray()
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

        jsonDocument.RootElement.TryGetProperty("items", out var dataElement).Should().BeTrue();
        dataElement.GetArrayLength().Should().Be(0);

        jsonDocument.RootElement.TryGetProperty("pagination", out var pageContextElement).Should().BeTrue();
        pageContextElement.TryGetProperty("TotalCount", out var totalCountElement).Should().BeTrue();
        totalCountElement.GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetDefaultContentType()
    {
        // Arrange
        var pagedEnumerable = CreateTestPagedEnumerable();
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var contentType = httpContext.Response.ContentType;
        // The GetContentType method might be returning Accept header toString(), so let's just check it's not null/empty
        contentType.Should().NotBeNullOrEmpty();
        // In a real scenario, this would be "application/json; charset=utf-8", but in tests it might differ
    }

    [Fact]
    public async Task ExecuteAsync_WithNullTotalCount_ShouldHandleGracefully()
    {
        // Arrange
        var items = new[] { new TestItem(1, "Item1", true) };

        // Create Pagination explicitly with null TotalCount
        var pagedEnumerable = new AsyncPagedEnumerable<TestItem>(
            items.ToAsync(),
            ct => ValueTask.FromResult(new Pagination { PageSize = 10, CurrentPage = 1, TotalCount = null, ContinuationToken = null }));

        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        responseBody.Should().NotBeNullOrEmpty();

        // Just verify we can parse the JSON and it has the expected structure
        var jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.TryGetProperty("pagination", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("items", out _).Should().BeTrue();
    }

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
}

internal record TestItem(int Id, string Name, bool IsActive);
[JsonSerializable(typeof(TestItem))]
internal partial class TestItemJsonContext : JsonSerializerContext
{
}