/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

using FluentAssertions;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.UnitTests.AsyncPaged.Integration;

/// <summary>
/// Integration tests for JSON serialization of async paged enumerables.
/// </summary>
public sealed class JsonSerializationIntegrationTests
{
    [Fact]
    public async Task SerializeAsyncPaged_ToStream_WithGenericTypeInfo_ShouldSerializeCorrectly()
    {
        // Arrange
        var items = new[] { new Product(1, "Apple"), new Product(2, "Banana"), new Product(3, "Cherry") };
        IAsyncPagedEnumerable<Product> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1, null, 100));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };

        // Act - Use the options-based overload instead of JsonTypeInfo
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<Product>>(json, options);

        result.Should().NotBeNull();
        result!.Pagination.Should().NotBeNull();
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.CurrentPage.Should().Be(1);
        result.Pagination.TotalCount.Should().Be(100);
        result.Items.Should().HaveCount(3);
        result.Items.Should().ContainEquivalentOf(new Product(1, "Apple"));
    }

    [Fact]
    public async Task SerializeAsyncPaged_ToStream_WithOptions_ShouldSerializeCorrectly()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 };
        IAsyncPagedEnumerable<int> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(5, 2, "token123", 25));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<int>>(json, options);

        result.Should().NotBeNull();
        result!.Pagination.PageSize.Should().Be(5);
        result.Pagination.CurrentPage.Should().Be(2);
        result.Pagination.ContinuationToken.Should().Be("token123");
        result.Pagination.TotalCount.Should().Be(25);
        result.Items.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithEmptyEnumerable_ShouldSerializeCorrectly()
    {
        // Arrange
        IAsyncPagedEnumerable<string> pagedEnumerable = CreatePagedEnumerable(
          Array.Empty<string>(),
          Pagination.FromTotalCount(0));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<string>>(json);

        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithLargeDataSet_ShouldStreamEfficiently()
    {
        // Arrange
        var items = Enumerable.Range(1, 1000).ToArray();
        IAsyncPagedEnumerable<int> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(100, 5, null, 10000));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("\"pagination\"");
        json.Should().Contain("\"items\"");
        json.Should().Contain("\"PageSize\":100");  // PascalCase is default
        json.Should().Contain("\"CurrentPage\":5");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var slowEnumerable = CreateSlowPagedEnumerable(100, TimeSpan.FromMilliseconds(50));
        using var stream = new MemoryStream();

        // Act
        cts.CancelAfter(100);
        Func<Task> act = async () => await JsonSerializer.SerializeAsyncPaged(
          stream,
          slowEnumerable,
          cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SerializeAsyncPaged_NonGeneric_WithTypeInfo_ShouldSerializeCorrectly()
    {
        // Arrange
        var items = new[] { new Person("Alice", 30), new Person("Bob", 25) };
        IAsyncPagedEnumerable<Person> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1, null, 50));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };

        // Act - Use options-based overload
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Alice");
        json.Should().Contain("Bob");
        json.Should().Contain("\"pagination\"");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithSpecialCharacters_ShouldEscapeCorrectly()
    {
        // Arrange
        var items = new[] { "Hello \"World\"", "Line1\nLine2", "Tab\there" };
        IAsyncPagedEnumerable<string> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1, null, 3));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<string>>(json, options);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.Items.Should().Contain("Hello \"World\"");
        result.Items.Should().Contain("Line1\nLine2");
        result.Items.Should().Contain("Tab\there");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithComplexNestedObjects_ShouldSerializeCorrectly()
    {
        // Arrange
        var items = new[]
        {
            new Order(1, new Customer("Alice", "alice@test.com"), new[] { "Item1", "Item2" }),
            new Order(2, new Customer("Bob", "bob@test.com"), new[] { "Item3" })
 };
        IAsyncPagedEnumerable<Order> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1, null, 20));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<Order>>(json, options);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items[0].Customer.Name.Should().Be("Alice");
        result.Items[0].ItemNames.Should().Equal("Item1", "Item2");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithNullValues_ShouldHandleCorrectly()
    {
        // Arrange
        var items = new[] { "Value1", null, "Value3" };
        IAsyncPagedEnumerable<string?> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1, null, 3));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<string?>>(json, options);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.Items[1].Should().BeNull();
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithCustomJsonOptions_ShouldApplyOptions()
    {
        // Arrange
        var items = new[] { new Product(1, "test_product") };
        IAsyncPagedEnumerable<Product> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // Use CamelCase instead of SnakeCaseLower
            WriteIndented = true
        };

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("pageSize");  // camelCase
        json.Should().Contain("currentPage"); // camelCase
        json.Should().Contain("test_product");
    }

    [Fact]
    public void SerializeAsyncPaged_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Arrange
        var pagedEnumerable = CreatePagedEnumerable(new[] { 1, 2, 3 }, Pagination.Create(10, 1));
        Stream nullStream = null!;

        // Act
        Func<Task> act = async () => await JsonSerializer.SerializeAsyncPaged(nullStream, pagedEnumerable);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void SerializeAsyncPaged_WithNullPagedEnumerable_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        Func<Task> act = async () => await JsonSerializer.SerializeAsyncPaged<int>(stream, null!);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithUnicodeCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        var items = new[] { "Hello 世界", "Привет мир", "مرحبا العالم" };
        IAsyncPagedEnumerable<string> pagedEnumerable = CreatePagedEnumerable(
          items,
          Pagination.Create(10, 1, null, 3));

        using var stream = new MemoryStream();
        var options = new JsonSerializerOptions();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        string json = await new StreamReader(stream, Encoding.UTF8).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<string>>(json, options);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.Items.Should().Contain("Hello 世界");
        result.Items.Should().Contain("Привет мир");
        result.Items.Should().Contain("مرحبا العالم");
    }

    // Helper methods and types
    private static IAsyncPagedEnumerable<T> CreatePagedEnumerable<T>(T[] items, Pagination pagination)
    {
        async IAsyncEnumerable<T> AsyncSource()
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        return new AsyncPagedEnumerable<T>(
          AsyncSource(),
          _ => new ValueTask<Pagination>(pagination));
    }

    private static IAsyncPagedEnumerable<int> CreateSlowPagedEnumerable(int count, TimeSpan delay)
    {
        async IAsyncEnumerable<int> AsyncSource()
        {
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(delay);
                yield return i;
            }
        }

        return new AsyncPagedEnumerable<int>(
          AsyncSource(),
          _ => new ValueTask<Pagination>(Pagination.Create(10, 1, null, count)));
    }

    // Test DTOs
    private record Product(int Id, string Name);
    private record Person(string Name, int Age);
    private record Customer(string Name, string Email);
    private record Order(int Id, Customer Customer, string[] ItemNames);

    private class PagedResponse<T>
    {
        public Pagination Pagination { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
