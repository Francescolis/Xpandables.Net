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
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System;

#pragma warning disable CA2225 // Operator overloads have named alternates
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CA1000 // Do not declare static members on generic types

/// <summary>
/// Defines a contract for objects that encapsulate a primitive value.
/// </summary>
/// <remarks>Use the <see cref="IPrimitive{TPrimitive, TValue}"/> interface for strongly-typed primitive value wrappers.</remarks>
public interface IPrimitive
{
    object Value { get; }
}

/// <summary>
/// Defines a strongly typed primitive value wrapper that exposes the underlying value of type <typeparamref
/// name="TValue"/>.
/// </summary>
/// <remarks>Use the <see cref="IPrimitive{TPrimitive, TValue}"/> interface for strongly-typed primitive value wrappers.</remarks>
public interface IPrimitive<TValue> : IPrimitive
    where TValue : notnull
{
    new TValue Value { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    object IPrimitive.Value => Value;
}

/// <summary>
/// Defines a generic interface for strongly-typed primitive value objects, supporting creation, comparison, formatting,
/// and conversion between the primitive type and its underlying value type.
/// </summary>
/// <remarks>Implementations of this interface provide value semantics, type safety, and support for equality,
/// comparison, and formatting operations. The interface also defines static abstract members for creating instances and
/// performing implicit conversions between the primitive and its underlying value type. This pattern is commonly used
/// to wrap primitive types (such as integers or strings) in domain-specific value objects.
/// <para>Always decorate your primitive types with the <see cref="PrimitiveJsonConverterAttribute{TPrimitive, TValue}"/> to enable automatic
/// serialization and deserialization.</para>
/// </remarks>
/// <typeparam name="TPrimitive">The struct type that implements this interface, representing the strongly-typed primitive value.</typeparam>
/// <typeparam name="TValue">The underlying value type encapsulated by the primitive. Must be non-null.</typeparam>
public interface IPrimitive<TPrimitive, TValue> :
    IPrimitive<TValue>,
    IEquatable<TPrimitive>,
    IComparable<TPrimitive>,
    IComparable,
    IFormattable
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    /// <summary>
    /// Creates a new instance of the primitive type from the specified value.
    /// </summary>
    /// <param name="value">The value to be encapsulated by the primitive type. The interpretation and validation of this value depend on
    /// the implementation.</param>
    /// <returns>A new instance of the primitive type that represents the specified value.</returns>
    static abstract TPrimitive Create(TValue value);

    /// <summary>
    /// Gets the default value for the type parameter <typeparamref name="TValue"/> as defined by the implementing type.
    /// </summary>
    /// <remarks>This property provides a standardized way to access the default value for a given type
    /// parameter. The meaning of the default value may vary depending on the implementation; for reference types, it is
    /// typically <see langword="null"/>, and for value types, it is usually the result of the default
    /// constructor.</remarks>
    static abstract TValue DefaultValue { get; }

    /// <summary>
    /// Returns a default instance of the primitive type represented by the implementing type.
    /// </summary>
    /// <remarks>This method is typically used to obtain a baseline or uninitialized value for the primitive
    /// type. The definition of the default instance may vary depending on the implementation.</remarks>
    /// <returns>A default value of type <typeparamref name="TPrimitive"/> as defined by the implementing type.</returns>
    static TPrimitive DefaultInstance() => TPrimitive.Create(TPrimitive.DefaultValue);

    /// <summary>
    /// Defines an implicit conversion from the primitive type to the value type.
    /// </summary>
    /// <remarks>Implement this operator to enable seamless conversion from the underlying primitive type to
    /// the custom value type. This allows instances of the primitive type to be used where the value type is expected
    /// without explicit casting.</remarks>
    /// <param name="primitive">The primitive value to convert to the value type.</param>
    static abstract implicit operator TValue(TPrimitive primitive);

    /// <summary>
    /// Defines an implicit conversion from the TValue type to its corresponding TPrimitive type.
    /// </summary>
    /// <remarks>This operator enables seamless conversion between TValue and TPrimitive without requiring an
    /// explicit cast. Use this conversion when TValue can be represented as TPrimitive without loss of
    /// information.</remarks>
    /// <param name="value">The TValue instance to convert to TPrimitive.</param>
    static abstract implicit operator TPrimitive(TValue value);

    /// <summary>
    /// Determines whether two instances of the primitive type are equal.
    /// </summary>
    /// <param name="left">The first primitive value to compare.</param>
    /// <param name="right">The second primitive value to compare.</param>
    /// <returns>true if the specified values are equal; otherwise, false.</returns>
    static abstract bool operator ==(TPrimitive left, TPrimitive right);

    /// <summary>
    /// Determines whether two instances of the primitive type are not equal.
    /// </summary>
    /// <param name="left">The first primitive value to compare.</param>
    /// <param name="right">The second primitive value to compare.</param>
    /// <returns>true if the specified values are not equal; otherwise, false.</returns>
    static abstract bool operator !=(TPrimitive left, TPrimitive right);

    /// <summary>
    /// Returns a string representation of the current value.
    /// </summary>
    /// <returns>A string that represents the value. Returns an empty string if the value is null.</returns>
    public string ToString() => Value.ToString() ?? string.Empty;

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code representing the value of this instance.</returns>
    int GetHashCode() => Value.GetHashCode();

    bool IEquatable<TPrimitive>.Equals(TPrimitive other) =>
            EqualityComparer<TValue>.Default.Equals(Value, other.Value);
    int IComparable<TPrimitive>.CompareTo(TPrimitive other) =>
        Comparer<TValue>.Default.Compare(Value, other.Value);
    int IComparable.CompareTo(object? obj) =>
        obj is TPrimitive other ? CompareTo(other) : throw new ArgumentException("Invalid comparison type");
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) =>
        (Value as IFormattable)?.ToString(format, formatProvider) ?? Value.ToString() ?? string.Empty;
}

/// <summary>
/// Provides extension methods for working with types that implement the IPrimitive interface.
/// </summary>
/// <remarks>This static class contains extension methods designed to simplify common operations on primitive
/// wrapper types. These methods can be used to enhance readability and reduce boilerplate when handling custom
/// primitive types throughout an application.</remarks>
public static class PrimitiveExtensions
{
    extension<TPrimitive, TValue>(IPrimitive<TPrimitive, TValue> primitive)
        where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
        where TValue : notnull
    {
        /// <summary>
        /// Gets a value indicating whether the current value is equal to the default value for the type.
        /// </summary>
        public bool IsEmpty => EqualityComparer<TValue>.Default.Equals(primitive.Value, TPrimitive.DefaultValue);
    }
}

/// <summary>
/// Specifies that the target struct should use a custom JSON converter for serialization and deserialization of
/// primitive value objects.
/// </summary>
/// <remarks>Apply this attribute to a struct to enable automatic JSON conversion using the associated primitive
/// converter. This is typically used to ensure that value objects are serialized and deserialized as their underlying
/// primitive values in JSON payloads.</remarks>
/// <typeparam name="TPrimitive">The struct type that implements the primitive value object pattern and the IPrimitive interface.</typeparam>
/// <typeparam name="TValue">The underlying value type represented by the primitive value object. Must be non-nullable.</typeparam>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class PrimitiveJsonConverterAttribute<TPrimitive, TValue> : JsonConverterAttribute
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new PrimitiveJsonConverter<TPrimitive, TValue>();
    }
}

/// <summary>
/// Provides a custom JSON converter for value objects that implement the IPrimitive interface, enabling serialization
/// and deserialization of primitive-backed types using their underlying value representation.
/// </summary>
/// <remarks>This converter allows value objects that wrap primitive types to be serialized and deserialized as
/// their underlying value, rather than as complex objects. It is typically used to simplify JSON representations of
/// strongly-typed primitives in APIs or data contracts.</remarks>
/// <typeparam name="TPrimitive">The value object type to convert. Must be a struct implementing <see cref="IPrimitive{TPrimitive, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The underlying value type used by the primitive. Must be a non-nullable type.</typeparam>
public sealed class PrimitiveJsonConverter<TPrimitive, TValue> : JsonConverter<TPrimitive>
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    public override TPrimitive Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TValue? value = (TValue?)JsonSerializer.Deserialize(ref reader, typeof(TValue), PrimitiveJsonContext.Default)
            ?? throw new JsonException($"Unable to convert null to {typeof(TPrimitive)}.");

        return TPrimitive.Create(value);
    }
    public override void Write(Utf8JsonWriter writer, TPrimitive value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, typeof(TValue), PrimitiveJsonContext.Default);
    }
}

/// <summary>
/// Provides a source-generated context for serializing and deserializing primitive .NET types using System.Text.Json.
/// </summary>
/// <remarks>This context enables efficient JSON serialization and deserialization for commonly used primitive
/// types, including strings, numeric types, booleans, and several date/time types. The context is configured to use
/// indented formatting, camel case property naming, and to ignore default values when writing JSON. It also supports
/// case-insensitive property names, string enum conversion, and allows trailing commas in JSON input. Use this context
/// with System.Text.Json APIs to benefit from improved performance and reduced runtime reflection when working with
/// supported types.</remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(TimeSpan))]
public partial class PrimitiveJsonContext : JsonSerializerContext { }

#pragma warning restore CA1000 // Do not declare static members on generic types
#pragma warning restore CA1033 // Interface methods should be callable by child types
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA2225 // Operator overloads have named alternates