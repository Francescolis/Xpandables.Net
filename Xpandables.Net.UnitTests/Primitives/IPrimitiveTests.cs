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

using System.Text.Json;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class IPrimitiveTests
{
    #region Test Primitives

    [PrimitiveJsonConverter<CustomerId, Guid>]
    public readonly partial record struct CustomerId : IPrimitive<CustomerId, Guid>
    {
        public Guid Value { get; }
        private CustomerId(Guid value) => Value = value;
        public override string ToString() => Value.ToString();
        public static CustomerId Create(Guid value) => new(value);
        public static Guid DefaultValue => Guid.Empty;
        public static implicit operator Guid(CustomerId primitive) => primitive.Value;
        public static implicit operator CustomerId(Guid value) => new(value);
    }

    [PrimitiveJsonConverter<ProductName, string>]
    public readonly partial record struct ProductName : IPrimitive<ProductName, string>
    {
        public string Value { get; }
        private ProductName(string value) => Value = value ?? string.Empty;
        public override string ToString() => Value;
        public static ProductName Create(string value) => new(value);
        public static string DefaultValue => string.Empty;
        public static implicit operator string(ProductName primitive) => primitive.Value;
        public static implicit operator ProductName(string value) => new(value);
    }

    [PrimitiveJsonConverter<Price, decimal>]
    public readonly partial record struct Price : IPrimitive<Price, decimal>
    {
        public decimal Value { get; }
        private Price(decimal value) => Value = value;
        public override string ToString() => Value.ToString();
        public static Price Create(decimal value) => new(value);
        public static decimal DefaultValue => 0m;
        public static implicit operator decimal(Price primitive) => primitive.Value;
        public static implicit operator Price(decimal value) => new(value);
    }

    [PrimitiveJsonConverter<Quantity, int>]
    public readonly partial record struct Quantity : IPrimitive<Quantity, int>
    {
        public int Value { get; }
        private Quantity(int value) => Value = value;
        public override string ToString() => Value.ToString();
        public static Quantity Create(int value) => new(value);
        public static int DefaultValue => 0;
        public static implicit operator int(Quantity primitive) => primitive.Value;
        public static implicit operator Quantity(int value) => new(value);
    }

    [PrimitiveJsonConverter<CreatedDate, DateTime>]
    public readonly partial record struct CreatedDate : IPrimitive<CreatedDate, DateTime>
    {
        public DateTime Value { get; }
        private CreatedDate(DateTime value) => Value = value;
        public override string ToString() => Value.ToString();
        public static CreatedDate Create(DateTime value) => new(value);
        public static DateTime DefaultValue => DateTime.MinValue;
        public static implicit operator DateTime(CreatedDate primitive) => primitive.Value;
        public static implicit operator CreatedDate(DateTime value) => new(value);
    }

    #endregion

    #region Creation Tests

    [Fact]
    public void WhenCreatingPrimitiveWithGuidThenValueShouldBeSet()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var customerId = CustomerId.Create(expectedId);

        // Assert
        customerId.Value.Should().Be(expectedId);
    }

    [Fact]
    public void WhenCreatingPrimitiveWithStringThenValueShouldBeSet()
    {
        // Arrange
        const string expectedName = "Premium Widget";

        // Act
        var productName = ProductName.Create(expectedName);

        // Assert
        productName.Value.Should().Be(expectedName);
    }

    [Fact]
    public void WhenCreatingPrimitiveWithDecimalThenValueShouldBeSet()
    {
        // Arrange
        const decimal expectedPrice = 99.99m;

        // Act
        var price = Price.Create(expectedPrice);

        // Assert
        price.Value.Should().Be(expectedPrice);
    }

    [Fact]
    public void WhenCreatingPrimitiveWithIntThenValueShouldBeSet()
    {
        // Arrange
        const int expectedQuantity = 42;

        // Act
        var quantity = Quantity.Create(expectedQuantity);

        // Assert
        quantity.Value.Should().Be(expectedQuantity);
    }

    [Fact]
    public void WhenCreatingPrimitiveWithDateTimeThenValueShouldBeSet()
    {
        // Arrange
        var expectedDate = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var createdDate = CreatedDate.Create(expectedDate);

        // Assert
        createdDate.Value.Should().Be(expectedDate);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void WhenAccessingDefaultValueForGuidThenShouldReturnGuidEmpty()
    {
        CustomerId.DefaultValue.Should().Be(Guid.Empty);
    }

    [Fact]
    public void WhenAccessingDefaultValueForStringThenShouldReturnEmptyString()
    {
        ProductName.DefaultValue.Should().Be(string.Empty);
    }

    [Fact]
    public void WhenAccessingDefaultValueForDecimalThenShouldReturnZero()
    {
        Price.DefaultValue.Should().Be(0m);
    }

    [Fact]
    public void WhenAccessingDefaultValueForIntThenShouldReturnZero()
    {
        Quantity.DefaultValue.Should().Be(0);
    }

    [Fact]
    public void WhenCreatingWithDefaultValueThenShouldHaveDefaultValue()
    {
        // Act
        var customerId = CustomerId.Create(Guid.Empty);
        var productName = ProductName.Create(string.Empty);
        var price = Price.Create(0m);

        // Assert
        customerId.Value.Should().Be(Guid.Empty);
        productName.Value.Should().Be(string.Empty);
        price.Value.Should().Be(0m);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void WhenImplicitlyConvertingPrimitiveToValueThenShouldReturnValue()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var customerId = CustomerId.Create(expectedId);

        // Act
        Guid actualValue = customerId;

        // Assert
        actualValue.Should().Be(expectedId);
    }

    [Fact]
    public void WhenImplicitlyConvertingValueToPrimitiveThenShouldCreatePrimitive()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        CustomerId customerId = expectedId;

        // Assert
        customerId.Value.Should().Be(expectedId);
    }

    [Fact]
    public void WhenImplicitlyConvertingStringPrimitiveThenShouldWork()
    {
        // Arrange
        const string name = "Test Product";

        // Act
        ProductName productName = name;
        string actualName = productName;

        // Assert
        actualName.Should().Be(name);
    }

    [Fact]
    public void WhenImplicitlyConvertingDecimalPrimitiveThenShouldWork()
    {
        // Arrange
        const decimal expectedPrice = 199.99m;

        // Act
        Price price = expectedPrice;
        decimal actualPrice = price;

        // Assert
        actualPrice.Should().Be(expectedPrice);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void WhenComparingEqualPrimitivesThenShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId1 = CustomerId.Create(id);
        var customerId2 = CustomerId.Create(id);

        // Assert
        customerId1.Should().Be(customerId2);
        customerId1.Equals(customerId2).Should().BeTrue();
    }

    [Fact]
    public void WhenComparingDifferentPrimitivesThenShouldNotBeEqual()
    {
        // Arrange
        var customerId1 = CustomerId.Create(Guid.NewGuid());
        var customerId2 = CustomerId.Create(Guid.NewGuid());

        // Assert
        customerId1.Should().NotBe(customerId2);
        customerId1.Equals(customerId2).Should().BeFalse();
    }

    [Fact]
    public void WhenComparingStringPrimitivesWithSameValueThenShouldBeEqual()
    {
        // Arrange
        var name1 = ProductName.Create("Widget");
        var name2 = ProductName.Create("Widget");

        // Assert
        name1.Should().Be(name2);
        name1.Equals(name2).Should().BeTrue();
    }

    [Fact]
    public void WhenGettingHashCodeForEqualPrimitivesThenShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId1 = CustomerId.Create(id);
        var customerId2 = CustomerId.Create(id);

        // Assert
        customerId1.GetHashCode().Should().Be(customerId2.GetHashCode());
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void WhenComparingPrimitivesThenShouldReturnCorrectOrder()
    {
        // Arrange
        var price1 = Price.Create(10.00m);
        var price2 = Price.Create(20.00m);
        var price3 = Price.Create(10.00m);

        // Assert
        ((IComparable<Price>)price1).CompareTo(price2).Should().BeNegative();
        ((IComparable<Price>)price2).CompareTo(price1).Should().BePositive();
        ((IComparable<Price>)price1).CompareTo(price3).Should().Be(0);
    }

    [Fact]
    public void WhenComparingQuantitiesThenShouldReturnCorrectOrder()
    {
        // Arrange
        var qty1 = Quantity.Create(5);
        var qty2 = Quantity.Create(10);

        // Assert
        ((IComparable<Quantity>)qty1).CompareTo(qty2).Should().BeNegative();
        ((IComparable<Quantity>)qty2).CompareTo(qty1).Should().BePositive();
    }

    [Fact]
    public void WhenComparingDatesThenShouldReturnCorrectOrder()
    {
        // Arrange
        var date1 = CreatedDate.Create(new DateTime(2025, 1, 1));
        var date2 = CreatedDate.Create(new DateTime(2025, 12, 31));

        // Assert
        ((IComparable<CreatedDate>)date1).CompareTo(date2).Should().BeNegative();
        ((IComparable<CreatedDate>)date2).CompareTo(date1).Should().BePositive();
    }

    [Fact]
    public void WhenSortingPrimitiveCollectionThenShouldBeSorted()
    {
        // Arrange
        var prices = new[]
        {
            Price.Create(50.00m),
            Price.Create(10.00m),
            Price.Create(30.00m),
            Price.Create(20.00m)
        };

        // Act
        var sortedPrices = prices.OrderBy(p => p.Value).ToArray();

        // Assert
        sortedPrices[0].Value.Should().Be(10.00m);
        sortedPrices[1].Value.Should().Be(20.00m);
        sortedPrices[2].Value.Should().Be(30.00m);
        sortedPrices[3].Value.Should().Be(50.00m);
    }

    #endregion

    #region Formattable Tests

    [Fact]
    public void WhenFormattingPriceThenShouldUseFormatProvider()
    {
        // Arrange
        var price = Price.Create(1234.56m);

        // Act
        var formatted = ((IFormattable)price).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

        // Assert
        formatted.Should().Contain("1,234.56");
    }

    [Fact]
    public void WhenFormattingDateThenShouldUseFormat()
    {
        // Arrange
        var date = CreatedDate.Create(new DateTime(2025, 6, 15));

        // Act
        var formatted = ((IFormattable)date).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        formatted.Should().Be("2025-06-15");
    }

    [Fact]
    public void WhenCallingToStringThenShouldReturnValueString()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = CustomerId.Create(id);

        // Act
        var result = customerId.ToString();

        // Assert
        result.Should().Be(id.ToString());
    }

    #endregion

    #region JSON Serialization Tests

    [Fact]
    public void WhenSerializingPrimitiveToJsonThenShouldSerializeAsValue()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.Parse("12345678-1234-1234-1234-123456789012"));

        // Act
        var json = JsonSerializer.Serialize(customerId);

        // Assert
        json.Should().Be("\"12345678-1234-1234-1234-123456789012\"");
    }

    [Fact]
    public void WhenDeserializingJsonToPrimitiveThenShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = "\"12345678-1234-1234-1234-123456789012\"";

        // Act
        var customerId = JsonSerializer.Deserialize<CustomerId>(json);

        // Assert
        customerId.Value.Should().Be(Guid.Parse("12345678-1234-1234-1234-123456789012"));
    }

    [Fact]
    public void WhenSerializingStringPrimitiveThenShouldSerializeAsString()
    {
        // Arrange
        var productName = ProductName.Create("Test Widget");

        // Act
        var json = JsonSerializer.Serialize(productName);

        // Assert
        json.Should().Be("\"Test Widget\"");
    }

    [Fact]
    public void WhenDeserializingStringPrimitiveThenShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = "\"Premium Widget\"";

        // Act
        var productName = JsonSerializer.Deserialize<ProductName>(json);

        // Assert
        productName.Value.Should().Be("Premium Widget");
    }

    [Fact]
    public void WhenSerializingDecimalPrimitiveThenShouldSerializeAsNumber()
    {
        // Arrange
        var price = Price.Create(99.99m);

        // Act
        var json = JsonSerializer.Serialize(price);

        // Assert
        json.Should().Be("99.99");
    }

    [Fact]
    public void WhenDeserializingDecimalPrimitiveThenShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = "149.95";

        // Act
        var price = JsonSerializer.Deserialize<Price>(json);

        // Assert
        price.Value.Should().Be(149.95m);
    }

    [Fact]
    public void WhenSerializingIntPrimitiveThenShouldSerializeAsNumber()
    {
        // Arrange
        var quantity = Quantity.Create(42);

        // Act
        var json = JsonSerializer.Serialize(quantity);

        // Assert
        json.Should().Be("42");
    }

    [Fact]
    public void WhenRoundTrippingPrimitiveThroughJsonThenShouldBeEqual()
    {
        // Arrange
        var originalId = CustomerId.Create(Guid.NewGuid());
        var originalName = ProductName.Create("Test Product");
        var originalPrice = Price.Create(199.99m);

        // Act
        var idJson = JsonSerializer.Serialize(originalId);
        var nameJson = JsonSerializer.Serialize(originalName);
        var priceJson = JsonSerializer.Serialize(originalPrice);

        var deserializedId = JsonSerializer.Deserialize<CustomerId>(idJson);
        var deserializedName = JsonSerializer.Deserialize<ProductName>(nameJson);
        var deserializedPrice = JsonSerializer.Deserialize<Price>(priceJson);

        // Assert
        deserializedId.Should().Be(originalId);
        deserializedName.Should().Be(originalName);
        deserializedPrice.Should().Be(originalPrice);
    }

    [Fact]
    public void WhenSerializingObjectWithPrimitivePropertiesThenShouldSerializeCorrectly()
    {
        // Arrange
        var order = new TestOrder
        {
            Id = CustomerId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111")),
            ProductName = ProductName.Create("Widget"),
            Price = Price.Create(25.50m),
            Quantity = Quantity.Create(3)
        };

        // Act
        var json = JsonSerializer.Serialize(order);
        var deserialized = JsonSerializer.Deserialize<TestOrder>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(order.Id);
        deserialized.ProductName.Should().Be(order.ProductName);
        deserialized.Price.Should().Be(order.Price);
        deserialized.Quantity.Should().Be(order.Quantity);
    }

    private sealed class TestOrder
    {
        public CustomerId Id { get; set; }
        public ProductName ProductName { get; set; }
        public Price Price { get; set; }
        public Quantity Quantity { get; set; }
    }

    #endregion

    #region Value Equality Tests

    [Fact]
    public void WhenPrimitiveHasDefaultValueThenValueShouldEqualDefaultValue()
    {
        // Arrange
        var emptyCustomerId = CustomerId.Create(Guid.Empty);
        var emptyProductName = ProductName.Create(string.Empty);
        var emptyPrice = Price.Create(0m);

        // Assert
        emptyCustomerId.Value.Should().Be(CustomerId.DefaultValue);
        emptyProductName.Value.Should().Be(ProductName.DefaultValue);
        emptyPrice.Value.Should().Be(Price.DefaultValue);
    }

    [Fact]
    public void WhenPrimitiveHasNonDefaultValueThenValueShouldNotEqualDefaultValue()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid());
        var productName = ProductName.Create("Widget");
        var price = Price.Create(10.00m);

        // Assert
        customerId.Value.Should().NotBe(CustomerId.DefaultValue);
        productName.Value.Should().NotBe(ProductName.DefaultValue);
        price.Value.Should().NotBe(Price.DefaultValue);
    }

    [Fact]
    public void WhenCreatedWithDefaultValuesThenShouldMatchDefaults()
    {
        // Act
        var customerId = CustomerId.Create(CustomerId.DefaultValue);
        var productName = ProductName.Create(ProductName.DefaultValue);

        // Assert
        customerId.Value.Should().Be(Guid.Empty);
        productName.Value.Should().Be(string.Empty);
    }

    #endregion

    #region IComparable (Non-Generic) Tests

    [Fact]
    public void WhenComparingToObjectThenShouldReturnCorrectResult()
    {
        // Arrange
        var price1 = Price.Create(10.00m);
        var price2 = Price.Create(20.00m);

        // Act
        var result = ((IComparable)price1).CompareTo(price2);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void WhenComparingToInvalidObjectThenShouldThrowArgumentException()
    {
        // Arrange
        var price = Price.Create(10.00m);

        // Act
        var act = () => ((IComparable)price).CompareTo("invalid");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
