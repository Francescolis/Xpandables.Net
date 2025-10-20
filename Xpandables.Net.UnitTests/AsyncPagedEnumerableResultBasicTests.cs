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
using FluentAssertions;

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

/// <summary>
/// Basic functionality tests for AsyncPagedEnumerableResult to verify core operations work.
/// </summary>
public class AsyncPagedEnumerableResultBasicTests
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
        var act = () => new AsyncPagedEnumerableResult<TestItem>(null!);
        act.Should().Throw<ArgumentNullException>();
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

    [Fact]
    public async Task ExecuteAsync_WithSimpleData_ShouldSetContentType()
    {
        // Arrange
        var items = new[] { new TestItem(1, "Item1", true) };
        var pagedEnumerable = CreateTestPagedEnumerable(items);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task ExecuteAsync_WithSimpleData_ShouldWriteValidJson()
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
        var act = () => System.Text.Json.JsonDocument.Parse(responseBody);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyData_ShouldCreateValidJsonWithEmptyArray()
    {
        // Arrange
        var items = Array.Empty<TestItem>();
        var pagedEnumerable = CreateTestPagedEnumerable(items, pageSize: 10, currentPage: 1, totalCount: 0);
        var result = pagedEnumerable.ToResult(TestItemJsonContext.Default.TestItem);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        responseBody.Should().NotBeNullOrEmpty();

        // Should be valid JSON with empty items array
        var jsonDoc = System.Text.Json.JsonDocument.Parse(responseBody);
        jsonDoc.RootElement.TryGetProperty("pagination", out _).Should().BeTrue();
        jsonDoc.RootElement.TryGetProperty("items", out var dataElement).Should().BeTrue();
        dataElement.GetArrayLength().Should().Be(0);
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