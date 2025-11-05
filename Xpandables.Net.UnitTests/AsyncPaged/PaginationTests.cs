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
/// Unit tests for the <see cref="Pagination"/> struct.
/// </summary>
public sealed class PaginationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnPaginationWithCorrectValues()
    {
        // Arrange
        const int pageSize = 10;
        const int currentPage = 2;
        const string continuationToken = "token123";
        const int totalCount = 100;

        // Act
        Pagination pagination = Pagination.Create(pageSize, currentPage, continuationToken, totalCount);

        // Assert
        pagination.PageSize.Should().Be(pageSize);
        pagination.CurrentPage.Should().Be(currentPage);
        pagination.ContinuationToken.Should().Be(continuationToken);
        pagination.TotalCount.Should().Be(totalCount);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void Create_WithNegativeValues_ShouldThrowArgumentOutOfRangeException(int pageSize, int currentPage)
    {
        // Act
        Action act = () => Pagination.Create(pageSize, currentPage);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeTotalCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        Action act = () => Pagination.Create(10, 1, null, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FromTotalCount_WithValidTotalCount_ShouldReturnPaginationWithTotalCount()
    {
        // Arrange
        const int totalCount = 50;

        // Act
        Pagination pagination = Pagination.FromTotalCount(totalCount);

        // Assert
        pagination.TotalCount.Should().Be(totalCount);
        pagination.PageSize.Should().Be(0);
        pagination.CurrentPage.Should().Be(0);
        pagination.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public void FromTotalCount_WithNegativeTotalCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        Action act = () => Pagination.FromTotalCount(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Empty_ShouldReturnDefaultPagination()
    {
        // Act
        Pagination pagination = Pagination.Empty;

        // Assert
        pagination.PageSize.Should().Be(0);
        pagination.CurrentPage.Should().Be(0);
        pagination.ContinuationToken.Should().BeNull();
        pagination.TotalCount.Should().BeNull();
    }

    [Fact]
    public void NextPage_ShouldIncrementCurrentPageAndUpdateToken()
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 1, null, 100);
        const string newToken = "nextToken";

        // Act
        Pagination nextPage = pagination.NextPage(newToken);

        // Assert
        nextPage.CurrentPage.Should().Be(2);
        nextPage.ContinuationToken.Should().Be(newToken);
        nextPage.PageSize.Should().Be(pagination.PageSize);
        nextPage.TotalCount.Should().Be(pagination.TotalCount);
    }

    [Fact]
    public void PreviousPage_WhenHasPreviousPage_ShouldDecrementCurrentPage()
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 3, "token", 100);

        // Act
        Pagination previousPage = pagination.PreviousPage();

        // Assert
        previousPage.CurrentPage.Should().Be(2);
        previousPage.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public void PreviousPage_WhenOnFirstPage_ShouldReturnSamePagination()
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 1, "token", 100);

        // Act
        Pagination previousPage = pagination.PreviousPage();

        // Assert
        previousPage.Should().Be(pagination);
    }

    [Fact]
    public void WithTotalCount_ShouldUpdateTotalCount()
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 1);
        const int newTotalCount = 200;

        // Act
        Pagination updated = pagination.WithTotalCount(newTotalCount);

        // Assert
        updated.TotalCount.Should().Be(newTotalCount);
        updated.PageSize.Should().Be(pagination.PageSize);
        updated.CurrentPage.Should().Be(pagination.CurrentPage);
    }

    [Fact]
    public void WithTotalCount_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 1);

        // Act
        Action act = () => pagination.WithTotalCount(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(0, false)]
    [InlineData(100, false)]
    public void IsUnknown_ShouldReturnCorrectValue(int? totalCount, bool expectedIsUnknown)
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 1, null, totalCount);

        // Act & Assert
        pagination.IsUnknown.Should().Be(expectedIsUnknown);
    }

    [Theory]
    [InlineData(10, 1, 0)]
    [InlineData(10, 2, 10)]
    [InlineData(10, 5, 40)]
    [InlineData(0, 5, 0)]
    public void Skip_ShouldCalculateCorrectSkipValue(int pageSize, int currentPage, int expectedSkip)
    {
        // Arrange
        Pagination pagination = Pagination.Create(pageSize, currentPage);

        // Act & Assert
        pagination.Skip.Should().Be(expectedSkip);
    }

    [Fact]
    public void Take_ShouldReturnPageSize()
    {
        // Arrange
        const int pageSize = 25;
        Pagination pagination = Pagination.Create(pageSize, 1);

        // Act & Assert
        pagination.Take.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("token", true)]
    public void HasContinuation_ShouldReturnCorrectValue(string? token, bool expectedHasContinuation)
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 1, token, 100);

        // Act & Assert
        pagination.HasContinuation.Should().Be(expectedHasContinuation);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, true)]
    [InlineData(2, false)]
    public void IsFirstPage_ShouldReturnCorrectValue(int currentPage, bool expectedIsFirstPage)
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, currentPage);

        // Act & Assert
        pagination.IsFirstPage.Should().Be(expectedIsFirstPage);
    }

    [Theory]
    [InlineData(10, 10, 100, true)]  // Last page
    [InlineData(10, 9, 100, false)]  // Not last page
    [InlineData(10, 1, null, false)] // Unknown total
    [InlineData(0, 1, 100, false)]   // Zero page size
    public void IsLastPage_ShouldReturnCorrectValue(int pageSize, int currentPage, int? totalCount, bool expectedIsLastPage)
    {
        // Arrange
        Pagination pagination = Pagination.Create(pageSize, currentPage, null, totalCount);

        // Act & Assert
        pagination.IsLastPage.Should().Be(expectedIsLastPage);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public void HasPreviousPage_ShouldReturnCorrectValue(int currentPage, bool expectedHasPreviousPage)
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, currentPage);

        // Act & Assert
        pagination.HasPreviousPage.Should().Be(expectedHasPreviousPage);
    }

    [Theory]
    [InlineData(10, 5, 100, true)] // Has next page
    [InlineData(10, 10, 100, false)] // Last page
    [InlineData(10, 1, null, false)] // Unknown total
    [InlineData(0, 1, 100, false)]   // Zero page size
    public void HasNextPage_ShouldReturnCorrectValue(int pageSize, int currentPage, int? totalCount, bool expectedHasNextPage)
    {
        // Arrange
        Pagination pagination = Pagination.Create(pageSize, currentPage, null, totalCount);

        // Act & Assert
        pagination.HasNextPage.Should().Be(expectedHasNextPage);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(10, 0, false)] // PageSize > 0 but currentPage 0
    [InlineData(0, 5, false)] // CurrentPage > 0 but pageSize 0
    [InlineData(10, 5, true)]
    public void IsPaginated_ShouldReturnCorrectValue(int skip, int take, bool expectedIsPaginated)
    {
        // Arrange
        int pageSize = take;
        int currentPage = skip > 0 && take > 0 ? (skip / take) + 1 : 0;
        Pagination pagination = Pagination.Create(pageSize, currentPage);

        // Act & Assert
        pagination.IsPaginated.Should().Be(expectedIsPaginated);
    }

    [Theory]
    [InlineData(10, 100, 10)]
    [InlineData(10, 95, 10)]
    [InlineData(10, 101, 11)]
    [InlineData(0, 100, null)]
    [InlineData(10, null, null)]
    public void TotalPages_ShouldCalculateCorrectly(int pageSize, int? totalCount, int? expectedTotalPages)
    {
        // Arrange
        Pagination pagination = Pagination.Create(pageSize, 1, null, totalCount);

        // Act & Assert
        pagination.TotalPages.Should().Be(expectedTotalPages);
    }

    [Fact]
    public void Pagination_ShouldBeRecordStruct_WithValueSemantics()
    {
        // Arrange
        Pagination pagination1 = Pagination.Create(10, 1, "token", 100);
        Pagination pagination2 = Pagination.Create(10, 1, "token", 100);
        Pagination pagination3 = Pagination.Create(10, 2, "token", 100);

        // Act & Assert
        (pagination1 == pagination2).Should().BeTrue();
        (pagination1 != pagination3).Should().BeTrue();
        pagination1.GetHashCode().Should().Be(pagination2.GetHashCode());
    }

    [Fact]
    public void Pagination_ToString_ShouldReturnReadableString()
    {
        // Arrange
        Pagination pagination = Pagination.Create(10, 2, "token123", 100);

        // Act
        string result = pagination.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("10"); // PageSize
        result.Should().Contain("2");  // CurrentPage
    }
}
