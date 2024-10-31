/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Text;

namespace Xpandables.Net;

/// <summary>
/// Provides a set of helper extension methods.
/// </summary>
public static class HelperExtensions
{
    /// <summary>
    /// Throws an InvalidOperationException if the provided function throws an 
    /// exception that is not an InvalidOperationException.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the function 
    /// throws an exception that is not an InvalidOperationException.</exception>
    public static T ThrowInvalidOperationException<T>(this Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>
    /// Throws an InvalidOperationException if the provided Task throws an 
    /// exception that is not an InvalidOperationException.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the Task.</typeparam>
    /// <param name="func">The Task to execute.</param>
    /// <returns>The original Task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Task 
    /// throws an exception that is not an InvalidOperationException.</exception>
    public static Task<T> ThrowInvalidOperationException<T>(this Task<T> func)
    {
        try
        {
            return func;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>
    /// Throws an InvalidOperationException if the provided Task throws an 
    /// exception that is not an InvalidOperationException.
    /// </summary>
    /// <param name="func">The Task to execute.</param>
    /// <returns>The original Task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Task 
    /// throws an exception that is not an InvalidOperationException.</exception>
    public static Task ThrowInvalidOperationException(this Task func)
    {
        try
        {
            return func;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>
    /// Retrieves the full message from an exception, including all inner 
    /// exceptions.
    /// </summary>
    /// <param name="exception">The exception to get the full message from.</param>
    /// <returns>A string containing the full message of the exception and i
    /// ts inner exceptions.</returns>
    public static string GetFullMessage(this Exception exception)
    {
        StringBuilder message = new();
        _ = message.AppendLine(exception.Message);

        while (exception.InnerException is not null)
        {
            exception = exception.InnerException;
            _ = message.AppendLine(exception.Message);
        }

        return message.ToString();
    }

    /// <summary>
    /// Changes the type of the given value to the specified conversion type, 
    /// handling nullable types.
    /// </summary>
    /// <param name="value">The value to change the type of.</param>
    /// <param name="conversionType">The type to convert the value to.</param>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>The value converted to the specified type, or null if the 
    /// value is null.</returns>
    public static object? ChangeTypeNullable(
        this object? value,
        Type conversionType,
        IFormatProvider? formatProvider = default)
    {
        if (value is null)
        {
            return null;
        }

        Type targetType = conversionType
            ?? throw new ArgumentNullException(nameof(conversionType));

        if (!conversionType.IsGenericType
            || conversionType.GetGenericTypeDefinition() != typeof(Nullable<>))
        {
            return Convert.ChangeType(value, targetType, formatProvider);
        }

        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType is null)
        {
            return null;
        }

        targetType = underlyingType;

        return Convert.ChangeType(value, targetType, formatProvider);
    }

    /// <summary>
    /// Changes the type of the given value to the specified conversion type, 
    /// handling nullable types.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="value">The value to change the type of.</param>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>The value converted to the specified type, or null if the 
    /// value is null.</returns>
    public static T? ChangeTypeNullable<T>(
        this object? value,
        IFormatProvider? formatProvider = default)
    {
        if (value is null)
        {
            return default;
        }

        return value.ChangeTypeNullable(typeof(T), formatProvider) is T instance
            ? instance : default;
    }

    /// <summary>
    /// Executes the provided function if the condition is true, otherwise 
    /// returns the original object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to potentially modify.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="func">The function to execute if the condition is true.</param>
    /// <returns>The modified object if the condition is true, otherwise the 
    /// original object.</returns>
    public static T When<T>(this T obj, bool condition, Func<T, T> func) =>
        condition ? func(obj) : obj;

    /// <summary>
    /// Executes the provided action if the condition is true, otherwise 
    /// returns the original object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to potentially modify.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="action">The action to execute if the condition is true.</param>
    /// <returns>The original object.</returns>
    public static T When<T>(this T obj, bool condition, Action<T> action)
    {
        if (condition)
        {
            action(obj);
        }

        return obj;
    }
}
