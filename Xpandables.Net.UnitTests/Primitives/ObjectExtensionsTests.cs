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

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class ObjectExtensionsTests
{
    #region IsNull Tests

    [Fact]
    public void WhenObjectIsNullThenIsNullShouldBeTrue()
    {
        // Arrange
        object? obj = null;

        // Assert
        obj.IsNull.Should().BeTrue();
    }

    [Fact]
    public void WhenObjectIsNotNullThenIsNullShouldBeFalse()
    {
        // Arrange
        object obj = "test";

        // Assert
        obj.IsNull.Should().BeFalse();
    }

    [Fact]
    public void WhenReferenceTypeIsNullThenIsNullShouldBeTrue()
    {
        // Arrange
        string? str = null;

        // Assert
        ((object?)str).IsNull.Should().BeTrue();
    }

    [Fact]
    public void WhenValueTypeBoxedThenIsNullShouldBeFalse()
    {
        // Arrange
        object obj = 42;

        // Assert
        obj.IsNull.Should().BeFalse();
    }

    #endregion

    #region ChangeTypeNullable Tests

    [Fact]
    public void WhenConvertingToNullableIntThenShouldReturnValue()
    {
        // Arrange
        object obj = "42";

        // Act
        var result = obj.ChangeTypeNullable<int?>();

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void WhenConvertingNullToNullableTypeThenShouldReturnNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = obj.ChangeTypeNullable<int?>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WhenConvertingStringToIntThenShouldConvert()
    {
        // Arrange
        object obj = "123";

        // Act
        var result = obj.ChangeTypeNullable<int>();

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void WhenConvertingDoubleToDecimalThenShouldConvert()
    {
        // Arrange
        object obj = 123.45;

        // Act
        var result = obj.ChangeTypeNullable<decimal>();

        // Assert
        result.Should().Be(123.45m);
    }

    [Fact]
    public void WhenConvertingWithCultureInfoThenShouldUseCulture()
    {
        // Arrange
        object obj = "1,234.56";
        var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

        // Act
        var result = obj.ChangeTypeNullable<decimal>(culture);

        // Assert
        result.Should().Be(1234.56m);
    }

    [Fact]
    public void WhenConvertingToNullableWithNullTypeThenShouldThrowArgumentNullException()
    {
        // Arrange
        object obj = "test";

        // Act
        var act = () => obj.ChangeTypeNullable(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenConvertingToNullableDateTimeThenShouldConvert()
    {
        // Arrange
        object obj = "2025-06-15";

        // Act
        var result = obj.ChangeTypeNullable<DateTime?>();

        // Assert
        result.Should().Be(new DateTime(2025, 6, 15));
    }

    [Fact]
    public void WhenConvertingIntToNullableIntThenShouldReturnValue()
    {
        // Arrange
        object obj = 42;

        // Act
        var result = obj.ChangeTypeNullable<int?>();

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void WhenConvertingToNonNullableTypeThenShouldConvert()
    {
        // Arrange
        object obj = "true";

        // Act
        var result = obj.ChangeTypeNullable<bool>();

        // Assert
        result.Should().Be(true);
    }

    #endregion

    #region As<T> Tests

    [Fact]
    public void WhenCastingToCorrectTypeThenShouldReturnInstance()
    {
        // Arrange
        object obj = "test string";

        // Act
        var result = ObjectExtensions.As<string>(obj);

        // Assert
        result.Should().Be("test string");
    }

    [Fact]
    public void WhenCastingToIncorrectTypeThenShouldReturnNull()
    {
        // Arrange
        object obj = "test string";

        // Act
        var result = ObjectExtensions.As<List<int>>(obj);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WhenCastingNullThenShouldReturnNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = ObjectExtensions.As<string>(obj);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WhenCastingToBaseTypeThenShouldSucceed()
    {
        // Arrange
        object obj = new DerivedClass { BaseProperty = "base", DerivedProperty = "derived" };

        // Act
        var result = ObjectExtensions.As<BaseClass>(obj);

        // Assert
        result.Should().NotBeNull();
        result!.BaseProperty.Should().Be("base");
    }

    [Fact]
    public void WhenCastingToInterfaceThenShouldSucceed()
    {
        // Arrange
        object obj = new ImplementingClass { Name = "test" };

        // Act
        var result = ObjectExtensions.As<ITestInterface>(obj);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("test");
    }

    [Fact]
    public void WhenCastingWithTemplateThenShouldReturnCorrectType()
    {
        // Arrange
        object obj = new DerivedClass { BaseProperty = "base", DerivedProperty = "derived" };
        DerivedClass? template = null;

        // Act
        var result = obj.As(template);

        // Assert
        result.Should().NotBeNull();
        result!.DerivedProperty.Should().Be("derived");
    }

    private class BaseClass
    {
        public string? BaseProperty { get; set; }
    }

    private sealed class DerivedClass : BaseClass
    {
        public string? DerivedProperty { get; set; }
    }

    private interface ITestInterface
    {
        string Name { get; }
    }

    private sealed class ImplementingClass : ITestInterface
    {
        public required string Name { get; set; }
    }

    #endregion

    #region AsRequired<T> Tests

    [Fact]
    public void WhenCastingWithAsRequiredToCorrectTypeThenShouldReturnInstance()
    {
        // Arrange
        object obj = "test string";

        // Act
        var result = obj.AsRequired<string>();

        // Assert
        result.Should().Be("test string");
    }

    [Fact]
    public void WhenCastingNullWithAsRequiredThenShouldThrowArgumentNullException()
    {
        // Arrange
        object? obj = null;

        // Act
        var act = () => obj.AsRequired<string>();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenCastingToIncorrectTypeWithAsRequiredThenShouldThrowInvalidCastException()
    {
        // Arrange
        object obj = "test string";

        // Act
        var act = () => obj.AsRequired<List<int>>();

        // Assert
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void WhenCastingToBaseTypeWithAsRequiredThenShouldSucceed()
    {
        // Arrange
        object obj = new DerivedClass { BaseProperty = "base" };

        // Act
        var result = obj.AsRequired<BaseClass>();

        // Assert
        result.Should().NotBeNull();
        result.BaseProperty.Should().Be("base");
    }

    #endregion

    #region When Action Tests

    [Fact]
    public void WhenConditionIsTrueThenActionShouldBeExecuted()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        var executed = false;

        // Act
        var result = list.When(true, l =>
        {
            l.Add(4);
            executed = true;
        });

        // Assert
        executed.Should().BeTrue();
        result.Should().Contain(4);
    }

    [Fact]
    public void WhenConditionIsFalseThenActionShouldNotBeExecuted()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        var executed = false;

        // Act
        var result = list.When(false, l =>
        {
            l.Add(4);
            executed = true;
        });

        // Assert
        executed.Should().BeFalse();
        result.Should().NotContain(4);
    }

    [Fact]
    public void WhenConditionIsTrueThenShouldReturnOriginalObject()
    {
        // Arrange
        var original = new StringBuilder("test");

        // Act
        var result = original.When(true, sb => sb.Append(" modified"));

        // Assert
        result.Should().BeSameAs(original);
        result.ToString().Should().Be("test modified");
    }

    [Fact]
    public void WhenActionIsNullThenShouldThrowArgumentNullException()
    {
        // Arrange
        var obj = new object();

        // Act
        var act = () => obj.When(true, (Action<object>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenObjectIsNullThenShouldThrowArgumentNullException()
    {
        // Arrange
        string? obj = null;

        // Act
        var act = () => obj!.When(true, _ => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region When Func Tests

    [Fact]
    public void WhenConditionIsTrueThenFuncShouldBeExecuted()
    {
        // Arrange
        const string original = "test";

        // Act
        var result = original.When(true, s => s.ToUpperInvariant());

        // Assert
        result.Should().Be("TEST");
    }

    [Fact]
    public void WhenConditionIsFalseThenFuncShouldNotBeExecutedAndReturnOriginal()
    {
        // Arrange
        const string original = "test";

        // Act
        var result = original.When(false, s => s.ToUpperInvariant());

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void WhenFuncIsNullThenShouldThrowArgumentNullException()
    {
        // Arrange
        var obj = "test";

        // Act
        var act = () => obj.When(true, (Func<string, string>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenChainingMultipleWhenCallsThenShouldApplyAll()
    {
        // Arrange
        const int original = 10;

        // Act
        var result = original
            .When(true, x => x * 2)
            .When(true, x => x + 5)
            .When(false, x => x * 100);

        // Assert
        result.Should().Be(25);
    }

    [Fact]
    public void WhenTransformingObjectThenShouldReturnTransformedResult()
    {
        // Arrange
        var order = new TestOrder { Id = 1, Total = 100m };

        // Act
        var result = order.When(order.Total > 50,
            o => o with { Total = o.Total * 0.9m });

        // Assert
        result.Total.Should().Be(90m);
    }

    private sealed record TestOrder
    {
        public int Id { get; init; }
        public decimal Total { get; init; }
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenProcessingApiResponseThenShouldHandleTypeConversions()
    {
        // Arrange - Simulating an API response with dynamic data
        object? response = new Dictionary<string, object>
        {
            ["id"] = "123",
            ["amount"] = "99.99",
            ["active"] = "true"
        };

        var dict = ObjectExtensions.As<Dictionary<string, object>>(response);

        // Act
        var id = dict!["id"].ChangeTypeNullable<int>();
        var amount = dict["amount"].ChangeTypeNullable<decimal>();
        var active = dict["active"].ChangeTypeNullable<bool>();

        // Assert
        id.Should().Be(123);
        amount.Should().Be(99.99m);
        active.Should().BeTrue();
    }

    [Fact]
    public void WhenBuildingFluentQueryThenWhenShouldAddConditionalClauses()
    {
        // Arrange
        var query = new QueryBuilder();
        var includeInactive = true;
        var minPrice = 100m;
        decimal? maxPrice = null;

        // Act
        var result = query
            .When(includeInactive, q => q.AddFilter("active", "false"))
            .When(minPrice > 0, q => q.AddFilter("minPrice", minPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            .When(maxPrice.HasValue, q => q.AddFilter("maxPrice", maxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? ""));

        // Assert
        result.Filters.Should().Contain(("active", "false"));
        result.Filters.Should().Contain(("minPrice", "100"));
        result.Filters.Should().NotContain(f => f.Key == "maxPrice");
    }

    private sealed class QueryBuilder
    {
        public List<(string Key, string Value)> Filters { get; } = [];

        public QueryBuilder AddFilter(string key, string value)
        {
            Filters.Add((key, value));
            return this;
        }
    }

    [Fact]
    public void WhenHandlingUnknownObjectTypeThenAsShouldReturnNull()
    {
        // Arrange
        object unknownData = new { Name = "Test", Value = 42 };

        // Act
        var asCustomer = ObjectExtensions.As<Customer>(unknownData);
        var asOrder = ObjectExtensions.As<Order>(unknownData);

        // Assert
        asCustomer.Should().BeNull();
        asOrder.Should().BeNull();
    }

    private sealed class Customer
    {
        public string? Name { get; set; }
    }

    private sealed class Order
    {
        public int Id { get; set; }
    }

    [Fact]
    public void WhenConvertingDatabaseValuesThenShouldHandleNullables()
    {
        // Arrange - Simulating database row values
        object?[] rowValues = ["John Doe", 42, DBNull.Value, "2025-06-15"];

        // Act
        var name = rowValues[0]?.ChangeTypeNullable<string>();
        var age = rowValues[1]?.ChangeTypeNullable<int?>();
        var middleName = rowValues[2] == DBNull.Value ? null : rowValues[2]?.ChangeTypeNullable<string>();
        var date = rowValues[3]?.ChangeTypeNullable<DateTime?>();

        // Assert
        name.Should().Be("John Doe");
        age.Should().Be(42);
        middleName.Should().BeNull();
        date.Should().Be(new DateTime(2025, 6, 15));
    }

    [Fact]
    public void WhenApplyingDiscountsConditionallyThenWhenShouldWork()
    {
        // Arrange
        var cart = new ShoppingCart { Subtotal = 150m };
        const bool isPremiumMember = true;
        const bool hasPromoCode = false;

        // Act
        var finalCart = cart
            .When(isPremiumMember, c => c with { Discount = c.Subtotal * 0.1m })
            .When(hasPromoCode, c => c with { Discount = c.Discount + 20m });

        // Assert
        finalCart.Discount.Should().Be(15m);
        finalCart.Total.Should().Be(135m);
    }

    private sealed record ShoppingCart
    {
        public decimal Subtotal { get; init; }
        public decimal Discount { get; init; }
        public decimal Total => Subtotal - Discount;
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WhenConvertingEmptyStringToNullableIntThenShouldThrow()
    {
        // Arrange
        object obj = "";

        // Act
        var act = () => obj.ChangeTypeNullable<int?>();

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void WhenConvertingValidGuidStringThenShouldConvert()
    {
        // Arrange
        var guidString = Guid.NewGuid().ToString();
        object obj = guidString;

        // Act
        var result = obj.ChangeTypeNullable<Guid?>();

        // Assert
        result.Should().Be(Guid.Parse(guidString));
    }

    [Fact]
    public void WhenChainingSameObjectThenShouldMaintainReference()
    {
        // Arrange
        var original = new StringBuilder("start");

        // Act
        var result = original
            .When<StringBuilder>(true, sb => { sb.Append("-mid"); })
            .When<StringBuilder>(true, sb => { sb.Append("-end"); });

        // Assert
        result.Should().BeSameAs(original);
        result.ToString().Should().Be("start-mid-end");
    }

    #endregion
}
