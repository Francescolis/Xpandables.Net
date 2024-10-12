using FluentAssertions;

using Xpandables.Net.Text;

namespace Xpandables.Net.Test.UnitTests;
public sealed class TextCryptographyUnitTest
{
    [Fact]
    public void Encrypt_WithValidValue_ShouldReturnEncryptedValue()
    {
        // Arrange
        string value = "TestValue";

        // Act
        EncryptedValue result = TextCryptography.Encrypt(value);

        // Assert
        result.Value.Should().NotBeNullOrEmpty();
        result.Key.Should().NotBeNullOrEmpty();
        result.Salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Decrypt_WithValidEncryptedValue_ShouldReturnOriginalValue()
    {
        // Arrange
        string value = "TestValue";
        EncryptedValue encrypted = TextCryptography.Encrypt(value);

        // Act
        string result = TextCryptography.Decrypt(encrypted);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void AreEqual_WithMatchingValues_ShouldReturnTrue()
    {
        // Arrange
        string value = "TestValue";
        EncryptedValue encrypted = TextCryptography.Encrypt(value);

        // Act
        bool result = TextCryptography.AreEqual(encrypted, value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WithNonMatchingValues_ShouldReturnFalse()
    {
        // Arrange
        string value = "TestValue";
        EncryptedValue encrypted = TextCryptography.Encrypt(value);
        string differentValue = "DifferentValue";

        // Act
        bool result = TextCryptography.AreEqual(encrypted, differentValue);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateSalt_WithValidLength_ShouldReturnSaltOfSpecifiedLength()
    {
        // Arrange
        ushort length = 32;

        // Act
        string result = TextCryptography.GenerateSalt(length);

        // Assert
        byte[] saltBytes = Convert.FromBase64String(result);
        saltBytes.Should().HaveCount(length);
    }

    [Fact]
    public void GenerateSalt_WithLengthLessThanOne_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        ushort length = 0;

        // Act
        Action act = () => TextCryptography.GenerateSalt(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateSalt_WithLengthGreaterThanMaxValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        int length = ushort.MaxValue + 1;

        // Act
        Action act = () => TextCryptography.GenerateSalt(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
