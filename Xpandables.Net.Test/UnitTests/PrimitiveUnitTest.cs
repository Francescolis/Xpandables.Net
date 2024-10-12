using FluentAssertions;

using Xpandables.Net.Text;

namespace Xpandables.Net.Test.UnitTests;
public sealed class PrimitiveUnitTest
{
    public readonly record struct TestPrimitive : IPrimitive<TestPrimitive, string>
    {
        public required string Value { get; init; }

        public static TestPrimitive Create(string value) =>
            new() { Value = value };

        public static TestPrimitive Default() =>
            new() { Value = string.Empty };

        public static implicit operator string(TestPrimitive primitive) =>
            primitive.Value;

        public static implicit operator TestPrimitive(string value) =>
            new() { Value = value };
    }

    public class IPrimitiveTests
    {
        [Fact]
        public void Create_ShouldReturnPrimitiveWithValue()
        {
            // Arrange
            string value = "test";

            // Act
            var primitive = TestPrimitive.Create(value);

            // Assert
            primitive.Value.Should().Be(value);
        }

        [Fact]
        public void Default_ShouldReturnPrimitiveWithDefaultValue()
        {
            // Act
            var primitive = TestPrimitive.Default();

            // Assert
            primitive.Value.Should().BeEmpty();
        }

        [Fact]
        public void ImplicitConversion_ToValue_ShouldReturnPrimitiveValue()
        {
            // Arrange
            var primitive = TestPrimitive.Create("test");

            // Act
            string value = primitive;

            // Assert
            value.Should().Be("test");
        }

        [Fact]
        public void ImplicitConversion_ToPrimitive_ShouldReturnPrimitive()
        {
            // Arrange
            string value = "test";

            // Act
            TestPrimitive primitive = value;

            // Assert
            primitive.Value.Should().Be(value);
        }

        [Fact]
        public void IPrimitive_Value_ShouldReturnObjectValue()
        {
            // Arrange
            IPrimitive primitive = TestPrimitive.Create("test");

            // Act
            object value = primitive.Value;

            // Assert
            value.Should().Be("test");
        }
    }

}
