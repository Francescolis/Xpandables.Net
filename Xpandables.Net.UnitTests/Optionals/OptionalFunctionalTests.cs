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

using Xpandables.Net.Optionals;

namespace Xpandables.Net.UnitTests.Optionals;

/// <summary>
/// Unit tests for Optional&lt;T&gt; functional methods including
/// Map, Bind, Empty, and other monadic operations.
/// </summary>
public class OptionalFunctionalTests
{
    #region Map Tests

    [Fact]
    public void Map_WithFunction_WhenNotEmpty_ShouldTransformValue()
    {
        // Arrange
        var optional = Optional.Some(10);

        // Act
        var result = optional.Map(x => x * 2);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Map_WithFunction_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        var result = optional.Map(x => x * 2);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Map_WithFunctionToSameType_WhenNotEmpty_ShouldTransform()
    {
        // Arrange
        var optional = Optional.Some("hello");

        // Act
        var result = optional.Map(x => x.ToUpper());

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public void Map_WithOptionalReturningFunction_WhenNotEmpty_ShouldTransform()
    {
        // Arrange
        var optional = Optional.Some(5);

        // Act
        var result = optional.Map(x => x.ToString());

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Map_WithAction_WhenNotEmpty_ShouldExecuteAction()
    {
        // Arrange
        var optional = Optional.Some(42);
        int capturedValue = 0;

        // Act
        var result = optional.Map(x => capturedValue = x);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(42);
        capturedValue.Should().Be(42);
    }

    [Fact]
    public void Map_WithAction_WhenEmpty_ShouldNotExecuteAction()
    {
        // Arrange
        var optional = Optional.Empty<int>();
        bool actionExecuted = false;

        // Act
        var result = optional.Map(x => actionExecuted = true);

        // Assert
        result.IsEmpty.Should().BeTrue();
        actionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Map_WithParameterlessAction_WhenNotEmpty_ShouldExecute()
    {
        // Arrange
        var optional = Optional.Some("test");
        bool executed = false;

        // Act
        var result = optional.Map(() => executed = true);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void Map_WithNullFunction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var optional = Optional.Some(42);

        // Act
        Action act = () => optional.Map((Func<int, int>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Bind Tests

    [Fact]
    public void Bind_WithFunction_WhenNotEmpty_ShouldTransform()
    {
        // Arrange
        var optional = Optional.Some(10);

        // Act
        var result = optional.Bind(x => x * 2.5);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(25.0);
    }

    [Fact]
    public void Bind_WithFunction_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        var result = optional.Bind(x => x.ToString());

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Bind_WithOptionalReturningFunction_WhenNotEmpty_ShouldFlatten()
    {
        // Arrange
        var optional = Optional.Some(5);

        // Act
        var result = optional.Bind(x => Optional.Some(x.ToString()));

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_WithOptionalReturningEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var optional = Optional.Some(5);

        // Act
        var result = optional.Bind(x => Optional.Empty<string>());

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Bind_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var optional = Optional.Some(10);

        // Act
        var result = optional
            .Bind(x => x * 2)
            .Bind(x => x + 5)
            .Bind(x => x.ToString());

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be("25");
    }

    [Fact]
    public void Bind_WithNullFunction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var optional = Optional.Some(42);

        // Act
        Action act = () => optional.Bind((Func<int, string>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Empty (Else) Tests

    [Fact]
    public void Empty_WithFunction_WhenEmpty_ShouldProvideValue()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        var result = optional.Empty(() => 42);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Empty_WithFunction_WhenNotEmpty_ShouldReturnOriginal()
    {
        // Arrange
        var optional = Optional.Some(10);

        // Act
        var result = optional.Empty(() => 42);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Empty_WithOptionalReturningFunction_WhenEmpty_ShouldProvideOptional()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        var result = optional.Empty(() => Optional.Some(99));

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void Empty_WithAction_WhenEmpty_ShouldExecute()
    {
        // Arrange
        var optional = Optional.Empty<int>();
        bool executed = false;

        // Act
        var result = optional.Empty(() => executed = true);

        // Assert
        result.IsEmpty.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void Empty_WithAction_WhenNotEmpty_ShouldNotExecute()
    {
        // Arrange
        var optional = Optional.Some(42);
        bool executed = false;

        // Act
        var result = optional.Empty(() => executed = true);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        executed.Should().BeFalse();
    }

    [Fact]
    public void Empty_WithNullFunction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        Action act = () => optional.Empty((Func<int>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToOptional Tests

    [Fact]
    public void ToOptional_WithValue_ShouldCreateOptional()
    {
        // Arrange
        string value = "test";

        // Act
        var optional = value.ToOptional();

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Should().Be("test");
    }

    [Fact]
    public void ToOptional_WithNull_ShouldCreateEmpty()
    {
        // Arrange
        string? value = null;

        // Act
        var optional = value.ToOptional();

        // Assert
        optional.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void ToOptional_FromOptional_ShouldWorkForSameType()
    {
        // Arrange
        var original = Optional.Some(42);

        // Act
        var converted = original.ToOptional<int>();

        // Assert
        converted.IsNotEmpty.Should().BeTrue();
        converted.Value.Should().Be(42);
    }

    [Fact]
    public void ToOptional_FromOptional_WithCompatibleType_ShouldConvert()
    {
        // Arrange
        var original = Optional.Some<object>("string value");

        // Act
        var converted = original.ToOptional<string>();

        // Assert
        converted.IsNotEmpty.Should().BeTrue();
        converted.Value.Should().Be("string value");
    }

    [Fact]
    public void ToOptional_FromOptional_WithIncompatibleType_ShouldReturnEmpty()
    {
        // Arrange
        var original = Optional.Some(42);

        // Act
        var converted = original.ToOptional<string>();

        // Assert
        converted.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region Practical Scenario Tests

    [Fact]
    public void Scenario_ParseInt_Success_ShouldReturnOptional()
    {
        // Arrange
        string input = "123";

        // Act
        var result = TryParseInt(input);

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(123);
    }

    [Fact]
    public void Scenario_ParseInt_Failure_ShouldReturnEmpty()
    {
        // Arrange
        string input = "not a number";

        // Act
        var result = TryParseInt(input);

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Scenario_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var user = Optional.Some(new User { Age = 25 });

        // Act
        var ageCategory = user
            .Bind(u => u.Age < 18 ? Optional.Some("Minor") : u.Age < 65 ? Optional.Some("Adult") : Optional.Some("Senior"))
            .Empty(() => Optional.Some("Unknown"));

        // Assert
        ageCategory.IsNotEmpty.Should().BeTrue();
        ageCategory.Value.Should().Be("Adult");
    }

    [Fact]
    public void Scenario_NullPropagation_ShouldHandleGracefully()
    {
        // Arrange
        var user = Optional.Empty<User>();

        // Act
        var email = user
            .Bind(u => u.Email)
            .Empty(() => "no-email@example.com");

        // Assert
        email.IsNotEmpty.Should().BeTrue();
        email.Value.Should().Be("no-email@example.com");
    }

    [Fact]
    public void Scenario_ConditionalMapping_ShouldFilterCorrectly()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act
        var results = numbers
            .Select(n => Optional.Some(n))
            .Select(opt => opt.Map(n => n % 2 == 0 ? n * 10 : 0))
            .Where(opt => opt.IsNotEmpty && opt.Value != 0)
            .Select(opt => opt.Value)
            .ToList();

        // Assert
        results.Should().Equal(20, 40);
    }

    [Fact]
    public void Scenario_DefaultValueWithFactory_ShouldBeEfficient()
    {
        // Arrange
        var optional = Optional.Some(42);
        bool expensiveOperationCalled = false;

        // Act
        var result = optional.Empty(() =>
        {
            expensiveOperationCalled = true;
            return PerformExpensiveOperation();
        });

        // Assert
        result.Value.Should().Be(42);
        expensiveOperationCalled.Should().BeFalse("factory should not be called when optional has value");
    }

    #endregion

    #region Async Tests

    [Fact]
    public async Task MapAsync_WhenNotEmpty_ShouldTransformAsync()
    {
        // Arrange
        var optional = Optional.Some(10);

        // Act
        var result = await optional.MapAsync(async x =>
        {
            await Task.Delay(10);
            return x * 2;
        });

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task MapAsync_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        var result = await optional.MapAsync(async x =>
        {
            await Task.Delay(10);
            return x * 2;
        });

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_WhenNotEmpty_ShouldTransformAsync()
    {
        // Arrange
        var optional = Optional.Some("123");

        // Act
        var result = await optional.BindAsync(async s =>
        {
            await Task.Delay(10);
            return int.TryParse(s, out var value) ? Optional.Some(value) : Optional.Empty<int>();
        });

        // Assert
        result.IsNotEmpty.Should().BeTrue();
        result.Value.Should().Be(123);
    }

    #endregion

    #region Helper Methods

    private static Optional<int> TryParseInt(string input)
    {
        return int.TryParse(input, out var result)
            ? Optional.Some(result)
            : Optional.Empty<int>();
    }

    private static int PerformExpensiveOperation()
    {
        // Simulate expensive operation
        Thread.Sleep(100);
        return 999;
    }

    private class User
    {
        public int Age { get; set; }
        public Optional<string> Email { get; set; }
    }

    #endregion
}

