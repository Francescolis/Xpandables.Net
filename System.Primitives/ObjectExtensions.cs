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
        public object? ChangeTypeNullable(Type conversionType, IFormatProvider? formatProvider = null)
        {
            if (obj is null)
            {
                return null;
            }

            Type targetType = conversionType
                ?? throw new ArgumentNullException(nameof(conversionType));

            if (!conversionType.IsGenericType
                || conversionType.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return TryConvertSpecialTypes(obj, targetType, formatProvider)
                    ?? Convert.ChangeType(obj, targetType, formatProvider);
            }

            Type? underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType is null)
            {
                return null;
            }

            targetType = underlyingType;

            return TryConvertSpecialTypes(obj, targetType, formatProvider)
                ?? Convert.ChangeType(obj, targetType, formatProvider);
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
        public T? ChangeTypeNullable<T>(IFormatProvider? formatProvider = null)
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

    private static object? TryConvertSpecialTypes(object obj, Type targetType, IFormatProvider? formatProvider)
    {
        // Guid conversion
        if (targetType == typeof(Guid) && obj is string guidString)
        {
            return Guid.Parse(guidString);
        }

        // TimeSpan conversion
        if (targetType == typeof(TimeSpan) && obj is string timeSpanString)
        {
            return TimeSpan.Parse(timeSpanString, formatProvider);
        }

        // Uri conversion
        if (targetType == typeof(Uri) && obj is string uriString)
        {
            return new Uri(uriString);
        }

        // Version conversion
        if (targetType == typeof(Version) && obj is string versionString)
        {
            return Version.Parse(versionString);
        }

        // Enum conversion
        if (targetType.IsEnum && obj is string enumString)
        {
            return Enum.Parse(targetType, enumString, ignoreCase: true);
        }

        return null;
    }
}
