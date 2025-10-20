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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests;

/// <summary>
/// Additional integration and edge case tests for AsyncPagedEnumerableResult.
/// </summary>
public class AsyncPagedEnumerableResultIntegrationTests
{
    [Fact]
    public async Task ExecuteAsync_WithComplexObject_ShouldSerializeCorrectly()
    {
        // Arrange
        var products = new[]
        {
            new Product(1, "Laptop", 999.99m, "Electronics"),
            new Product(2, "Book", 29.95m, "Education"),
            new Product(3, "Coffee", 12.50m, "Food & Beverage")
        };

        var pagedEnumerable = new AsyncPagedEnumerable<Product>(
            products.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        var result = pagedEnumerable.ToResult(ProductContext.Default.Product);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        var dataArray = jsonDocument.RootElement.GetProperty("items");
        dataArray.GetArrayLength().Should().Be(3);

        dataArray[0].GetProperty("Id").GetInt32().Should().Be(1);
        dataArray[0].GetProperty("Name").GetString().Should().Be("Laptop");
        dataArray[0].GetProperty("Price").GetDecimal().Should().Be(999.99m);
        dataArray[0].GetProperty("Category").GetString().Should().Be("Electronics");
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeDataSet_ShouldHandleEfficiently()
    {
        // Arrange
        const int itemCount = 1000;
        var users = Enumerable.Range(1, itemCount)
            .Select(i => new User(i, $"User{i}", $"user{i}@example.com", i % 2 == 0))
            .ToArray();

        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(100, 1, totalCount: itemCount)));

        var result = pagedEnumerable.ToResult(UserContext.Default.User);
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

        // Should complete in reasonable time (less than 2 seconds for 1k items)
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomJsonOptions_ShouldUseProvidedOptions()
    {
        // Arrange
        var users = new[] { new User(1, "TestUser", "test@example.com", true) };
        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var result = pagedEnumerable.ToResult();

        var httpContext = HttpContextTestHelpers.CreateTestHttpContext(services => services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.WriteIndented = true;
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.TypeInfoResolverChain.Add(UserContext.Default);
            }));

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);

        // Should be indented
        responseBody.Should().Contain("  ");

        // Should still parse correctly
        var jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnicodeCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new User(1, "??", "user@??.com", true),
            new User(2, "?? User", "emoji@test.com", false),
            new User(3, "Müller", "müller@test.de", true)
        };

        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        var result = pagedEnumerable.ToResult(UserContext.Default.User);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var jsonDocument = JsonDocument.Parse(responseBody);

        var dataArray = jsonDocument.RootElement.GetProperty("items");
        dataArray[0].GetProperty("Name").GetString().Should().Be("??");
        dataArray[0].GetProperty("Email").GetString().Should().Be("user@??.com");
        dataArray[1].GetProperty("Name").GetString().Should().Be("?? User");
        dataArray[2].GetProperty("Name").GetString().Should().Be("Müller");
    }

    [Fact]
    public async Task ExecuteAsync_WithSpecialCharactersInJson_ShouldEscapeProperly()
    {
        // Arrange
        var users = new[]
        {
            new User(1, "User with \"quotes\"", "user@test.com", true),
            new User(2, "User with \n newline", "newline@test.com", false),
            new User(3, "User with \\ backslash", "backslash@test.com", true)
        };

        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        var result = pagedEnumerable.ToResult(UserContext.Default.User);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);

        // Assert response body can be parsed as JSON
        _ = JsonDocument.Parse(responseBody);

        var dataArray = JsonDocument.Parse(responseBody).RootElement.GetProperty("items");
        dataArray[0].GetProperty("Name").GetString().Should().Be("User with \"quotes\"");
        dataArray[1].GetProperty("Name").GetString().Should().Be("User with \n newline");
        dataArray[2].GetProperty("Name").GetString().Should().Be("User with \\ backslash");
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var slowEnumerable = CreateSlowPagedEnumerable();
        var result = slowEnumerable.ToResult(UserContext.Default.User);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        httpContext.RequestAborted = cts.Token;

        // Act & Assert
        var act = async () => await result.ExecuteAsync(httpContext);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Formatter_WriteResponseBodyAsync_Utf8Encoding_SerializesCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new User(1, "Alpha", "alpha@test.com", true),
            new User(2, "Beta", "beta@test.com", false)
        };

        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(5, 1, totalCount: users.Length)));

        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        var options = new JsonSerializerOptions();
        options.TypeInfoResolverChain.Add(UserContext.Default);
        var formatter = new AsyncPagedEnumerableJsonOutputFormatter(options);

        var context = new OutputFormatterWriteContext(
            httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(IAsyncPagedEnumerable<User>),
            pagedEnumerable);

        // Act
        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
        json.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("Alpha");
    }

    [Fact]
    public async Task Formatter_WriteResponseBodyAsync_UnicodeEncoding_SerializesCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new User(1, "?????", "gamma@test.com", true),
            new User(2, "???ta", "delta@test.com", false)
        };

        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(5, 1, totalCount: users.Length)));

        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        var options = new JsonSerializerOptions();
        options.TypeInfoResolverChain.Add(UserContext.Default);
        var formatter = new AsyncPagedEnumerableJsonOutputFormatter(options);

        var context = new OutputFormatterWriteContext(
            httpContext,
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(IAsyncPagedEnumerable<User>),
            pagedEnumerable);

        // Act
        await formatter.WriteResponseBodyAsync(context, Encoding.Unicode);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext, Encoding.Unicode);
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("?????");
        json.RootElement.GetProperty("items")[1].GetProperty("Name").GetString().Should().Be("???ta");
    }

    [Fact]
    public async Task ExecuteAsync_WithSerializerOptionsOverload_ShouldUseProvidedOptions()
    {
        // Arrange
        var users = new[] { new User(1, "CamelCase", "camel@test.com", true) };
        var pagedEnumerable = new AsyncPagedEnumerable<User>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        // Use runtime serialization, or add resolver if needed
        serializerOptions.TypeInfoResolverChain.Add(UserContext.Default);

        var result = pagedEnumerable.ToResult(serializerOptions);
        var httpContext = HttpContextTestHelpers.CreateTestHttpContext();

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var responseBody = HttpContextTestHelpers.GetResponseBodyAsString(httpContext);
        responseBody.Should().Contain("  "); // indented
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
        json.RootElement.GetProperty("items")[0].GetProperty("name").GetString().Should().Be("CamelCase");
        json.RootElement.GetProperty("items")[0].TryGetProperty("Name", out _).Should().BeFalse();
    }

    private static IAsyncPagedEnumerable<User> CreateSlowPagedEnumerable()
    {
        return new AsyncPagedEnumerable<User>(
            SlowAsyncEnumerable(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        static async IAsyncEnumerable<User> SlowAsyncEnumerable()
        {
            for (int i = 1; i <= 3; i++)
            {
                await Task.Delay(100); // Intentionally slow for cancellation testing
                yield return new User(i, $"SlowUser{i}", $"slow{i}@test.com", true);
            }
        }
    }
}

internal record Product(int Id, string Name, decimal Price, string Category);
internal record User(int Id, string Name, string Email, bool IsActive);
[JsonSerializable(typeof(Product))]
internal partial class ProductContext : JsonSerializerContext { }
[JsonSerializable(typeof(User))]
internal partial class UserContext : JsonSerializerContext { }
