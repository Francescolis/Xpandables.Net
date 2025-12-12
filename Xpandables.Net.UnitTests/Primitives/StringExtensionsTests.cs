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
using System.Globalization;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class StringExtensionsTests
{
    #region StringFormat Tests

    [Fact]
    public void WhenFormattingStringWithArgumentsThenShouldFormatCorrectly()
    {
        // Arrange
        const string template = "Hello, {0}! You have {1} messages.";

        // Act
        var result = template.StringFormat("John", 5);

        // Assert
        result.Should().Be("Hello, John! You have 5 messages.");
    }

    [Fact]
    public void WhenFormattingStringWithCultureThenShouldUseCulture()
    {
        // Arrange
        const string template = "Price: {0:C}";
        var usCulture = CultureInfo.GetCultureInfo("en-US");
        var germanCulture = CultureInfo.GetCultureInfo("de-DE");

        // Act
        var usResult = template.StringFormat(usCulture, 1234.56m);
        var germanResult = template.StringFormat(germanCulture, 1234.56m);

        // Assert
        usResult.Should().Contain("$");
        usResult.Should().Contain("1,234.56");
        germanResult.Should().Contain("€");
    }

    [Fact]
    public void WhenFormattingStringWithDateThenShouldFormatWithCulture()
    {
        // Arrange
        const string template = "Date: {0:d}";
        var date = new DateTime(2025, 6, 15);
        var usCulture = CultureInfo.GetCultureInfo("en-US");

        // Act
        var result = template.StringFormat(usCulture, date);

        // Assert
        result.Should().Contain("6/15/2025");
    }

    [Fact]
    public void WhenFormattingNullStringThenShouldThrowArgumentNullException()
    {
        // Arrange
        string? template = null;

        // Act
        var act = () => template!.StringFormat("arg");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenFormattingWithNullCultureThenShouldThrowArgumentNullException()
    {
        // Arrange
        const string template = "Test {0}";

        // Act
        var act = () => template.StringFormat(null!, "arg");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenFormattingWithMultipleArgumentsThenShouldFormatAll()
    {
        // Arrange
        const string template = "{0} + {1} = {2}";

        // Act
        var result = template.StringFormat(1, 2, 3);

        // Assert
        result.Should().Be("1 + 2 = 3");
    }

    [Fact]
    public void WhenFormattingWithInvariantCultureThenShouldUseInvariantFormat()
    {
        // Arrange
        const string template = "Value: {0:F2}";

        // Act
        var result = template.StringFormat(1234.5678);

        // Assert
        result.Should().Be("Value: 1234.57");
    }

    #endregion

    #region SplitTypeName Tests

    [Fact]
    public void WhenSplittingPascalCaseTypeNameThenShouldSeparateWords()
    {
        // Arrange
        const string typeName = "CustomerOrderService";

        // Act
        var result = typeName.SplitTypeName();

        // Assert
        result.Should().Be("Customer Order Service");
    }

    [Fact]
    public void WhenSplittingCamelCaseNameThenShouldSeparateWords()
    {
        // Arrange
        const string typeName = "customerOrderService";

        // Act
        var result = typeName.SplitTypeName();

        // Assert
        result.Should().Be("customer Order Service");
    }

    [Fact]
    public void WhenSplittingNameWithAcronymThenShouldKeepAcronymTogether()
    {
        // Arrange
        const string typeName = "XMLHttpRequest";

        // Act
        var result = typeName.SplitTypeName();

        // Assert
        result.Should().Be("XML Http Request");
    }

    [Fact]
    public void WhenSplittingNameWithNumbersThenShouldHandleDigits()
    {
        // Arrange
        const string typeName = "Order2024Handler";

        // Act
        var result = typeName.SplitTypeName();

        // Assert
        result.Should().Contain("Order2024");
    }

    [Fact]
    public void WhenSplittingSimpleNameThenShouldReturnSameWord()
    {
        // Arrange
        const string typeName = "Customer";

        // Act
        var result = typeName.SplitTypeName();

        // Assert
        result.Should().Be("Customer");
    }

    [Fact]
    public void WhenSplittingNameWithConsecutiveUppercaseThenShouldHandleCorrectly()
    {
        // Arrange
        const string typeName = "HTTPSConnection";

        // Act
        var result = typeName.SplitTypeName();

        // Assert
        result.Should().Contain("HTTPS");
    }

    [Fact]
    public void WhenSplittingNullTypeNameThenShouldThrowArgumentNullException()
    {
        // Arrange
        string? typeName = null;

        // Act
        var act = () => typeName!.SplitTypeName();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("GetById", "Get By Id")]
    [InlineData("FindAllCustomers", "Find All Customers")]
    [InlineData("SaveToDatabase", "Save To Database")]
    [InlineData("ID", "ID")]
    [InlineData("APIController", "API Controller")]
    public void WhenSplittingVariousTypeNamesThenShouldSplitCorrectly(string input, string expected)
    {
        // Act
        var result = input.SplitTypeName();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region StringJoin Tests

    [Fact]
    public void WhenJoiningCollectionWithCharSeparatorThenShouldJoinCorrectly()
    {
        // Arrange
        var items = new[] { "apple", "banana", "cherry" };

        // Act
        var result = string.Join(',', items);

        // Assert
        result.Should().Be("apple,banana,cherry");
    }

    [Fact]
    public void WhenJoiningCollectionWithStringSeparatorThenShouldJoinCorrectly()
    {
        // Arrange
        var items = new[] { "one", "two", "three" };

        // Act
        var result = string.Join(" - ", items);

        // Assert
        result.Should().Be("one - two - three");
    }

    [Fact]
    public void WhenJoiningEmptyCollectionThenShouldReturnEmptyString()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var result = string.Join(',', items);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenJoiningSingleItemThenShouldReturnItem()
    {
        // Arrange
        var items = new[] { "only" };

        // Act
        var result = string.Join(',', items);

        // Assert
        result.Should().Be("only");
    }

    [Fact]
    public void WhenJoiningIntegersThenShouldConvertAndJoin()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = string.Join(", ", numbers);

        // Assert
        result.Should().Be("1, 2, 3, 4, 5");
    }

    [Fact]
    public void WhenJoiningWithNewLineSeparatorThenShouldJoinWithNewLines()
    {
        // Arrange
        var lines = new[] { "Line 1", "Line 2", "Line 3" };

        // Act
        var result = string.Join(Environment.NewLine, lines);

        // Assert
        result.Should().Contain("Line 1");
        result.Should().Contain("Line 2");
        result.Should().Contain("Line 3");
    }

    [Fact]
    public void WhenJoiningCustomObjectsThenShouldUseToString()
    {
        // Arrange
        var items = new[]
        {
            new TestItem("Item1", 10),
            new TestItem("Item2", 20)
        };

        // Act
        var result = string.Join(" | ", items.Select(i => i.ToString()));

        // Assert
        result.Should().Be("Item1:10 | Item2:20");
    }

    private sealed record TestItem(string Name, int Value)
    {
        public override string ToString() => $"{Name}:{Value}";
    }

    #endregion

    #region ToElementCollection Tests

    [Fact]
    public void WhenConvertingValidJsonToElementCollectionThenShouldDeserialize()
    {
        // Arrange
        const string json = """[{"key":"name","values":["John"]}]""";

        // Act
        var result = json.ToElementCollection();

        // Assert
        result.Should().NotBeNull();
        result.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void WhenConvertingJsonWithMultipleEntriesToElementCollectionThenShouldDeserializeAll()
    {
        // Arrange
        const string json = """
            [
                {"key":"name","values":["John"]},
                {"key":"email","values":["john@example.com"]}
            ]
            """;

        // Act
        var result = json.ToElementCollection();

        // Assert
        result.Count.Should().Be(2);
        result.ContainsKey("name").Should().BeTrue();
        result.ContainsKey("email").Should().BeTrue();
    }

    [Fact]
    public void WhenConvertingEmptyArrayToElementCollectionThenShouldReturnEmpty()
    {
        // Arrange
        const string json = "[]";

        // Act
        var result = json.ToElementCollection();

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region DeserializeAnonymousType Tests

    [Fact]
    public void WhenDeserializingAnonymousTypeThenShouldReturnCorrectObject()
    {
        // Arrange
        const string json = """{"name":"John","age":30}""";
        var template = new { name = "", age = 0 };

        // Act
        var result = json.DeserializeAnonymousType(template);

        // Assert
        result.Should().NotBeNull();
        result!.name.Should().Be("John");
        result.age.Should().Be(30);
    }

    [Fact]
    public void WhenDeserializingComplexAnonymousTypeThenShouldReturnCorrectObject()
    {
        // Arrange
        const string json = """{"user":{"id":1,"name":"Jane"},"active":true}""";
        var template = new
        {
            user = new { id = 0, name = "" },
            active = false
        };

        // Act
        var result = json.DeserializeAnonymousType(template);

        // Assert
        result.Should().NotBeNull();
        result!.user.id.Should().Be(1);
        result.user.name.Should().Be("Jane");
        result.active.Should().BeTrue();
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenFormattingLogMessageThenShouldProduceReadableOutput()
    {
        // Arrange
        const string template = "[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}: {3}";
        var timestamp = new DateTime(2025, 6, 15, 14, 30, 0);
        const string level = "INFO";
        const string source = "OrderService";
        const string message = "Order processed successfully";

        // Act
        var result = template.StringFormat(CultureInfo.InvariantCulture, timestamp, level, source, message);

        // Assert
        result.Should().Be("[2025-06-15 14:30:00] [INFO] OrderService: Order processed successfully");
    }

    [Fact]
    public void WhenFormattingEmailTemplateThenShouldReplaceAllPlaceholders()
    {
        // Arrange
        const string template = """
            Dear {0},
            
            Your order #{1} has been shipped.
            Expected delivery: {2:d}
            
            Best regards,
            {3}
            """;

        // Act
        var result = template.StringFormat(
            CultureInfo.GetCultureInfo("en-US"),
            "John Doe",
            "ORD-2025-001",
            new DateTime(2025, 6, 20),
            "Customer Service");

        // Assert
        result.Should().Contain("Dear John Doe");
        result.Should().Contain("ORD-2025-001");
        result.Should().Contain("6/20/2025");
        result.Should().Contain("Customer Service");
    }

    [Fact]
    public void WhenBuildingCsvLineThenShouldJoinWithCommas()
    {
        // Arrange
        var row = new[] { "John", "Doe", "john@example.com", "New York" };

        // Act
        var csvLine = string.Join(",", row);

        // Assert
        csvLine.Should().Be("John,Doe,john@example.com,New York");
    }

    [Fact]
    public void WhenBuildingHtmlClassListThenShouldJoinWithSpaces()
    {
        // Arrange
        var classes = new[] { "btn", "btn-primary", "btn-lg", "active" };

        // Act
        var classAttribute = string.Join(" ", classes);

        // Assert
        classAttribute.Should().Be("btn btn-primary btn-lg active");
    }

    [Fact]
    public void WhenBuildingQueryStringThenShouldFormatAndJoin()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            ["search"] = "laptop",
            ["category"] = "electronics",
            ["page"] = "1"
        };

        // Act
        var queryString = string.Join("&", parameters
            .Select(kvp => "{0}={1}".StringFormat(kvp.Key, kvp.Value)));

        // Assert
        queryString.Should().Contain("search=laptop");
        queryString.Should().Contain("category=electronics");
        queryString.Should().Contain("page=1");
    }

    [Fact]
    public void WhenSplittingControllerNameForRouteThenShouldProduceKebabCase()
    {
        // Arrange
        const string controllerName = "CustomerOrderController";

        // Act
        var words = controllerName.Replace("Controller", "", StringComparison.Ordinal).SplitTypeName();
        var route = words.ToLowerInvariant().Replace(' ', '-');

        // Assert
        route.Should().Be("customer-order");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WhenFormattingWithSpecialCharactersThenShouldHandleCorrectly()
    {
        // Arrange
        const string template = "Path: {0}\\{1}";

        // Act
        var result = template.StringFormat("C:", "Users");

        // Assert
        result.Should().Be("Path: C:\\Users");
    }

    [Fact]
    public void WhenJoiningWithEmptyStringSeparatorThenShouldConcatenate()
    {
        // Arrange
        var chars = new[] { "H", "e", "l", "l", "o" };

        // Act
        var result = string.Join("", chars);

        // Assert
        result.Should().Be("Hello");
    }

    [Fact]
    public void WhenJoiningNullableCollectionWithNullItemsThenShouldHandleGracefully()
    {
        // Arrange
        var items = new string?[] { "a", null, "b", null, "c" };

        // Act
        var result = string.Join(",", items);

        // Assert
        result.Should().Be("a,,b,,c");
    }

    #endregion
}
