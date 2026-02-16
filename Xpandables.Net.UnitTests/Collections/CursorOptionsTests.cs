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
using System.Linq.Expressions;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Collections;

/// <summary>
/// Contains unit tests for <see cref="CursorOptions{TSource}"/> and the
/// <see cref="CursorOptions"/> factory, exercising the public API only.
/// </summary>
public sealed class CursorOptionsTests
{
    // ──────────────────────────────────────────────────
    //  CursorOptions.Create – factory configuration
    // ──────────────────────────────────────────────────

    [Fact]
    public void Create_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, int>>? selector = null;

        // Act
        Action act = () => CursorOptions.Create(
            selector!,
            CursorDirection.Forward,
            false,
            null,
            null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("selector");
    }

    [Fact]
    public void Create_WithIntSelector_SetsKeySelectorAndCursorType()
    {
        // Arrange
        Expression<Func<TestEntity, int>> selector = x => x.Id;

        // Act
        var result = CursorOptions.Create(selector);

        // Assert
        result.KeySelector.Should().BeSameAs(selector);
        result.CursorType.Should().Be<int>();
    }

    [Fact]
    public void Create_WithGuidSelector_SetsKeySelectorAndCursorType()
    {
        // Arrange
        Expression<Func<TestEntity, Guid>> selector = x => x.UniqueId;

        // Act
        var result = CursorOptions.Create(selector);

        // Assert
        result.KeySelector.Should().BeSameAs(selector);
        result.CursorType.Should().Be<Guid>();
    }

    [Fact]
    public void Create_WithStringSelector_SetsKeySelectorAndCursorType()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = x => x.Name;

        // Act
        var result = CursorOptions.Create(selector);

        // Assert
        result.KeySelector.Should().BeSameAs(selector);
        result.CursorType.Should().Be<string>();
    }

    [Fact]
    public void Create_WithDefaults_SetsForwardDirectionAndNonInclusive()
    {
        // Arrange & Act
        var result = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Assert
        result.Direction.Should().Be(CursorDirection.Forward);
        result.IsInclusive.Should().BeFalse();
        result.AppliedToken.Should().BeNull();
    }

    [Theory]
    [InlineData(CursorDirection.Forward)]
    [InlineData(CursorDirection.Backward)]
    public void Create_WithDirection_SetsDirection(CursorDirection direction)
    {
        // Act
        var result = CursorOptions.Create<TestEntity, int>(x => x.Id, direction);

        // Assert
        result.Direction.Should().Be(direction);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WithIsInclusive_SetsIsInclusive(bool isInclusive)
    {
        // Act
        var result = CursorOptions.Create<TestEntity, int>(x => x.Id, isInclusive: isInclusive);

        // Assert
        result.IsInclusive.Should().Be(isInclusive);
    }

    [Fact]
    public void Create_WithBackwardDirectionAndInclusive_SetsAllParameters()
    {
        // Act
        var result = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            CursorDirection.Backward,
            isInclusive: true);

        // Assert
        result.Direction.Should().Be(CursorDirection.Backward);
        result.IsInclusive.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────
    //  Default formatter (via CursorOptions.Create)
    // ──────────────────────────────────────────────────

    [Fact]
    public void DefaultFormatter_WithNullToken_ReturnsNull()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var result = options.FormatToken(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(42, "42")]
    [InlineData(-42, "-42")]
    [InlineData(int.MaxValue, "2147483647")]
    [InlineData(int.MinValue, "-2147483648")]
    public void DefaultFormatter_WithIntToken_ReturnsInvariantString(int token, string expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var result = options.FormatToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0L, "0")]
    [InlineData(long.MaxValue, "9223372036854775807")]
    [InlineData(long.MinValue, "-9223372036854775808")]
    public void DefaultFormatter_WithLongToken_ReturnsInvariantString(long token, string expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, long>(x => x.Id);

        // Act
        var result = options.FormatToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1.5, "1.5")]
    [InlineData(-1.5, "-1.5")]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    public void DefaultFormatter_WithDoubleToken_ReturnsInvariantString(double token, string expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, double>(x => x.Score);

        // Act
        var result = options.FormatToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "True")]
    [InlineData(false, "False")]
    public void DefaultFormatter_WithBoolToken_ReturnsInvariantString(bool token, string expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, bool>(x => true);

        // Act
        var result = options.FormatToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DefaultFormatter_WithDateTimeToken_ReturnsInvariantString()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, DateTime>(x => x.CreatedAt);
        var token = new DateTime(2025, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var expected = token.ToString(CultureInfo.InvariantCulture);

        // Act
        var result = options.FormatToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DefaultFormatter_WithGuidToken_ReturnsGuidString()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, Guid>(x => x.UniqueId);
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var result = options.FormatToken(guid);

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("special\ncharacters\ttab")]
    public void DefaultFormatter_WithStringToken_ReturnsStringDirectly(string token)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, string>(x => x.Name);

        // Act
        var result = options.FormatToken(token);

        // Assert
        result.Should().Be(token);
    }

    // ──────────────────────────────────────────────────
    //  Default parser (via CursorOptions.Create)
    // ──────────────────────────────────────────────────

    [Fact]
    public void DefaultParser_WithNullValue_ReturnsNull()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var result = options.ParseToken(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("-42", -42)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void DefaultParser_WithIntString_ReturnsInteger(string token, int expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var result = options.ParseToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    public void DefaultParser_WithLongString_ReturnsLong(string token, long expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, long>(x => x.Id);

        // Act
        var result = options.ParseToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("-123.45", -123.45)]
    public void DefaultParser_WithDoubleString_ReturnsDouble(string token, double expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, double>(x => x.Score);

        // Act
        var result = options.ParseToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    public void DefaultParser_WithBoolString_ReturnsBoolean(string token, bool expected)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, bool>(x => true);

        // Act
        var result = options.ParseToken(token);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DefaultParser_WithDateTimeString_ReturnsDateTime()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, DateTime>(x => x.CreatedAt);

        // Act
        var result = options.ParseToken("01/15/2025 10:30:00");

        // Assert
        result.Should().Be(new DateTime(2025, 1, 15, 10, 30, 0));
    }

    [Fact]
    public void DefaultParser_WithGuidString_ReturnsGuid()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, Guid>(x => x.UniqueId);
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var result = options.ParseToken("12345678-1234-1234-1234-123456789012");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CustomParser_WithGuidString_ParsesCorrectly()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, Guid>(
            x => x.UniqueId,
            parser: s => s is null ? Guid.Empty : Guid.Parse(s));
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var result = options.ParseToken("12345678-1234-1234-1234-123456789012");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    [InlineData("   ")]
    public void DefaultParser_WithStringValue_ReturnsStringUnchanged(string token)
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, string>(x => x.Name);

        // Act
        var result = options.ParseToken(token);

        // Assert
        result.Should().Be(token);
    }

    [Fact]
    public void DefaultParser_WithInvalidIntString_ThrowsFormatException()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        Action act = () => options.ParseToken("not-a-number");

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void DefaultParser_WithInvalidGuidString_ThrowsFormatException()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, Guid>(x => x.UniqueId);

        // Act
        Action act = () => options.ParseToken("invalid-guid");

        // Assert
        act.Should().Throw<FormatException>();
    }

    // ──────────────────────────────────────────────────
    //  Roundtrip: format then parse via default delegates
    // ──────────────────────────────────────────────────

    [Fact]
    public void DefaultFormatAndParse_IntRoundtrip_ReturnsOriginalValue()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);
        const int original = 42;

        // Act
        var formatted = options.FormatToken(original);
        var parsed = options.ParseToken(formatted);

        // Assert
        parsed.Should().Be(original);
    }

    [Fact]
    public void DefaultFormatAndParse_GuidRoundtrip_ReturnsOriginalValue()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, Guid>(x => x.UniqueId);
        var original = Guid.NewGuid();

        // Act
        var formatted = options.FormatToken(original);
        var parsed = options.ParseToken(formatted);

        // Assert
        parsed.Should().Be(original);
    }

    [Fact]
    public void DefaultFormatAndParse_StringRoundtrip_ReturnsOriginalValue()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, string>(x => x.Name);
        const string original = "cursor-value-123";

        // Act
        var formatted = options.FormatToken(original);
        var parsed = options.ParseToken(formatted);

        // Assert
        parsed.Should().Be(original);
    }

    [Fact]
    public void DefaultFormatAndParse_LongRoundtrip_ReturnsOriginalValue()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, long>(x => x.Id);
        const long original = 9_876_543_210L;

        // Act
        var formatted = options.FormatToken(original);
        var parsed = options.ParseToken(formatted);

        // Assert
        parsed.Should().Be(original);
    }

    [Fact]
    public void DefaultFormatAndParse_NullRoundtrip_ReturnsNull()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var formatted = options.FormatToken(null);
        var parsed = options.ParseToken(formatted);

        // Assert
        formatted.Should().BeNull();
        parsed.Should().BeNull();
    }

    // ──────────────────────────────────────────────────
    //  Custom formatter / parser
    // ──────────────────────────────────────────────────

    [Fact]
    public void Create_WithCustomFormatter_UsesProvidedFormatter()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            formatter: v => $"custom-{v}");

        // Act
        var result = options.FormatToken(42);

        // Assert
        result.Should().Be("custom-42");
    }

    [Fact]
    public void Create_WithCustomParser_UsesProvidedParser()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            parser: s => int.Parse(s!, CultureInfo.InvariantCulture) * 2);

        // Act
        var result = options.ParseToken("21");

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void Create_WithCustomFormatterAndParser_RoundtripsCorrectly()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            formatter: v => $"prefix-{v}",
            parser: s => int.Parse(s!.Replace("prefix-", ""), CultureInfo.InvariantCulture));

        // Act
        var formatted = options.FormatToken(99);
        var parsed = options.ParseToken(formatted);

        // Assert
        formatted.Should().Be("prefix-99");
        parsed.Should().Be(99);
    }

    [Fact]
    public void CustomFormatter_ReceivesNullForNullToken()
    {
        // Arrange
        object? captured = "not-set";
        var options = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            formatter: v =>
            {
                captured = v;
                return "result";
            });

        // Act
        options.FormatToken(null);

        // Assert — null is mapped to default(int) which is 0
        captured.Should().Be(0);
    }

    [Fact]
    public void CustomFormatter_ReceivesTypedValueForMatchingToken()
    {
        // Arrange
        int? captured = null;
        var options = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            formatter: v =>
            {
                captured = v;
                return v.ToString();
            });

        // Act
        options.FormatToken(77);

        // Assert
        captured.Should().Be(77);
    }

    // ──────────────────────────────────────────────────
    //  FormatToken / FormatAppliedToken / ParseToken
    //  (record instance methods with explicit delegates)
    // ──────────────────────────────────────────────────

    [Fact]
    public void FormatAppliedToken_DelegatesToFormatTokenWithAppliedToken()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var options = CursorOptions.Create<TestEntity, Guid>(x => x.UniqueId);
        var optionsWithToken = options with { AppliedToken = guid };

        // Act
        var result = optionsWithToken.FormatAppliedToken();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void FormatAppliedToken_WithNullAppliedToken_ReturnsNull()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var result = options.FormatAppliedToken();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FormatToken_WhenFormatterThrows_PropagatesException()
    {
        // Arrange
        var options = new CursorOptions<TestEntity>
        {
            KeySelector = (Expression<Func<TestEntity, int>>)(x => x.Id),
            CursorType = typeof(int),
            TokenFormatter = _ => throw new InvalidOperationException("boom"),
            TokenParser = _ => null!
        };

        // Act
        Action act = () => options.FormatToken(42);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("boom");
    }

    [Fact]
    public void ParseToken_WhenParserThrows_PropagatesException()
    {
        // Arrange
        var options = new CursorOptions<TestEntity>
        {
            KeySelector = (Expression<Func<TestEntity, int>>)(x => x.Id),
            CursorType = typeof(int),
            TokenFormatter = _ => null,
            TokenParser = _ => throw new InvalidOperationException("parse-boom")
        };

        // Act
        Action act = () => options.ParseToken("value");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("parse-boom");
    }

    [Fact]
    public void FormatToken_PassesExactReferenceToDelegate()
    {
        // Arrange
        object? captured = null;
        var options = new CursorOptions<TestEntity>
        {
            KeySelector = (Expression<Func<TestEntity, int>>)(x => x.Id),
            CursorType = typeof(int),
            TokenFormatter = t => { captured = t; return "ok"; },
            TokenParser = _ => null!
        };
        var token = new object();

        // Act
        options.FormatToken(token);

        // Assert
        captured.Should().BeSameAs(token);
    }

    [Fact]
    public void ParseToken_PassesExactStringToDelegate()
    {
        // Arrange
        string? captured = null;
        var options = new CursorOptions<TestEntity>
        {
            KeySelector = (Expression<Func<TestEntity, int>>)(x => x.Id),
            CursorType = typeof(int),
            TokenFormatter = _ => null,
            TokenParser = s => { captured = s; return 0; }
        };

        // Act
        options.ParseToken("my-cursor");

        // Assert
        captured.Should().Be("my-cursor");
    }

    // ──────────────────────────────────────────────────
    //  CursorOptions<TSource> record semantics
    // ──────────────────────────────────────────────────

    [Fact]
    public void WithExpression_CreatesNewInstanceWithOverriddenProperty()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(x => x.Id);

        // Act
        var modified = options with { Direction = CursorDirection.Backward, IsInclusive = true };

        // Assert
        modified.Direction.Should().Be(CursorDirection.Backward);
        modified.IsInclusive.Should().BeTrue();
        modified.KeySelector.Should().BeSameAs(options.KeySelector);
        modified.CursorType.Should().Be(options.CursorType);
    }

    [Fact]
    public void WithExpression_AppliedToken_PreservesOtherProperties()
    {
        // Arrange
        var options = CursorOptions.Create<TestEntity, int>(
            x => x.Id,
            CursorDirection.Backward,
            isInclusive: true);

        // Act
        var withToken = options with { AppliedToken = 99 };

        // Assert
        withToken.AppliedToken.Should().Be(99);
        withToken.Direction.Should().Be(CursorDirection.Backward);
        withToken.IsInclusive.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────
    //  CursorDirection enum coverage
    // ──────────────────────────────────────────────────

    [Fact]
    public void CursorDirection_Forward_HasValueZero()
    {
        ((int)CursorDirection.Forward).Should().Be(0);
    }

    [Fact]
    public void CursorDirection_Backward_HasValueOne()
    {
        ((int)CursorDirection.Backward).Should().Be(1);
    }

    // ──────────────────────────────────────────────────
    //  Helper type
    // ──────────────────────────────────────────────────

    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UniqueId { get; set; }
        public double Score { get; set; }
    }
}