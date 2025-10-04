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
using System.Net.Async;
using System.Net.UnitTests.Helpers;

using FluentAssertions;

namespace System.Net.UnitTests;

public class AsyncPagedEnumerableJoinExtensionsTests
{
    private record Customer(int Id, string Name, string Department);
    private record Order(int Id, int CustomerId, string Product, decimal Amount);
    private record JoinResult(string CustomerName, string Product, decimal Amount);
    private record GroupJoinResult(string CustomerName, IEnumerable<Order> Orders);

    private static IAsyncPagedEnumerable<Customer> CreateCustomers()
    {
        var customers = new[]
        {
            new Customer(1, "Alice", "Engineering"),
            new Customer(2, "Bob", "Sales"),
            new Customer(3, "Charlie", "Engineering"),
            new Customer(4, "Diana", "Marketing")
        };
        return new AsyncPagedEnumerable<Customer, Customer>(
            customers.ToAsync(),
            ct => ValueTask.FromResult(PageContext.Create(4, 1, totalCount: 4)));
    }

    private static IAsyncEnumerable<Order> CreateOrders()
    {
        var orders = new[]
        {
            new Order(1, 1, "Laptop", 1200m),
            new Order(2, 1, "Mouse", 25m),
            new Order(3, 2, "Keyboard", 80m),
            new Order(4, 3, "Monitor", 300m),
            new Order(5, 3, "Webcam", 150m),
            new Order(6, 1, "Headphones", 200m)
        };
        return orders.ToAsync();
    }

    private static IAsyncEnumerable<Order> CreateEmptyOrders()
    {
        return AsyncEnumerable.Empty<Order>();
    }

    private static IAsyncPagedEnumerable<Customer> CreateEmptyCustomers()
    {
        return new AsyncPagedEnumerable<Customer, Customer>(
            AsyncEnumerable.Empty<Customer>(),
            ct => ValueTask.FromResult(PageContext.Create(0, 0, totalCount: 0)));
    }

    #region Join Tests

    [Fact]
    public async Task JoinPaged_WithMatchingKeys_JoinsCorrectly()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act
        var results = await customers.JoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(6);

        var aliceOrders = results.Where(r => r.CustomerName == "Alice").ToList();
        aliceOrders.Should().HaveCount(3);
        aliceOrders.Select(r => r.Product).Should().BeEquivalentTo(["Laptop", "Mouse", "Headphones"]);

        var bobOrders = results.Where(r => r.CustomerName == "Bob").ToList();
        bobOrders.Should().HaveCount(1);
        bobOrders[0].Product.Should().Be("Keyboard");

        var charlieOrders = results.Where(r => r.CustomerName == "Charlie").ToList();
        charlieOrders.Should().HaveCount(2);
        charlieOrders.Select(r => r.Product).Should().BeEquivalentTo(["Monitor", "Webcam"]);

        // Diana has no orders, so should not appear in results
        results.Where(r => r.CustomerName == "Diana").Should().BeEmpty();
    }

    [Fact]
    public async Task JoinPaged_WithCustomComparer_JoinsCorrectly()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();
        var comparer = EqualityComparer<int>.Default;

        // Act
        var results = await customers.JoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new JoinResult(c.Name, o.Product, o.Amount),
            comparer
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(6);
        results.Select(r => r.CustomerName).Distinct().Should().BeEquivalentTo(["Alice", "Bob", "Charlie"]);
    }

    [Fact]
    public async Task JoinPaged_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateEmptyOrders();

        // Act
        var results = await customers.JoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
        ).ToListPagedAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task JoinPaged_WithEmptyOuter_ReturnsEmpty()
    {
        // Arrange
        var customers = CreateEmptyCustomers();
        var orders = CreateOrders();

        // Act
        var results = await customers.JoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
        ).ToListPagedAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task JoinPaged_WithNullOuterKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await customers.JoinPaged(
                orders,
                null!,
                o => o.CustomerId,
                (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
            ).ToListPagedAsync());
    }

    [Fact]
    public async Task JoinPaged_WithNullInnerKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await customers.JoinPaged(
                orders,
                c => c.Id,
                null!,
                (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
            ).ToListPagedAsync());
    }

    [Fact]
    public async Task JoinPaged_WithNullResultSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await customers.JoinPaged(
                orders,
                c => c.Id,
                o => o.CustomerId,
                (Func<Customer, Order, JoinResult>)null!
            ).ToListPagedAsync());
    }

    [Fact]
    public async Task JoinPaged_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await customers.JoinPaged(
                orders,
                c => c.Id,
                o => o.CustomerId,
                (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
            ).ToListPagedAsync(cts.Token));
    }

    #endregion

    #region GroupJoin Tests

    [Fact]
    public async Task GroupJoinPaged_WithMatchingKeys_GroupsCorrectly()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act
        var results = await customers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new GroupJoinResult(c.Name, orders)
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(4); // All customers should be included

        var alice = results.Single(r => r.CustomerName == "Alice");
        alice.Orders.Should().HaveCount(3);
        alice.Orders.Select(o => o.Product).Should().BeEquivalentTo(["Laptop", "Mouse", "Headphones"]);

        var bob = results.Single(r => r.CustomerName == "Bob");
        bob.Orders.Should().HaveCount(1);
        bob.Orders.Single().Product.Should().Be("Keyboard");

        var charlie = results.Single(r => r.CustomerName == "Charlie");
        charlie.Orders.Should().HaveCount(2);
        charlie.Orders.Select(o => o.Product).Should().BeEquivalentTo(["Monitor", "Webcam"]);

        var diana = results.Single(r => r.CustomerName == "Diana");
        diana.Orders.Should().BeEmpty(); // Diana has no orders
    }

    [Fact]
    public async Task GroupJoinPaged_WithCustomComparer_GroupsCorrectly()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();
        var comparer = EqualityComparer<int>.Default;

        // Act
        var results = await customers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new { Customer = c.Name, OrderCount = orders.Count() },
            comparer
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(4);
        results.Single(r => r.Customer == "Alice").OrderCount.Should().Be(3);
        results.Single(r => r.Customer == "Bob").OrderCount.Should().Be(1);
        results.Single(r => r.Customer == "Charlie").OrderCount.Should().Be(2);
        results.Single(r => r.Customer == "Diana").OrderCount.Should().Be(0);
    }

    [Fact]
    public async Task GroupJoinPaged_WithNoMatches_IncludesAllOuterElements()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateEmptyOrders();

        // Act
        var results = await customers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new GroupJoinResult(c.Name, orders)
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(4); // All customers should be included
        results.All(r => !r.Orders.Any()).Should().BeTrue(); // All should have empty order collections
    }

    [Fact]
    public async Task GroupJoinPaged_WithEmptyOuter_ReturnsEmpty()
    {
        // Arrange
        var customers = CreateEmptyCustomers();
        var orders = CreateOrders();

        // Act
        var results = await customers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new GroupJoinResult(c.Name, orders)
        ).ToListPagedAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GroupJoinPaged_WithNullOuterKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await customers.GroupJoinPaged(
                orders,
                null!,
                o => o.CustomerId,
                (c, orders) => new GroupJoinResult(c.Name, orders)
            ).ToListPagedAsync());
    }

    [Fact]
    public async Task GroupJoinPaged_WithNullInnerKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await customers.GroupJoinPaged(
                orders,
                c => c.Id,
                null!,
                (c, orders) => new GroupJoinResult(c.Name, orders)
            ).ToListPagedAsync());
    }

    [Fact]
    public async Task GroupJoinPaged_WithNullResultSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await customers.GroupJoinPaged(
                orders,
                c => c.Id,
                o => o.CustomerId,
                (Func<Customer, IEnumerable<Order>, GroupJoinResult>)null!
            ).ToListPagedAsync());
    }

    [Fact]
    public async Task GroupJoinPaged_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await customers.GroupJoinPaged(
                orders,
                c => c.Id,
                o => o.CustomerId,
                (c, orders) => new GroupJoinResult(c.Name, orders)
            ).ToListPagedAsync(cts.Token));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task JoinPaged_PreservesPageContext()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();
        var originalPageContext = await customers.GetPageContextAsync();

        // Act
        var joined = customers.JoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new JoinResult(c.Name, o.Product, o.Amount)
        );
        var joinedContext = await joined.GetPageContextAsync();

        // Assert
        joinedContext.PageSize.Should().Be(originalPageContext.PageSize);
        joinedContext.CurrentPage.Should().Be(originalPageContext.CurrentPage);
        joinedContext.TotalCount.Should().Be(originalPageContext.TotalCount);
    }

    [Fact]
    public async Task GroupJoinPaged_PreservesPageContext()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = CreateOrders();
        var originalPageContext = await customers.GetPageContextAsync();

        // Act
        var grouped = customers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new GroupJoinResult(c.Name, orders)
        );
        var groupedContext = await grouped.GetPageContextAsync();

        // Assert
        groupedContext.PageSize.Should().Be(originalPageContext.PageSize);
        groupedContext.CurrentPage.Should().Be(originalPageContext.CurrentPage);
        groupedContext.TotalCount.Should().Be(originalPageContext.TotalCount);
    }

    [Fact]
    public async Task JoinPaged_WithComplexKeys_WorksCorrectly()
    {
        // Arrange
        var customers = CreateCustomers();
        var orders = new[]
        {
            new Order(1, 1, "Laptop", 1200m),
            new Order(2, 2, "Mouse", 25m)
        }.ToAsync();

        // Act
        var results = await customers.JoinPaged(
            orders,
            c => new { c.Id, Dept = c.Department },
            o => new { Id = o.CustomerId, Dept = o.CustomerId == 1 ? "Engineering" : "Sales" },
            (c, o) => new { Customer = c.Name, o.Product }
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.Customer == "Alice" && r.Product == "Laptop");
        results.Should().Contain(r => r.Customer == "Bob" && r.Product == "Mouse");
    }

    [Fact]
    public async Task GroupJoinPaged_WithMultipleMatchesPerKey_GroupsCorrectly()
    {
        // Arrange
        var customers = new[]
        {
            new Customer(1, "Alice", "Engineering")
        }.ToAsync();
        var pagedCustomers = new AsyncPagedEnumerable<Customer, Customer>(
            customers,
            ct => ValueTask.FromResult(PageContext.Create(1, 1, totalCount: 1)));

        var orders = new[]
        {
            new Order(1, 1, "Laptop", 1200m),
            new Order(2, 1, "Mouse", 25m),
            new Order(3, 1, "Keyboard", 80m)
        }.ToAsync();

        // Act
        var results = await pagedCustomers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new { Customer = c.Name, TotalAmount = orders.Sum(o => o.Amount) }
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Customer.Should().Be("Alice");
        results[0].TotalAmount.Should().Be(1305m); // 1200 + 25 + 80
    }

    #endregion

    #region Performance and Edge Cases

    [Fact]
    public async Task JoinPaged_WithLargeDatasets_PerformsEfficiently()
    {
        // Arrange
        var customers = Enumerable.Range(1, 100)
            .Select(i => new Customer(i, $"Customer{i}", "Department"))
            .ToAsync();
        var pagedCustomers = new AsyncPagedEnumerable<Customer, Customer>(
            customers,
            ct => ValueTask.FromResult(PageContext.Create(100, 1, totalCount: 100)));

        var orders = Enumerable.Range(1, 1000)
            .Select(i => new Order(i, (i % 100) + 1, $"Product{i}", i * 10m))
            .ToAsync();

        // Act
        var results = await pagedCustomers.JoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => new { c.Name, o.Product }
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(1000);
        results.Select(r => r.Name).Distinct().Should().HaveCount(100);
    }

    [Fact]
    public async Task GroupJoinPaged_WithDuplicateKeys_GroupsCorrectly()
    {
        // Arrange
        var customers = new[]
        {
            new Customer(1, "Alice", "Engineering"),
            new Customer(1, "Alice2", "Engineering") // Duplicate ID
        }.ToAsync();
        var pagedCustomers = new AsyncPagedEnumerable<Customer, Customer>(
            customers,
            ct => ValueTask.FromResult(PageContext.Create(2, 1, totalCount: 2)));

        var orders = new[]
        {
            new Order(1, 1, "Laptop", 1200m)
        }.ToAsync();

        // Act
        var results = await pagedCustomers.GroupJoinPaged(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orders) => new { Customer = c.Name, OrderCount = orders.Count() }
        ).ToListPagedAsync();

        // Assert
        results.Should().HaveCount(2);
        results.All(r => r.OrderCount == 1).Should().BeTrue(); // Both customers should get the same order
    }

    #endregion
}