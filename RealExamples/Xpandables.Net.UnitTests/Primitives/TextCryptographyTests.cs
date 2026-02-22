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

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class TextGeneratorTests
{
    #region Generate Tests

    [Fact]
    public void WhenGeneratingWithDefaultCharactersThenShouldReturnStringOfCorrectLength()
    {
        // Arrange
        const int length = 16;

        // Act
        string result = TextGenerator.Generate(length);

        // Assert
        result.Should().HaveLength(length);
    }

    [Fact]
    public void WhenGeneratingWithMinimumLengthThenShouldReturnSingleCharacter()
    {
        // Arrange
        const int length = 1;

        // Act
        string result = TextGenerator.Generate(length);

        // Assert
        result.Should().HaveLength(1);
    }

    [Fact]
    public void WhenGeneratingWithLargeLengthThenShouldReturnStringOfCorrectLength()
    {
        // Arrange
        const int length = 1000;

        // Act
        string result = TextGenerator.Generate(length);

        // Assert
        result.Should().HaveLength(length);
    }

    [Fact]
    public void WhenGeneratingTwiceThenResultsShouldBeUnique()
    {
        // Arrange
        const int length = 20;

        // Act
        string first = TextGenerator.Generate(length);
        string second = TextGenerator.Generate(length);

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void WhenGeneratingWithCustomCharactersThenResultShouldOnlyContainThoseCharacters()
    {
        // Arrange
        const int length = 50;
        const string lookup = "abc123";

        // Act
        string result = TextGenerator.Generate(length, lookup);

        // Assert
        result.Should().HaveLength(length);
        result.ToCharArray().Should().OnlyContain(c => lookup.Contains(c, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WhenGeneratingWithLengthLessThanOneThenShouldThrowArgumentOutOfRangeException(int length)
    {
        // Act
        Action act = () => TextGenerator.Generate(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WhenGeneratingWithLengthGreaterThanMaxThenShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        int length = ushort.MaxValue + 1;

        // Act
        Action act = () => TextGenerator.Generate(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WhenGeneratingWithNullOrWhitespaceLookupCharactersThenShouldThrowArgumentException(string? lookup)
    {
        // Act
        Action act = () => TextGenerator.Generate(10, lookup!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}

public sealed class TextCryptographyTests
{
    #region GenerateSalt Tests

    [Fact]
    public void WhenGeneratingSaltWithDefaultLengthThenShouldReturnNonEmptyBase64String()
    {
        // Act
        string salt = TextCryptography.GenerateSalt();

        // Assert
        salt.Should().NotBeNullOrWhiteSpace();
        Convert.FromBase64String(salt).Should().HaveCount(32);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(64)]
    public void WhenGeneratingSaltWithCustomLengthThenShouldReturnBase64StringOfCorrectByteLength(int length)
    {
        // Act
        string salt = TextCryptography.GenerateSalt(length);

        // Assert
        salt.Should().NotBeNullOrWhiteSpace();
        Convert.FromBase64String(salt).Should().HaveCount(length);
    }

    [Fact]
    public void WhenGeneratingSaltTwiceThenResultsShouldBeUnique()
    {
        // Act
        string first = TextCryptography.GenerateSalt();
        string second = TextCryptography.GenerateSalt();

        // Assert
        first.Should().NotBe(second);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WhenGeneratingSaltWithLengthLessThanOneThenShouldThrowArgumentOutOfRangeException(int length)
    {
        // Act
        Action act = () => TextCryptography.GenerateSalt(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WhenGeneratingSaltWithLengthGreaterThanMaxThenShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        int length = ushort.MaxValue + 1;

        // Act
        Action act = () => TextCryptography.GenerateSalt(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Encrypt Tests

    [Fact]
    public void WhenEncryptingWithAutoGeneratedKeyAndSaltThenShouldReturnPopulatedEncryptedValue()
    {
        // Arrange
        const string plainText = "Hello, World!";

        // Act
        EncryptedValue result = TextCryptography.Encrypt(plainText);

        // Assert
        result.Key.Should().NotBeNullOrWhiteSpace();
        result.Salt.Should().NotBeNullOrWhiteSpace();
        result.Value.Should().NotBeNullOrWhiteSpace();
        result.Value.Should().NotBe(plainText);
    }

    [Fact]
    public void WhenEncryptingWithExplicitKeyAndSaltThenEncryptedValueShouldContainThem()
    {
        // Arrange
        const string plainText = "SecretData";
        string key = TextGenerator.Generate(12);
        string salt = TextCryptography.GenerateSalt();

        // Act
        EncryptedValue result = TextCryptography.Encrypt(plainText, key, salt);

        // Assert
        result.Key.Should().Be(key);
        result.Salt.Should().Be(salt);
        result.Value.Should().NotBe(plainText);
    }

    [Fact]
    public void WhenEncryptingSameValueTwiceWithSameKeyAndSaltThenResultsShouldBeEqual()
    {
        // Arrange
        const string plainText = "RepeatableEncryption";
        string key = TextGenerator.Generate(12);
        string salt = TextCryptography.GenerateSalt();

        // Act
        EncryptedValue first = TextCryptography.Encrypt(plainText, key, salt);
        EncryptedValue second = TextCryptography.Encrypt(plainText, key, salt);

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void WhenEncryptingWithDifferentKeysThenResultsShouldDiffer()
    {
        // Arrange
        const string plainText = "SameText";
        string salt = TextCryptography.GenerateSalt();

        // Act
        EncryptedValue first = TextCryptography.Encrypt(plainText, TextGenerator.Generate(12), salt);
        EncryptedValue second = TextCryptography.Encrypt(plainText, TextGenerator.Generate(12), salt);

        // Assert
        first.Value.Should().NotBe(second.Value);
    }

    #endregion

    #region Decrypt Tests

    [Fact]
    public void WhenDecryptingEncryptedValueThenShouldReturnOriginalPlainText()
    {
        // Arrange
        const string plainText = "Hello, World!";
        EncryptedValue encrypted = TextCryptography.Encrypt(plainText);

        // Act
        string decrypted = TextCryptography.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("A longer string with special chars: !@#$%^&*()")]
    [InlineData("Unicode: 日本語テスト")]
    public void WhenDecryptingVariousPlainTextsThenShouldReturnOriginalValues(string plainText)
    {
        // Arrange
        EncryptedValue encrypted = TextCryptography.Encrypt(plainText);

        // Act
        string decrypted = TextCryptography.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void WhenDecryptingWithExplicitKeyAndSaltThenShouldReturnOriginalPlainText()
    {
        // Arrange
        const string plainText = "ExplicitKeyAndSalt";
        string key = TextGenerator.Generate(16);
        string salt = TextCryptography.GenerateSalt();
        EncryptedValue encrypted = TextCryptography.Encrypt(plainText, key, salt);

        // Act
        string decrypted = TextCryptography.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    #endregion

    #region AreEqual Tests

    [Fact]
    public void WhenComparingEncryptedValueWithMatchingPlainTextThenShouldReturnTrue()
    {
        // Arrange
        const string plainText = "PasswordToCheck";
        EncryptedValue encrypted = TextCryptography.Encrypt(plainText);

        // Act
        bool result = TextCryptography.AreEqual(encrypted, plainText);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void WhenComparingEncryptedValueWithNonMatchingPlainTextThenShouldReturnFalse()
    {
        // Arrange
        const string original = "CorrectPassword";
        const string wrong = "WrongPassword";
        EncryptedValue encrypted = TextCryptography.Encrypt(original);

        // Act
        bool result = TextCryptography.AreEqual(encrypted, wrong);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void WhenComparingEncryptedValueWithNullThenShouldThrowArgumentNullException()
    {
        // Arrange
        EncryptedValue encrypted = TextCryptography.Encrypt("SomeValue");

        // Act
        Action act = () => TextCryptography.AreEqual(encrypted, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenComparingEncryptedValueWithCaseDifferentValueThenShouldReturnFalse()
    {
        // Arrange
        const string original = "CaseSensitive";
        EncryptedValue encrypted = TextCryptography.Encrypt(original);

        // Act
        bool result = TextCryptography.AreEqual(encrypted, original.ToUpperInvariant());

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
