using FluentAssertions;

using Xpandables.Net.Text;

namespace Xpandables.Net.Test.UnitTests;
public sealed class TextGeneratorUnitTest
{
    [Fact]
    public void Generate_WithValidLength_ShouldReturnStringOfSpecifiedLength()
    {
        // Arrange
        ushort length = 10;

        // Act
        string result = TextGenerator.Generate(length);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(length);
    }

    [Fact]
    public void Generate_WithCustomLookupCharacters_ShouldReturnStringUsingThoseCharacters()
    {
        // Arrange
        ushort length = 10;
        string lookupCharacters = "ABC123";

        // Act
        string result = TextGenerator.Generate(length, lookupCharacters);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(length);
        result.Should().MatchRegex($"^[{lookupCharacters}]+$");
    }

    [Fact]
    public void Generate_WithLengthLessThanOne_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        ushort length = 0;

        // Act
        Action act = () => TextGenerator.Generate(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Generate_WithLengthGreaterThanMaxValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        int length = ushort.MaxValue + 1;

        // Act
        Action act = () => TextGenerator.Generate(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Generate_WithNullOrWhitespaceLookupCharacters_ShouldThrowArgumentException()
    {
        // Arrange
        ushort length = 10;
        string lookupCharacters = " ";

        // Act
        Action act = () => TextGenerator.Generate(length, lookupCharacters);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}