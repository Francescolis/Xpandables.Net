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
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Xpandables.Net.Async;
using Xpandables.Net.UnitTests.Helpers;

namespace Xpandables.Net.UnitTests.Json;

/// <summary>
/// Tests for JsonSerializer extension methods that serialize IAsyncPagedEnumerable types.
/// </summary>
public class JsonSerializerExtensionsTests
{
    #region Stream Generic Tests with JsonTypeInfo

    [Fact]
    public async Task SerializeAsyncPaged_Stream_Generic_WithJsonTypeInfo_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser(1, "Alice", "alice@test.com"),
            new TestUser(2, "Bob", "bob@test.com"),
            new TestUser(3, "Charlie", "charlie@test.com")
        };

        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(3);
        json.RootElement.GetProperty("pagination").GetProperty("PageSize").GetInt32().Should().Be(10);
        json.RootElement.GetProperty("pagination").GetProperty("CurrentPage").GetInt32().Should().Be(1);

        var items = json.RootElement.GetProperty("items");
        items.GetArrayLength().Should().Be(3);
        items[0].GetProperty("Id").GetInt32().Should().Be(1);
        items[0].GetProperty("Name").GetString().Should().Be("Alice");
        items[1].GetProperty("Name").GetString().Should().Be("Bob");
        items[2].GetProperty("Name").GetString().Should().Be("Charlie");
    }

    [Fact]
    public async Task SerializeAsyncPaged_Stream_Generic_WithEmptyEnumerable_ShouldSerializeEmptyArray()
    {
        // Arrange
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            Array.Empty<TestUser>().ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 0)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(0);
        json.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SerializeAsyncPaged_Stream_Generic_WithLargeDataSet_ShouldStreamEfficiently()
    {
        // Arrange
        const int itemCount = 1000;
        var users = Enumerable.Range(1, itemCount)
            .Select(i => new TestUser(i, $"User{i}", $"user{i}@test.com"))
            .ToArray();

        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(100, 1, totalCount: itemCount)));

        using var stream = new MemoryStream();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);
        stopwatch.Stop();

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(itemCount);
        json.RootElement.GetProperty("pagination").GetProperty("TotalCount").GetInt32().Should().Be(itemCount);

        // Should complete in reasonable time
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Stream Generic Tests with JsonSerializerOptions

    [Fact]
    public async Task SerializeAsyncPaged_Stream_Generic_WithOptions_ShouldApplyOptions()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.TypeInfoResolverChain.Add(TestUserContext.Default);

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());

        jsonString.Should().Contain("  "); // Indented
        jsonString.Should().Contain("\"name\""); // camelCase

        var json = JsonDocument.Parse(jsonString);
        json.RootElement.GetProperty("items")[0].GetProperty("name").GetString().Should().Be("Alice");
    }

    #endregion

    #region PipeWriter Generic Tests

    [Fact]
    public async Task SerializeAsyncPaged_PipeWriter_Generic_WithJsonTypeInfo_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser(1, "Alice", "alice@test.com"),
            new TestUser(2, "Bob", "bob@test.com")
        };

        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 2)));

        var pipe = new Pipe();

        // Act
        var writeTask = JsonSerializer.SerializeAsyncPaged(
            pipe.Writer,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        await writeTask;
        pipe.Writer.Complete();

        // Assert
        var readResult = await pipe.Reader.ReadAsync();
        var jsonString = Encoding.UTF8.GetString(ReadOnlySequenceToArray(readResult.Buffer));
        pipe.Reader.Complete();

        var json = JsonDocument.Parse(jsonString);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("Alice");
    }

    [Fact]
    public async Task SerializeAsyncPaged_PipeWriter_Generic_WithOptions_ShouldApplyOptions()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Test", "test@test.com") };
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };
        options.TypeInfoResolverChain.Add(TestUserContext.Default);

        var pipe = new Pipe();

        // Act
        var writeTask = JsonSerializer.SerializeAsyncPaged(pipe.Writer, pagedEnumerable, options);
        await writeTask;
        pipe.Writer.Complete();

        // Assert
        var readResult = await pipe.Reader.ReadAsync();
        var jsonString = Encoding.UTF8.GetString(ReadOnlySequenceToArray(readResult.Buffer));
        pipe.Reader.Complete();

        jsonString.Should().NotContain("\n"); // Not indented
    }

    #endregion

    #region Stream Non-Generic Tests

    [Fact]
    public async Task SerializeAsyncPaged_Stream_NonGeneric_WithJsonTypeInfo_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser(1, "Alice", "alice@test.com"),
            new TestUser(2, "Bob", "bob@test.com")
        };

        IAsyncPagedEnumerable pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 2)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(2);
        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("Alice");
    }

    [Fact]
    public async Task SerializeAsyncPaged_Stream_NonGeneric_WithContext_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        IAsyncPagedEnumerable pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("Alice");
    }

    [Fact]
    public async Task SerializeAsyncPaged_Stream_NonGeneric_WithOptions_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        IAsyncPagedEnumerable pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var options = new JsonSerializerOptions();
        options.TypeInfoResolverChain.Add(TestUserContext.Default);

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(stream, pagedEnumerable, options);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    #endregion

    #region PipeWriter Non-Generic Tests

    [Fact]
    public async Task SerializeAsyncPaged_PipeWriter_NonGeneric_WithJsonTypeInfo_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        IAsyncPagedEnumerable pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var pipe = new Pipe();

        // Act
        var writeTask = JsonSerializer.SerializeAsyncPaged(
            pipe.Writer,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        await writeTask;
        pipe.Writer.Complete();

        // Assert
        var readResult = await pipe.Reader.ReadAsync();
        var jsonString = Encoding.UTF8.GetString(ReadOnlySequenceToArray(readResult.Buffer));
        pipe.Reader.Complete();

        var json = JsonDocument.Parse(jsonString);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task SerializeAsyncPaged_PipeWriter_NonGeneric_WithContext_ShouldSerializeCorrectly()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        IAsyncPagedEnumerable pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        var pipe = new Pipe();

        // Act
        var writeTask = JsonSerializer.SerializeAsyncPaged(
            pipe.Writer,
            pagedEnumerable,
            TestUserContext.Default);

        await writeTask;
        pipe.Writer.Complete();

        // Assert
        var readResult = await pipe.Reader.ReadAsync();
        var jsonString = Encoding.UTF8.GetString(ReadOnlySequenceToArray(readResult.Buffer));
        pipe.Reader.Complete();

        var json = JsonDocument.Parse(jsonString);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    #endregion

    #region Special Cases Tests

    [Fact]
    public async Task SerializeAsyncPaged_WithUnicodeCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser(1, "日本語", "test@日本.jp"),
            new TestUser(2, "Émoji 😀", "emoji@test.com"),
            new TestUser(3, "Müller", "müller@test.de")
        };

        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("日本語");
        json.RootElement.GetProperty("items")[1].GetProperty("Name").GetString().Should().Be("Émoji 😀");
        json.RootElement.GetProperty("items")[2].GetProperty("Name").GetString().Should().Be("Müller");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithSpecialJsonCharacters_ShouldEscapeProperly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser(1, "User with \"quotes\"", "test@test.com"),
            new TestUser(2, "User with \n newline", "test2@test.com"),
            new TestUser(3, "User with \\ backslash", "test3@test.com")
        };

        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("items")[0].GetProperty("Name").GetString().Should().Be("User with \"quotes\"");
        json.RootElement.GetProperty("items")[1].GetProperty("Name").GetString().Should().Be("User with \n newline");
        json.RootElement.GetProperty("items")[2].GetProperty("Name").GetString().Should().Be("User with \\ backslash");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var slowEnumerable = CreateSlowPagedEnumerable();
        using var stream = new MemoryStream();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act & Assert
        var act = async () => await JsonSerializer.SerializeAsyncPaged(
            stream,
            slowEnumerable,
            TestUserContext.Default.TestUser,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        // Act & Assert
        var act = async () => await JsonSerializer.SerializeAsyncPaged(
            (Stream)null!,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithNullPagedEnumerable_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act & Assert
        var act = async () => await JsonSerializer.SerializeAsyncPaged(
            stream,
            (IAsyncPagedEnumerable<TestUser>)null!,
            TestUserContext.Default.TestUser);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithNullJsonTypeInfo_ShouldThrowArgumentNullException()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 1)));

        using var stream = new MemoryStream();

        // Act & Assert
        var act = async () => await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            (System.Text.Json.Serialization.Metadata.JsonTypeInfo<TestUser>)null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Pagination Metadata Tests

    [Fact]
    public async Task SerializeAsyncPaged_WithComplexPagination_ShouldSerializeMetadataCorrectly()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(
                pageSize: 20,
                currentPage: 3,
                continuationToken: "abc123",
                totalCount: 100)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        var pagination = json.RootElement.GetProperty("pagination");
        pagination.GetProperty("PageSize").GetInt32().Should().Be(20);
        pagination.GetProperty("CurrentPage").GetInt32().Should().Be(3);
        pagination.GetProperty("TotalCount").GetInt32().Should().Be(100);
        pagination.GetProperty("ContinuationToken").GetString().Should().Be("abc123");
    }

    [Fact]
    public async Task SerializeAsyncPaged_WithUnknownTotalCount_ShouldSerializeNullTotalCount()
    {
        // Arrange
        var users = new[] { new TestUser(1, "Alice", "alice@test.com") };
        var pagedEnumerable = new AsyncPagedEnumerable<TestUser>(
            users.ToAsync(),
            ct => ValueTask.FromResult(Pagination.Create(
                pageSize: 10,
                currentPage: 1,
                totalCount: null)));

        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsyncPaged(
            stream,
            pagedEnumerable,
            TestUserContext.Default.TestUser);

        // Assert
        stream.Position = 0;
        var json = await JsonDocument.ParseAsync(stream);

        json.RootElement.GetProperty("pagination").GetProperty("TotalCount").ValueKind.Should().Be(JsonValueKind.Null);
    }

    #endregion

    #region Helper Methods

    private static IAsyncPagedEnumerable<TestUser> CreateSlowPagedEnumerable()
    {
        return new AsyncPagedEnumerable<TestUser>(
            SlowAsyncEnumerable(),
            ct => ValueTask.FromResult(Pagination.Create(10, 1, totalCount: 3)));

        static async IAsyncEnumerable<TestUser> SlowAsyncEnumerable()
        {
            for (int i = 1; i <= 3; i++)
            {
                await Task.Delay(100);
                yield return new TestUser(i, $"User{i}", $"user{i}@test.com");
            }
        }
    }

    private static byte[] ReadOnlySequenceToArray(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsSingleSegment)
        {
            return sequence.FirstSpan.ToArray();
        }

        return sequence.ToArray();
    }

    #endregion
}

/// <summary>
/// Test user record for serialization tests.
/// </summary>
internal record TestUser(int Id, string Name, string Email);

/// <summary>
/// Source-generated JSON serialization context for test types.
/// </summary>
[JsonSerializable(typeof(TestUser))]
internal partial class TestUserContext : JsonSerializerContext { }
