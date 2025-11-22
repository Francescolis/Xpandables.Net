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
/// Provides extension methods for enforcing InvalidOperationException as the standard exception type when executing
/// delegates or tasks, ensuring that any exception other than InvalidOperationException is wrapped and rethrown as
/// InvalidOperationException.
/// </summary>
/// <remarks>These methods are useful for scenarios where consistent exception handling is required, particularly
/// when consumers expect only InvalidOperationException to be thrown. They can be applied to synchronous delegates or
/// asynchronous tasks to standardize error propagation. All methods throw InvalidOperationException if the underlying
/// operation throws any exception other than InvalidOperationException. ArgumentNullException is thrown if a null
/// delegate or task is provided.</remarks>
public static class ThrowExceptionExtensions
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
        ArgumentNullException.ThrowIfNull(func);

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
        ArgumentNullException.ThrowIfNull(func);

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
        ArgumentNullException.ThrowIfNull(func);

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
}
