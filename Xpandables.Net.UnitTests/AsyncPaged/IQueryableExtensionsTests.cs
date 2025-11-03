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

namespace Xpandables.Net.UnitTests.AsyncPaged;

/// <summary>
/// Unit tests for IQueryable extension methods.
/// </summary>
public sealed class IQueryableExtensionsTests
{
    [Fact]
    public void ToAsyncPagedEnumerable_WithValidQueryable_ShouldReturnPagedEnumerable()
{
        // Arrange
     IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable();

      // Act
      IAsyncPagedEnumerable<int> result = queryable.ToAsyncPagedEnumerable();

        // Assert
        result.Should().NotBeNull();
      result.Type.Should().Be(typeof(int));
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithNullQueryable_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<int> nullQueryable = null!;

// Act
        Action act = () => nullQueryable.ToAsyncPagedEnumerable();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithSkipAndTake_ShouldExtractPaginationCorrectly()
    {
   // Arrange
        IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable()
 .Skip(20)
            .Take(10);

        // Act
   IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
   Pagination pagination = await pagedEnumerable.GetPaginationAsync();

  // Assert
   pagination.PageSize.Should().Be(10);
   pagination.CurrentPage.Should().Be(3); // (20 / 10) + 1
 pagination.TotalCount.Should().Be(100);
  }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithOnlySkip_ShouldHandleCorrectly()
    {
        // Arrange
    IQueryable<int> queryable = Enumerable.Range(1, 50).AsQueryable()
            .Skip(10);

        // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
    Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(0);
     pagination.CurrentPage.Should().Be(0);
        pagination.TotalCount.Should().Be(50);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithOnlyTake_ShouldHandleCorrectly()
    {
        // Arrange
        IQueryable<int> queryable = Enumerable.Range(1, 50).AsQueryable()
            .Take(15);

  // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(15);
        pagination.CurrentPage.Should().Be(1);
        pagination.TotalCount.Should().Be(50);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithoutPaginationOperators_ShouldReturnDefaultPagination()
    {
        // Arrange
     IQueryable<int> queryable = Enumerable.Range(1, 25).AsQueryable();

        // Act
  IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
  pagination.PageSize.Should().Be(0);
      pagination.CurrentPage.Should().Be(0);
        pagination.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_ShouldEnumerateAllItems()
    {
 // Arrange
        IQueryable<int> queryable = Enumerable.Range(1, 10).AsQueryable()
   .Skip(3)
            .Take(4);

    // Act
IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        var items = new List<int>();
        await foreach (var item in pagedEnumerable)
  {
      items.Add(item);
        }

    // Assert
        items.Should().Equal(4, 5, 6, 7);
    }

    [Fact]
    public void ToAsyncPagedEnumerable_WithCustomTotalFactory_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable();
        Func<CancellationToken, ValueTask<long>> nullFactory = null!;

        // Act
Action act = () => queryable.ToAsyncPagedEnumerable(nullFactory);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithCustomTotalFactory_ShouldUseCustomTotal()
    {
        // Arrange
  IQueryable<int> queryable = Enumerable.Range(1, 50).AsQueryable()
  .Skip(10)
  .Take(5);
        Func<CancellationToken, ValueTask<long>> customTotalFactory = _ =>
            new ValueTask<long>(500); // Custom total count

        // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable(customTotalFactory);
Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.TotalCount.Should().Be(500);
   pagination.PageSize.Should().Be(5);
        pagination.CurrentPage.Should().Be(3); // (10 / 5) + 1
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithComplexQuery_ShouldCalculatePaginationCorrectly()
    {
        // Arrange
        IQueryable<Person> queryable = new[]
        {
            new Person("Alice", 30),
       new Person("Bob", 25),
            new Person("Charlie", 35),
          new Person("David", 40),
         new Person("Eve", 28)
        }.AsQueryable()
        .Where(p => p.Age > 25)
        .OrderBy(p => p.Name)
        .Skip(1)
        .Take(2);

   // Act
  IAsyncPagedEnumerable<Person> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();
        var items = new List<Person>();
 await foreach (var item in pagedEnumerable)
        {
   items.Add(item);
        }

  // Assert
        pagination.PageSize.Should().Be(2);
    // Skip 1 with page size 2 means (1 / 2) + 1 = 1 (integer division)
        pagination.CurrentPage.Should().Be(1);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithEmptyQueryable_ShouldHandleCorrectly()
    {
      // Arrange
        IQueryable<int> queryable = Enumerable.Empty<int>().AsQueryable();

        // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();
        var items = new List<int>();
     await foreach (var item in pagedEnumerable)
        {
      items.Add(item);
        }

        // Assert
        pagination.TotalCount.Should().Be(0);
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithMultipleSkipTakeCalls_ShouldUseLatestValues()
    {
      // Arrange
     IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable()
 .Skip(10)
        .Take(20)
         .Skip(5)
     .Take(10);

        // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
  // The query analyzer should handle multiple Skip/Take operations
        pagination.PageSize.Should().BeGreaterThan(0);
        pagination.TotalCount.Should().Be(100);
    }

[Fact]
    public async Task ToAsyncPagedEnumerable_WithCancellation_ShouldRespectCancellationToken()
    {
 // Arrange
  using var cts = new CancellationTokenSource();
        IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable()
            .Skip(10)
    .Take(20);

     // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable(async ct =>
        {
     await Task.Delay(200, ct);
return 100;
        });

        cts.CancelAfter(50);
    Func<Task> act = async () => await pagedEnumerable.GetPaginationAsync(cts.Token);

    // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithFirstPageRequest_ShouldCalculateCorrectly()
    {
        // Arrange - Simulating first page request
        const int pageSize = 10;
        const int pageNumber = 1;
    IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable()
       .Skip((pageNumber - 1) * pageSize)
  .Take(pageSize);

        // Act
   IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
  Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(10);
        pagination.CurrentPage.Should().Be(1);
        pagination.TotalCount.Should().Be(100);
      pagination.TotalPages.Should().Be(10);
        pagination.HasPreviousPage.Should().BeFalse();
        pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithLastPageRequest_ShouldCalculateCorrectly()
    {
        // Arrange - Simulating last page request
   const int pageSize = 10;
        const int pageNumber = 10;
        IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable()
 .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize);

        // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
        Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(10);
        pagination.CurrentPage.Should().Be(10);
        pagination.TotalCount.Should().Be(100);
        pagination.IsLastPage.Should().BeTrue();
    pagination.HasNextPage.Should().BeFalse();
        pagination.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task ToAsyncPagedEnumerable_WithMiddlePageRequest_ShouldCalculateCorrectly()
    {
        // Arrange - Simulating middle page request
  const int pageSize = 15;
        const int pageNumber = 4;
        IQueryable<int> queryable = Enumerable.Range(1, 100).AsQueryable()
      .Skip((pageNumber - 1) * pageSize)
       .Take(pageSize);

        // Act
        IAsyncPagedEnumerable<int> pagedEnumerable = queryable.ToAsyncPagedEnumerable();
      Pagination pagination = await pagedEnumerable.GetPaginationAsync();

        // Assert
        pagination.PageSize.Should().Be(15);
      pagination.CurrentPage.Should().Be(4);
        pagination.TotalCount.Should().Be(100);
        pagination.HasPreviousPage.Should().BeTrue();
      pagination.HasNextPage.Should().BeTrue();
      pagination.IsFirstPage.Should().BeFalse();
        pagination.IsLastPage.Should().BeFalse();
    }

    // Helper record
    private record Person(string Name, int Age);
}
