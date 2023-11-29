
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Extensions;

/// <summary>
/// Provides with methods to extend use of <see cref="object"/>.
/// </summary>
public static partial class XpandablesExtensions
{
    /// <summary>
    /// Returns the name of the current type.
    /// </summary>
    /// <param name="obj">The target object to act on.</param>
    /// <returns>A string that represents the name of the target object.</returns>
    public static string GetTypeName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        return obj.GetType().GetNameWithoutGenericArity();
    }

    /// <summary>
    /// Returns the full name of the current type.
    /// </summary>
    /// <param name="obj">The target object to act on.</param>
    /// <returns>A string that represents the full name of the target object.</returns>
    /// <exception cref="ArgumentException">Cannot get the full name of a generic type parameter.</exception>
    public static string GetTypeFullName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        if (obj.GetType().IsGenericTypeParameter)
            throw new ArgumentException(
                "Cannot get the full name of a generic type parameter.",
                obj.GetType().Name);

        return obj.GetType().AssemblyQualifiedName!;
    }

    /// <summary>
    /// Casts the current object to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to be casted to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <returns>The casted object to <typeparamref name="T"/> type or null.</returns>
    public static T? As<T>(this object? obj) => obj is T t ? t : default;

    /// <summary>
    /// Casts the current object to the target type.
    /// </summary>
    /// <typeparam name="T">The type to be casted to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <param name="_">The target object to get its type.</param>
    /// <returns>The casted object to <typeparamref name="T"/> type or null.</returns>
    public static T? As<T>(this object? obj, T _) => obj is T t ? t : default;

    /// <summary>
    /// Casts the current object to the specified type or throws exception if not possible.
    /// </summary>
    /// <typeparam name="T">The type to be casted to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <returns>The casted object to <typeparamref name="T"/> type or <see cref="InvalidCastException"/>.</returns>
    public static T AsRequired<T>(this object obj) => (T)obj;

    /// <summary>
    ///  Returns an object of the specified type whose value is equivalent to the specified object. 
    ///  A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <param name="value">An object that implements the System.IConvertible interface.</param>
    /// <param name="conversionType">The type of object to return.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>An object whose type is conversionType and whose value is equivalent to value. 
    /// -or- value, if the System.Type of value and conversionType are equal. -or- A
    /// null reference (Nothing in Visual Basic), if value is null and conversionType
    /// is not a value type.</returns>
    /// <exception cref="ArgumentNullException">conversionType is null.</exception>
    /// <exception cref="InvalidCastException">This conversion is not supported. -or- value is null and conversionType is a 
    /// value type. -or- value does not implement the System.IConvertible interface.</exception>
    /// <exception cref="FormatException">value is not in a format for conversionType recognized by provider.</exception>
    /// <exception cref="OverflowException">value represents a number that is out of the range of conversionType.</exception>
    public static object? ChangeTypeNullable(
        this object? value,
        Type conversionType,
        IFormatProvider? formatProvider = default)
    {
        if (value is null) return null;

        Type targetType = conversionType ?? throw new ArgumentNullException(nameof(conversionType));
        if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            Type? underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType is null) return null;
            targetType = underlyingType;
        }

        return Convert.ChangeType(value, targetType, formatProvider);
    }

    /// <summary>
    ///  Returns an object of the specified type whose value is equivalent to the specified object. 
    ///  A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="T">The type of object to return.</typeparam>
    /// <param name="value">An object that implements the System.IConvertible interface.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>An object whose type is conversionType and whose value is equivalent to value. 
    /// -or- value, if the System.Type of value and conversionType are equal. -or- A
    /// null reference (Nothing in Visual Basic), if value is null and conversionType
    /// is not a value type.</returns>
    /// <exception cref="InvalidCastException">This conversion is not supported. -or- value is null and conversionType is a 
    /// value type. -or- value does not implement the System.IConvertible interface.</exception>
    /// <exception cref="FormatException">value is not in a format for conversionType recognized by provider.</exception>
    /// <exception cref="OverflowException">value represents a number that is out of the range of conversionType.</exception>
    [return: MaybeNull]
    public static T ChangeTypeNullable<T>(this object? value, IFormatProvider? formatProvider = default)
    {
        if (value is null) return default;

        return value.ChangeTypeNullable(typeof(T), formatProvider) is T instance
            ? instance : default;
    }

    /// <summary>
    /// Conditionally performs a function on an object if the condition is <see langword="true"/>.
    /// </summary>
    /// <param name="obj">The target object.</param>
    /// <param name="condition">The condition that should be <see langword="true"/> to apply the function.</param>
    /// <param name="func">The delegate to be executed only if the condition is <see langword="true"/>.</param>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <returns>The object modified by the function if condition is <see langword="true"/>, otherwise the original object.</returns>
    public static T When<T>(this T obj, bool condition, Func<T, T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        return condition ? func(obj) : obj;
    }

    /// <summary>
    /// Conditionally performs an action on an object if the condition is <see langword="true"/>.
    /// </summary>
    /// <param name="obj">The target object.</param>
    /// <param name="condition">The condition that should be <see langword="true"/> to apply the action.</param>
    /// <param name="action">The delegate to be executed only if the condition is <see langword="true"/>.</param>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <returns>The object modified by the function if condition is <see langword="true"/>, otherwise the original object.</returns>
    public static T When<T>(this T obj, bool condition, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (condition)
            action(obj);

        return obj;
    }

}
