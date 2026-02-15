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
using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;

namespace System;

/// <summary>
/// Provides extension methods for performing null checks, type conversions, and safe casting operations on objects.
/// </summary>
/// <remarks>This static class offers a set of utility methods that simplify common object operations, such as
/// determining nullability, converting objects to nullable types, and safely casting to reference types. The methods
/// are designed to reduce boilerplate code and improve readability when working with objects whose types or values may
/// not be known at compile time.</remarks>
public static class ObjectExtensions
{
    extension(object? obj)
    {
        /// <summary>
        /// Determines whether the current object is null.
        /// </summary>
        /// <returns><c>true</c> if the current object is null; otherwise, <c>false</c>.</returns>
        public bool IsNull => obj is null;

        /// <summary>
        /// Converts the current object to the specified nullable type, using the provided format information if
        /// applicable. Returns null if the object is null or if the conversion cannot be performed.
        /// </summary>
        /// <remarks>If the target type is a nullable value type, the conversion is performed using its
        /// underlying type. This method uses <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> for the
        /// conversion operation.</remarks>
        /// <param name="conversionType">The target nullable type to convert the object to. Must be a valid type; cannot be null.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information for the conversion, or null to use the
        /// current culture.</param>
        /// <returns>An object of the specified nullable type representing the converted value, or null if the input object is
        /// null or the conversion is not possible.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="conversionType"/> is null.</exception>
        [RequiresDynamicCode("Calls System.ObjectExtensions.TryConvertEnumerable(Object, Type, IFormatProvider, out Object)")]
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize(String, Type, JsonSerializerOptions)")]
        public object? ChangeTypeNullable([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type conversionType, IFormatProvider? formatProvider = null)
        {
            ArgumentNullException.ThrowIfNull(conversionType);

            if (obj is null || obj is DBNull)
                return null;

            var sourceType = obj.GetType();

            // Fast path: already the exact type or directly assignable
            if (sourceType == conversionType || conversionType.IsAssignableFrom(sourceType))
                return obj;

            if (Nullable.GetUnderlyingType(conversionType) is Type underlying)
                return ChangeTypeNullable(obj, underlying, formatProvider);

            // Fast path: primitive-to-primitive via Convert.ChangeType
            if (sourceType.IsPrimitive && conversionType.IsPrimitive)
                return Convert.ChangeType(obj, conversionType, formatProvider);

            if (TryConvertKeyValuePair(obj, conversionType, formatProvider, out var kvpResult))
                return kvpResult;

            if (TryConvertImmutableArray(obj, conversionType, formatProvider, out var immutableArrayResult))
                return immutableArrayResult;

            if (TryConvertImmutableCollection(obj, conversionType, formatProvider, out var immutableResult))
                return immutableResult;

            if (TryConvertEnumerable(obj, conversionType, formatProvider, out var enumerableResult))
                return enumerableResult;

            if (TryConvertDictionary(obj, conversionType, formatProvider, out var dictResult))
                return dictResult;

            if (TryConvertExpando(obj, conversionType, out var expandoResult))
                return expandoResult;

            if (TryConvertSpecialTypes(obj, conversionType, formatProvider, out var specialResult))
                return specialResult;

            if (TryConvertWithTypeConverter(obj, conversionType, out var convertedByTarget))
                return convertedByTarget;

            if (TryConvertWithSourceConverter(obj, conversionType, out var convertedBySource))
                return convertedBySource;

            return Convert.ChangeType(obj, conversionType, formatProvider);
        }

        /// <summary>
        /// Attempts to convert the underlying object to the specified nullable type.
        /// </summary>
        /// <remarks>If the underlying object is <see langword="null"/>, the method returns <see
        /// langword="null"/>. This method is useful for safely attempting type conversions where the source value may
        /// be <see langword="null"/> or not convertible to the target type.</remarks>
        /// <typeparam name="T">The type to convert the object to. Must be a value type or a reference type that supports conversion.</typeparam>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information for the conversion, or <see
        /// langword="null"/> to use the current culture.</param>
        /// <returns>A value of type <typeparamref name="T"/> if the conversion succeeds; otherwise, <see langword="null"/>.</returns>
        [RequiresDynamicCode("Calls System.ObjectExtensions.TryConvertEnumerable(Object, Type, IFormatProvider, out Object)")]
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize(String, Type, JsonSerializerOptions)")]
        public T? ChangeTypeNullable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IFormatProvider? formatProvider = null)
        {
            if (obj is null)
            {
                return default;
            }

            return obj.ChangeTypeNullable(typeof(T), formatProvider) is T instance
                ? instance : default;
        }

        /// <summary>
        /// Attempts to cast the underlying object to the specified reference type, returning the result if successful.
        /// </summary>
        /// <typeparam name="T">The reference type to which to cast the object.</typeparam>
        /// <returns>An instance of type T if the object is of the specified type; otherwise, null.</returns>
        public T? As<T>() where T : class => obj is T t ? t : null;

        /// <summary>
        /// Attempts to cast the underlying object to the specified reference type, returning the result if successful;
        /// otherwise, returns null.
        /// </summary>
        /// <remarks>This method provides a type-safe way to attempt casting the underlying object to a
        /// specified reference type. If the object is not of the requested type, the method returns null rather than
        /// throwing an exception.</remarks>
        /// <typeparam name="T">The reference type to which to cast the underlying object.</typeparam>
        /// <param name="_">A dummy parameter used to infer the target type. The value is ignored.</param>
        /// <returns>The underlying object cast to type T if it is compatible; otherwise, null.</returns>
        public T? As<T>(T? _) where T : class => obj is T t ? t : null;

        /// <summary>
        /// Casts the current object to the specified reference type, throwing an exception if the object is null.
        /// </summary>
        /// <typeparam name="T">The reference type to which the object will be cast.</typeparam>
        /// <returns>The current object cast to type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the current object is null.</exception>
        /// <exception cref="InvalidCastException">Thrown if the current object cannot be cast to type <typeparamref name="T"/>.</exception>
        public T AsRequired<T>() where T : class
        {
            ArgumentNullException.ThrowIfNull(obj);
            return (T)obj;
        }
    }

    extension<T>(T obj)
    {
        /// <summary>
        /// Invokes the specified action on the object if the given condition is <see langword="true"/>, and returns the
        /// object.
        /// </summary>
        /// <remarks>This method enables conditional execution of an action within a fluent interface. The
        /// object is always returned, regardless of whether the action was executed.</remarks>
        /// <param name="condition">A value indicating whether the action should be executed. If <see langword="true"/>, the action is invoked;
        /// otherwise, it is skipped.</param>
        /// <param name="action">The action to perform on the object if the condition is <see langword="true"/>. Cannot be null.</param>
        /// <returns>The object on which the action may have been performed.</returns>
        public T When(bool condition, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(obj);

            if (condition)
            {
                action(obj);
            }

            return obj;
        }

        /// <summary>
        /// Applies the specified function to the current object if the given condition is <see langword="true"/>;
        /// otherwise, returns the object unchanged.
        /// </summary>
        /// <param name="condition">A value indicating whether to apply the function to the object. If <see langword="true"/>, the function is
        /// applied; otherwise, the object is returned as is.</param>
        /// <param name="func">The function to apply to the object when <paramref name="condition"/> is <see langword="true"/>. Cannot be
        /// null.</param>
        /// <returns>The result of applying <paramref name="func"/> to the object if <paramref name="condition"/> is <see
        /// langword="true"/>; otherwise, the original object.</returns>
        public T When(bool condition, Func<T, T> func)
        {
            ArgumentNullException.ThrowIfNull(func);
            ArgumentNullException.ThrowIfNull(obj);

            return condition ? func(obj) : obj;
        }
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize(String, Type, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize(String, Type, JsonSerializerOptions)")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private static bool TryConvertSpecialTypes(object obj, Type targetType, IFormatProvider? formatProvider, out object? result)
    {
        result = null;

        if (targetType == typeof(Guid) && obj is string g)
        {
            result = Guid.Parse(g);
            return true;
        }

        if (targetType == typeof(decimal))
        {
            if (obj is string decString)
            {
                result = decimal.Parse(decString, NumberStyles.Number, formatProvider);
                return true;
            }

            if (obj is double d)
            {
                result = Convert.ToDecimal(d, formatProvider);
                return true;
            }

            if (obj is float f)
            {
                result = Convert.ToDecimal(f, formatProvider);
                return true;
            }
        }

        if (targetType == typeof(int))
        {
            if (obj is string intString)
            {
                result = int.Parse(intString, NumberStyles.Integer, formatProvider);
                return true;
            }
        }

        if (targetType == typeof(DateTimeOffset) && obj is string dto)
        {
            result = DateTimeOffset.Parse(dto, formatProvider);
            return true;
        }

        if (targetType == typeof(long))
        {
            if (obj is string longString)
            {
                result = long.Parse(longString, NumberStyles.Integer, formatProvider);
                return true;
            }
        }

        if (targetType == typeof(TimeSpan) && obj is string ts)
        {
            result = TimeSpan.Parse(ts, formatProvider);
            return true;
        }

        if (targetType == typeof(Uri) && obj is string uri)
        {
            result = new Uri(uri);
            return true;
        }

        if (targetType == typeof(Version) && obj is string ver)
        {
            result = Version.Parse(ver);
            return true;
        }

        if (targetType.IsEnum && obj is string es)
        {
            result = Enum.Parse(targetType, es, ignoreCase: true);
            return true;
        }

        // DateOnly conversion
        if (targetType == typeof(DateOnly) && obj is string dateOnlyString)
        {
            result = DateOnly.Parse(dateOnlyString, formatProvider);
            return true;
        }

        // TimeOnly conversion
        if (targetType == typeof(TimeOnly) && obj is string timeOnlyString)
        {
            result = TimeOnly.Parse(timeOnlyString, formatProvider);
            return true;
        }

        // JSON conversion (string → object)
        if (obj is string json && LooksLikeJson(json))
        {
            try
            {
                result = Text.Json.JsonSerializer.Deserialize(json, targetType);
                return true;
            }
            catch
            {
                // fall through to other converters
            }
        }

        if (targetType == typeof(Text.Json.JsonDocument) && obj is string jsonString)
        {
            result = Text.Json.JsonDocument.Parse(jsonString);
            return true;
        }

        return false;
    }

    [RequiresUnreferencedCode("Calls System.ComponentModel.TypeDescriptor.GetConverter(Type)")]
    private static bool TryConvertWithTypeConverter(object obj, Type targetType, out object? result)
    {
        result = null;

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter is null || !converter.CanConvertFrom(obj.GetType()))
            return false;

        result = converter.ConvertFrom(null, CultureInfo.CurrentCulture, obj);
        return true;
    }

    [RequiresUnreferencedCode("Calls System.ComponentModel.TypeDescriptor.GetConverter(Type)")]
    private static bool TryConvertWithSourceConverter(object obj, Type targetType, out object? result)
    {
        result = null;

        var converter = TypeDescriptor.GetConverter(obj.GetType());
        if (converter is null || !converter.CanConvertTo(targetType))
            return false;

        result = converter.ConvertTo(null, CultureInfo.CurrentCulture, obj, targetType);
        return true;
    }

    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [RequiresUnreferencedCode("Calls System.ObjectExtensions.ChangeTypeNullable(Object, Type, IFormatProvider)")]
    private static bool TryConvertEnumerable(object obj, Type conversionType, IFormatProvider? formatProvider, out object? result)
    {
        result = null;

        if (obj is not string s)
            return false;

        // Detect separators
        var separators = new[] { ',', ';', ':' };
        if (!separators.Any(s.Contains))
            return false;

        // Determine element type
        Type? elementType = null;

        if (conversionType.IsArray)
            elementType = conversionType.GetElementType();
        else if (conversionType.IsGenericType &&
                 typeof(IEnumerable<>).IsAssignableFrom(conversionType.GetGenericTypeDefinition()))
            elementType = conversionType.GetGenericArguments()[0];

        if (elementType is null)
            return false;

        // Split and convert each element
        var parts = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            var converted = ChangeTypeNullable(trimmed, elementType, formatProvider);
            list.Add(converted);
        }

        // Convert to final type if needed
        if (conversionType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            result = array;
            return true;
        }

        if (conversionType.IsAssignableFrom(list.GetType()))
        {
            result = list;
            return true;
        }

        // Try to convert List<T> → target collection type
        try
        {
            result = Convert.ChangeType(list, conversionType, formatProvider);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool LooksLikeJson(string s)
    {
        s = s.Trim();
        return (s.StartsWith('{') && s.EndsWith('}')) ||
               (s.StartsWith('[') && s.EndsWith(']'));
    }

    [RequiresDynamicCode("Calls System.ObjectExtensions.TryConvertEnumerable(Object, Type, IFormatProvider, out Object)")]
    [RequiresUnreferencedCode("Calls System.ObjectExtensions.TryConvertEnumerable(Object, Type, IFormatProvider, out Object)")]
    private static bool TryConvertImmutableCollection(object obj, Type conversionType, IFormatProvider? formatProvider, out object? result)
    {
        result = null;

        if (!conversionType.IsGenericType)
            return false;

        var gen = conversionType.GetGenericTypeDefinition();
        var args = conversionType.GetGenericArguments();

        // ImmutableList<T>
        if (gen == typeof(ImmutableList<>))
        {
            if (!TryConvertEnumerable(obj, typeof(IEnumerable<>).MakeGenericType(args[0]), formatProvider, out var enumerable))
                return false;

            var method = typeof(ImmutableList)
                .GetMethod("CreateRange", [typeof(IEnumerable<>).MakeGenericType(args[0])])!
                .MakeGenericMethod(args[0]);

            result = method.Invoke(null, [enumerable]);
            return true;
        }

        // ImmutableHashSet<T>
        if (gen == typeof(ImmutableHashSet<>))
        {
            if (!TryConvertEnumerable(obj, typeof(IEnumerable<>).MakeGenericType(args[0]), formatProvider, out var enumerable))
                return false;

            var method = typeof(ImmutableHashSet)
                .GetMethod("CreateRange", [typeof(IEnumerable<>).MakeGenericType(args[0])])!
                .MakeGenericMethod(args[0]);

            result = method.Invoke(null, [enumerable]);
            return true;
        }

        // ImmutableDictionary<TKey,TValue>
        if (gen == typeof(ImmutableDictionary<,>))
        {
            if (!TryConvertDictionary(obj, conversionType, formatProvider, out var dictObj))
                return false;

            var dict = (IDictionary)dictObj!;

            var createRange = typeof(ImmutableDictionary)
                .GetMethod("CreateRange", [conversionType])!
                .MakeGenericMethod(args);

            result = createRange.Invoke(null, new[] { dict });
            return true;
        }

        return false;
    }

    [RequiresDynamicCode("Calls System.ObjectExtensions.TryConvertEnumerable(Object, Type, IFormatProvider, out Object)")]
    [RequiresUnreferencedCode("Calls System.ObjectExtensions.TryConvertEnumerable(Object, Type, IFormatProvider, out Object)")]
    private static bool TryConvertImmutableArray(object obj, Type conversionType, IFormatProvider? formatProvider, out object? result)
    {
        result = null;

        if (!conversionType.IsGenericType ||
            conversionType.GetGenericTypeDefinition() != typeof(ImmutableArray<>))
            return false;

        var elementType = conversionType.GetGenericArguments()[0];

        // Convert string → IEnumerable<T>
        if (!TryConvertEnumerable(obj, typeof(IEnumerable<>).MakeGenericType(elementType), formatProvider, out var enumerableObj))
            return false;

        var enumerable = (IEnumerable)enumerableObj!;

        // Build ImmutableArray<T>
        var builderType = typeof(ImmutableArray).GetMethod("CreateBuilder", [typeof(int)])!
            .MakeGenericMethod(elementType);

        var builder = builderType.Invoke(null, [0])!;

        var addMethod = builder.GetType().GetMethod("Add")!;

        foreach (var item in enumerable)
            addMethod.Invoke(builder, [item]);

        var toImmutableMethod = builder.GetType().GetMethod("ToImmutable")!;
        result = toImmutableMethod.Invoke(builder, null);

        return true;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private static bool TryConvertExpando(object obj, Type targetType, out object? result)
    {
        result = null;

        if (targetType != typeof(ExpandoObject))
            return false;

        if (obj is string json && LooksLikeJson(json))
        {
            try
            {
                var dict = Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
                if (dict is null)
                    return false;

                var expando = new ExpandoObject();
                var expandoDict = (IDictionary<string, object?>)expando;

                foreach (var kv in dict)
                    expandoDict[kv.Key] = kv.Value;

                result = expando;
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    [RequiresUnreferencedCode("Calls System.ObjectExtensions.ChangeTypeNullable(Object, Type, IFormatProvider)")]
    [RequiresDynamicCode("Calls System.ObjectExtensions.ChangeTypeNullable(Object, Type, IFormatProvider)")]
    private static bool TryConvertKeyValuePair(object obj, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type targetType, IFormatProvider? formatProvider, out object? result)
    {
        result = null;

        if (obj is not string s)
            return false;

        if (!targetType.IsGenericType ||
            targetType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            return false;

        var args = targetType.GetGenericArguments();
        var keyType = args[0];
        var valueType = args[1];

        var parts = s.Split('=', 2);
        if (parts.Length != 2)
            return false;

        var key = ChangeTypeNullable(parts[0].Trim(), keyType, formatProvider);
        var value = ChangeTypeNullable(parts[1].Trim(), valueType, formatProvider);

        result = Activator.CreateInstance(targetType, key, value);
        return true;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize(String, Type, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize(String, Type, JsonSerializerOptions)")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private static bool TryConvertDictionary(object obj, Type targetType, IFormatProvider? formatProvider, out object? result)
    {
        result = null;

        if (obj is not string s)
            return false;

        // JSON dictionary
        if (LooksLikeJson(s))
        {
            try
            {
                result = Text.Json.JsonSerializer.Deserialize(s, targetType);
                return true;
            }
            catch { }
        }

        // Non-JSON dictionary: "key=value;key2=value2"
        if (!targetType.IsGenericType ||
            targetType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            return false;

        var args = targetType.GetGenericArguments();
        var keyType = args[0];
        var valueType = args[1];

        var dict = (IDictionary)Activator.CreateInstance(targetType)!;

        var pairs = s.Split([';', ','], StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var kv = pair.Split('=', 2);
            if (kv.Length != 2)
                continue;

            var key = ChangeTypeNullable(kv[0].Trim(), keyType, formatProvider);
            var value = ChangeTypeNullable(kv[1].Trim(), valueType, formatProvider);

            dict.Add(key!, value);
        }

        result = dict;
        return true;
    }
}
