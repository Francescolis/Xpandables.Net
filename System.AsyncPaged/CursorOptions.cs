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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;

namespace System.Collections.Generic;

/// <summary>
/// Determines the direction in which a cursor-based pagination query advances.
/// </summary>
public enum CursorDirection
{
    /// <summary>
    /// The cursor moves forward using greater-than semantics.
    /// </summary>
    Forward = 0,

    /// <summary>
    /// The cursor moves backward using less-than semantics.
    /// </summary>
    Backward = 1
}

/// <summary>
/// Represents the configuration required to apply cursor-based pagination over a query.
/// </summary>
/// <typeparam name="TSource">The type of the items returned by the query.</typeparam>
/// <remarks>
/// Provide an instance of this type when constructing <see cref="IAsyncPagedEnumerable{T}"/>.
/// </remarks>
public sealed record CursorOptions<TSource>
{
    /// <summary>
    /// Gets the expression used to order source items when applying the cursor.
    /// </summary>
    public required LambdaExpression KeySelector { get; init; }

    /// <summary>
    /// Gets the underlying type of the cursor value.
    /// </summary>
    public required Type CursorType { get; init; }

    /// <summary>
    /// Gets the direction applied when evaluating the cursor.
    /// </summary>
    public CursorDirection Direction { get; init; } = CursorDirection.Forward;

    /// <summary>
    /// Gets a value indicating whether the cursor comparison should be inclusive.
    /// </summary>
    public bool IsInclusive { get; init; }

    /// <summary>
    /// Gets the currently applied cursor value, if any.
    /// </summary>
    public object? AppliedToken { get; init; }

    /// <summary>
    /// Gets the delegate used to convert cursor values into transport tokens.
    /// </summary>
    public required Func<object?, string?> TokenFormatter { get; init; }

    /// <summary>
    /// Gets the delegate used to parse transport tokens into cursor values.
    /// </summary>
    public required Func<string?, object?> TokenParser { get; init; }

    /// <summary>
    /// Formats the supplied cursor value using <see cref="TokenFormatter"/>.
    /// </summary>
    public string? FormatToken(object? token) => TokenFormatter(token);

    /// <summary>
    /// Formats <see cref="AppliedToken"/> using <see cref="TokenFormatter"/>.
    /// </summary>
    public string? FormatAppliedToken() => FormatToken(AppliedToken);

    /// <summary>
    /// Parses the specified token using <see cref="TokenParser"/>.
    /// </summary>
    public object? ParseToken(string? token) => TokenParser(token);
}

/// <summary>
/// Factory helpers for creating <see cref="CursorOptions{TSource}"/> instances.
/// </summary>
public static class CursorOptions
{
    /// <summary>
    /// Creates a new instance of the cursor options for the specified source and cursor types, using the provided key
    /// selector and optional formatting and parsing logic.
    /// </summary>
    /// <remarks>If no custom formatter or parser is provided, default implementations are used based on the
    /// cursor type. This method is typically used to configure cursor-based pagination or navigation
    /// scenarios.</remarks>
    /// <typeparam name="TSource">The type of the source elements over which the cursor operates.</typeparam>
    /// <typeparam name="TCursor">The type of the cursor value used for pagination or navigation.</typeparam>
    /// <param name="selector">An expression that selects the cursor key from a source element. Cannot be null.</param>
    /// <param name="direction">The direction in which the cursor should move. The default is <see cref="CursorDirection.Forward"/>.</param>
    /// <param name="isInclusive">A value indicating whether the cursor comparison is inclusive. If <see langword="true"/>, the element matching
    /// the cursor value is included in the results.</param>
    /// <param name="formatter">An optional function that formats a cursor value of type <typeparamref name="TCursor"/> as a string token. If
    /// null, a default formatter is used.</param>
    /// <param name="parser">An optional function that parses a string token into a cursor value of type <typeparamref name="TCursor"/>. If
    /// null, a default parser is used.</param>
    /// <returns>A <see cref="CursorOptions{TSource}"/> instance configured with the specified selector, direction, inclusivity,
    /// and optional formatter and parser.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    public static CursorOptions<TSource> Create<TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCursor>(
        Expression<Func<TSource, TCursor>> selector,
        CursorDirection direction = CursorDirection.Forward,
        bool isInclusive = false,
        Func<TCursor?, string?>? formatter = null,
        Func<string?, TCursor?>? parser = null)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new CursorOptions<TSource>
        {
            KeySelector = selector,
            CursorType = typeof(TCursor),
            Direction = direction,
            IsInclusive = isInclusive,
            TokenFormatter = formatter is null
                ? token => CursorTokenConverters.Format(token, typeof(TCursor))
                : token => formatter(token switch
                {
                    null => default,
                    TCursor typed => typed,
                    _ => ObjectExtensions.ChangeTypeNullable<TCursor>(token, CultureInfo.InvariantCulture)
                }),
            TokenParser = parser is null
                ? value => CursorTokenConverters.Parse(value, typeof(TCursor))
                : value => parser(value),
        };
    }

    /// <summary>
    /// Creates a new first-stage fluent builder for the specified source type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements over which the cursor operates.</typeparam>
    /// <returns>A new <see cref="CursorOptionsBuilder{TSource}"/> instance.</returns>
    public static CursorOptionsBuilder<TSource> For<TSource>()
        => new();
}

/// <summary>
/// A first-stage fluent builder for creating <see cref="CursorOptions{TSource}"/> instances.
/// </summary>
/// <typeparam name="TSource">The type of the source elements over which the cursor operates.</typeparam>
/// <remarks>Obtain an instance via <see cref="CursorOptions.For{TSource}"/>.</remarks>
public readonly record struct CursorOptionsBuilder<TSource>
{
    /// <summary>
    /// Specifies the key selector expression and advances the builder to the second stage.
    /// </summary>
    /// <typeparam name="TCursor">The type of the cursor value.</typeparam>
    /// <param name="selector">An expression that selects the cursor key from a source element.</param>
    /// <returns>A <see cref="CursorOptionsBuilder{TSource, TCursor}"/> with the key selector applied.</returns>
    public CursorOptionsBuilder<TSource, TCursor> WithKey<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCursor>(
        Expression<Func<TSource, TCursor>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(selector);
    }
}

/// <summary>
/// A second-stage fluent builder for creating <see cref="CursorOptions{TSource}"/> instances.
/// </summary>
/// <typeparam name="TSource">The type of the source elements over which the cursor operates.</typeparam>
/// <typeparam name="TCursor">The type of the cursor value used for pagination or navigation.</typeparam>
public readonly record struct CursorOptionsBuilder<TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCursor>
{
    private readonly Expression<Func<TSource, TCursor>>? _selector;
    private readonly CursorDirection _direction;
    private readonly bool _isInclusive;
    private readonly object? _appliedToken;
    private readonly Func<TCursor?, string?>? _formatter;
    private readonly Func<string?, TCursor?>? _parser;

    internal CursorOptionsBuilder(Expression<Func<TSource, TCursor>> selector)
    {
        _selector = selector;
        _direction = CursorDirection.Forward;
    }

    private CursorOptionsBuilder(
        Expression<Func<TSource, TCursor>>? selector,
        CursorDirection direction,
        bool isInclusive,
        object? appliedToken,
        Func<TCursor?, string?>? formatter,
        Func<string?, TCursor?>? parser)
    {
        _selector = selector;
        _direction = direction;
        _isInclusive = isInclusive;
        _appliedToken = appliedToken;
        _formatter = formatter;
        _parser = parser;
    }

    /// <summary>Sets the cursor direction to <see cref="CursorDirection.Forward"/>.</summary>
    /// <returns>A new builder with the direction set to forward.</returns>
    public CursorOptionsBuilder<TSource, TCursor> Forward()
        => new(_selector, CursorDirection.Forward, _isInclusive, _appliedToken, _formatter, _parser);

    /// <summary>Sets the cursor direction to <see cref="CursorDirection.Backward"/>.</summary>
    /// <returns>A new builder with the direction set to backward.</returns>
    public CursorOptionsBuilder<TSource, TCursor> Backward()
        => new(_selector, CursorDirection.Backward, _isInclusive, _appliedToken, _formatter, _parser);

    /// <summary>Sets the cursor direction.</summary>
    /// <param name="direction">The direction to apply.</param>
    /// <returns>A new builder with the specified direction.</returns>
    public CursorOptionsBuilder<TSource, TCursor> WithDirection(CursorDirection direction)
        => new(_selector, direction, _isInclusive, _appliedToken, _formatter, _parser);

    /// <summary>Marks the cursor comparison as inclusive.</summary>
    /// <returns>A new builder with inclusive comparison enabled.</returns>
    public CursorOptionsBuilder<TSource, TCursor> Inclusive()
        => new(_selector, _direction, true, _appliedToken, _formatter, _parser);

    /// <summary>Sets the currently applied cursor token value.</summary>
    /// <param name="token">The cursor token to apply.</param>
    /// <returns>A new builder with the applied token set.</returns>
    public CursorOptionsBuilder<TSource, TCursor> WithAppliedToken(object? token)
        => new(_selector, _direction, _isInclusive, token, _formatter, _parser);

    /// <summary>Sets a custom token formatter.</summary>
    /// <param name="formatter">A function that formats a cursor value as a string token.</param>
    /// <returns>A new builder with the custom formatter applied.</returns>
    public CursorOptionsBuilder<TSource, TCursor> WithFormatter(Func<TCursor?, string?> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        return new(_selector, _direction, _isInclusive, _appliedToken, formatter, _parser);
    }

    /// <summary>Sets a custom token parser.</summary>
    /// <param name="parser">A function that parses a string token into a cursor value.</param>
    /// <returns>A new builder with the custom parser applied.</returns>
    public CursorOptionsBuilder<TSource, TCursor> WithParser(Func<string?, TCursor?> parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        return new(_selector, _direction, _isInclusive, _appliedToken, _formatter, parser);
    }

    /// <summary>
    /// Builds and returns the configured <see cref="CursorOptions{TSource}"/> instance.
    /// </summary>
    /// <returns>A <see cref="CursorOptions{TSource}"/> configured from the current builder state.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    public CursorOptions<TSource> Build()
    {
        var selector = _selector;
        ArgumentNullException.ThrowIfNull(selector);

        return CursorOptions.Create(selector, _direction, _isInclusive, _formatter, _parser) with
        {
            AppliedToken = _appliedToken
        };
    }

    /// <summary>
    /// Converts the builder to a <see cref="CursorOptions{TSource}"/> instance
    /// by calling <see cref="Build"/>. Alternate for the implicit conversion operator.
    /// </summary>
    /// <returns>A <see cref="CursorOptions{TSource}"/> configured from the current builder state.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    public CursorOptions<TSource> ToCursorOptions() => Build();

    /// <summary>
    /// Implicitly converts the builder to a <see cref="CursorOptions{TSource}"/> instance
    /// by calling <see cref="Build"/>.
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "TCursor is annotated with DynamicallyAccessedMembers.")]
    public static implicit operator CursorOptions<TSource>(CursorOptionsBuilder<TSource, TCursor> builder)
        => builder.Build();
}

internal static class CursorTokenConverters
{
    public static string? Format(
        object? token,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type cursorType)
    {
        if (token is null)
        {
            return null;
        }

        return cursorType == typeof(string)
            ? (string)token
            : Convert.ToString(token, CultureInfo.InvariantCulture);
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "cursorType is annotated with DynamicallyAccessedMembers.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "cursorType is annotated with DynamicallyAccessedMembers.")]
    public static object? Parse(
        string? value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type cursorType)
    {
        if (value is null)
        {
            return null;
        }

        return cursorType == typeof(string)
            ? value
            : ObjectExtensions.ChangeTypeNullable(value, cursorType, CultureInfo.InvariantCulture);
    }
}
