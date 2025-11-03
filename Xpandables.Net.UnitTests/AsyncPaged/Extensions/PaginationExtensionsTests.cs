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

using FluentAssertions;
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

namespace Xpandables.Net.UnitTests.AsyncPaged.Extensions;

/// <summary>
/// Unit tests for pagination extension methods in <see cref="PaginationExtensions"/>.
/// </summary>
public sealed class PaginationExtensionsTests
{
    #region TakePaged / SkipPaged

    [Fact]
    public async Task TakePaged_WithValidCount_ShouldReturnSpecifiedNumberOfElements()
  {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

        // Act
  var result = source.TakePaged(3);
        var items = await result.ToListAsync();

 // Assert
        items.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task TakePaged_WithZeroCount_ShouldReturnEmpty()
    {
        // Arrange
      var source = CreatePagedEnumerable(1, 2, 3);

      // Act
        var result = source.TakePaged(0);
        var items = await result.ToListAsync();

        // Assert
        items.Should().BeEmpty();
    }

    [Fact]
    public void TakePaged_WithNegativeCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
  var source = CreatePagedEnumerable(1, 2, 3);

     // Act
   Action act = () => source.TakePaged(-1);

   // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task SkipPaged_WithValidCount_ShouldSkipSpecifiedElements()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = source.SkipPaged(2);
        var items = await result.ToListAsync();

        // Assert
        items.Should().Equal(3, 4, 5);
}

    [Fact]
    public async Task SkipPaged_WithZeroCount_ShouldReturnAllElements()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3);

        // Act
        var result = source.SkipPaged(0);
        var items = await result.ToListAsync();

        // Assert
  items.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void SkipPaged_WithNegativeCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3);

     // Act
        Action act = () => source.SkipPaged(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region TakeWhilePaged / SkipWhilePaged

    [Fact]
    public async Task TakeWhilePaged_WithPredicate_ShouldTakeWhileConditionIsTrue()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

        // Act
     var result = source.TakeWhilePaged(x => x < 4);
     var items = await result.ToListAsync();

      // Assert
        items.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task TakeWhilePaged_WithIndexedPredicate_ShouldUseIndex()
 {
        // Arrange
        var source = CreatePagedEnumerable(10, 20, 30, 40, 50);

   // Act
        var result = source.TakeWhilePaged((value, index) => index < 3);
      var items = await result.ToListAsync();

        // Assert
    items.Should().Equal(10, 20, 30);
    }

    [Fact]
    public async Task SkipWhilePaged_WithPredicate_ShouldSkipWhileConditionIsTrue()
    {
        // Arrange
      var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

      // Act
   var result = source.SkipWhilePaged(x => x < 3);
        var items = await result.ToListAsync();

        // Assert
      items.Should().Equal(3, 4, 5);
    }

    [Fact]
    public async Task SkipWhilePaged_WithIndexedPredicate_ShouldUseIndex()
    {
        // Arrange
        var source = CreatePagedEnumerable(10, 20, 30, 40, 50);

        // Act
  var result = source.SkipWhilePaged((value, index) => index < 2);
        var items = await result.ToListAsync();

        // Assert
  items.Should().Equal(30, 40, 50);
    }

    #endregion

    #region TakeLastPaged / SkipLastPaged

    [Fact]
    public async Task TakeLastPaged_ShouldReturnLastNElements()
    {
      // Arrange
        var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

    // Act
        var result = source.TakeLastPaged(3);
      var items = await result.ToListAsync();

        // Assert
  items.Should().Equal(3, 4, 5);
    }

    [Fact]
    public async Task TakeLastPaged_WithZeroCount_ShouldReturnEmpty()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3);

        // Act
        var result = source.TakeLastPaged(0);
      var items = await result.ToListAsync();

        // Assert
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task SkipLastPaged_ShouldSkipLastNElements()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

  // Act
    var result = source.SkipLastPaged(2);
     var items = await result.ToListAsync();

      // Assert
        items.Should().Equal(1, 2, 3);
    }

  [Fact]
    public async Task SkipLastPaged_WithZeroCount_ShouldReturnAllElements()
    {
        // Arrange
    var source = CreatePagedEnumerable(1, 2, 3);

      // Act
        var result = source.SkipLastPaged(0);
        var items = await result.ToListAsync();

   // Assert
      items.Should().Equal(1, 2, 3);
    }

 #endregion

    #region ChunkPaged

 [Fact]
    public async Task ChunkPaged_ShouldSplitIntoChunksOfSpecifiedSize()
    {
        // Arrange
   var source = CreatePagedEnumerable(1, 2, 3, 4, 5, 6, 7);

        // Act
        var result = source.ChunkPaged(3);
        var chunks = await result.ToListAsync();

        // Assert
        chunks.Should().HaveCount(3);
   chunks[0].Should().Equal(1, 2, 3);
  chunks[1].Should().Equal(4, 5, 6);
     chunks[2].Should().Equal(7);
    }

    [Fact]
    public void ChunkPaged_WithZeroSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 3);

 // Act
    Action act = () => source.ChunkPaged(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region DistinctPaged

    [Fact]
    public async Task DistinctPaged_ShouldReturnUniqueElements()
    {
        // Arrange
        var source = CreatePagedEnumerable(1, 2, 2, 3, 3, 3, 4);

  // Act
        var result = source.DistinctPaged();
        var items = await result.ToListAsync();

   // Assert
        items.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public async Task DistinctPaged_WithComparer_ShouldUseCustomComparer()
  {
      // Arrange
     var source = CreatePagedEnumerable("apple", "APPLE", "banana", "BANANA");

        // Act
var result = source.DistinctPaged(StringComparer.OrdinalIgnoreCase);
 var items = await result.ToListAsync();

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain("apple");
        items.Should().Contain("banana");
    }

    [Fact]
    public async Task DistinctByPaged_WithKeySelector_ShouldReturnDistinctByKey()
    {
        // Arrange
      var source = CreatePagedEnumerable(
      new Person("Alice", 30),
            new Person("Bob", 25),
 new Person("Alice", 35));

        // Act
        var result = source.DistinctByPaged(p => p.Name);
        var items = await result.ToListAsync();

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(p => p.Name == "Alice");
        items.Should().Contain(p => p.Name == "Bob");
    }

    #endregion

    #region WherePaged

    [Fact]
    public async Task WherePaged_WithPredicate_ShouldFilterElements()
    {
        // Arrange
     var source = CreatePagedEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = source.WherePaged(x => x % 2 == 0);
        var items = await result.ToListAsync();

      // Assert
        items.Should().Equal(2, 4);
    }

    [Fact]
    public async Task WherePaged_WithIndexedPredicate_ShouldUseIndex()
    {
        // Arrange
        var source = CreatePagedEnumerable(10, 20, 30, 40, 50);

        // Act
        var result = source.WherePaged((value, index) => index % 2 == 0);
    var items = await result.ToListAsync();

        // Assert
        items.Should().Equal(10, 30, 50);
    }

    [Fact]
    public void WherePaged_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Arrange
    var source = CreatePagedEnumerable(1, 2, 3);

        // Act
    Action act = () => source.WherePaged((Func<int, bool>)null!);

     // Assert
     act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Pagination Preservation

    [Fact]
public async Task PaginationExtensions_ShouldPreservePaginationMetadata()
    {
  // Arrange
        var expectedPagination = Pagination.Create(10, 2, "token123", 100);
        var source = CreatePagedEnumerableWithPagination(
          new[] { 1, 2, 3, 4, 5 },
          expectedPagination);

        // Act
        var result = source.TakePaged(3);
        var actualPagination = await result.GetPaginationAsync();

        // Assert
        actualPagination.Should().Be(expectedPagination);
    }

    [Fact]
    public async Task CombinedOperations_ShouldWorkCorrectly()
    {
      // Arrange
        var source = CreatePagedEnumerable(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

        // Act
      var result = source
     .WherePaged(x => x > 2)
     .SkipPaged(2)
 .TakePaged(3);
        var items = await result.ToListAsync();

        // Assert
        items.Should().Equal(5, 6, 7);
    }

    #endregion

    #region Null/Edge Case Tests

    [Fact]
    public void TakePaged_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IAsyncPagedEnumerable<int> nullSource = null!;

     // Act
        Action act = () => nullSource.TakePaged(5);

    // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SkipPaged_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
     IAsyncPagedEnumerable<int> nullSource = null!;

        // Act
        Action act = () => nullSource.SkipPaged(5);

        // Assert
     act.Should().Throw<ArgumentNullException>();
    }

 [Fact]
    public async Task TakePaged_WithCountGreaterThanSourceLength_ShouldReturnAllElements()
    {
     // Arrange
 var source = CreatePagedEnumerable(1, 2, 3);

        // Act
        var result = source.TakePaged(10);
   var items = await result.ToListAsync();

        // Assert
        items.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task SkipPaged_WithCountGreaterThanSourceLength_ShouldReturnEmpty()
    {
        // Arrange
    var source = CreatePagedEnumerable(1, 2, 3);

        // Act
        var result = source.SkipPaged(10);
        var items = await result.ToListAsync();

        // Assert
        items.Should().BeEmpty();
    }

    #endregion

    // Helper methods
    private static IAsyncPagedEnumerable<T> CreatePagedEnumerable<T>(params T[] items)
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
    _ => new ValueTask<Pagination>(Pagination.Create(items.Length, 1, null, items.Length)));
    }

    private static IAsyncPagedEnumerable<T> CreatePagedEnumerableWithPagination<T>(
        T[] items,
        Pagination pagination)
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

    private record Person(string Name, int Age);
}

/// <summary>
/// Helper extension to convert IAsyncPagedEnumerable to List.
/// </summary>
internal static class AsyncPagedEnumerableTestExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncPagedEnumerable<T> source)
    {
  var list = new List<T>();
        await foreach (var item in source)
        {
      list.Add(item);
        }
        return list;
    }
}
