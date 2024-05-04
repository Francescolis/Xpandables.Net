/*******************************************************************************
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
********************************************************************************/
using System.Text;

namespace Xpandables.Net;

/// <summary>
/// Provides a set of static methods for querying and 
/// manipulating instances of <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the
    /// the function throws an exception that is not an
    /// <see cref="InvalidOperationException"/> or an
    /// <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <exception cref="InvalidOperationException">The function throws an 
    /// exception that is not an <see cref="InvalidOperationException"/>
    /// .</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="func"/> 
    /// is null.</exception>"
    public static T ThrowInvalidOperationException<T>(
        this Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return func();
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException
                            and not ArgumentNullException)
        {
            throw new InvalidOperationException(
                exception.Message, exception);
        }
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the
    /// the function throws an exception that is not an
    /// <see cref="InvalidOperationException"/> or an
    /// <see cref="OperationCanceledException"/> or an
    /// <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <exception cref="InvalidOperationException">The function throws 
    /// an exception that is not an <see cref="InvalidOperationException"/> 
    /// or an <see cref="OperationCanceledException"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="func"/> 
    /// is null.</exception>"
    /// <exception cref="OperationCanceledException">The operation 
    /// was canceled.</exception>"
    public static async ValueTask<T> ThrowInvalidOperationException<T>(
        this ValueTask<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return await func.ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException
                            and not OperationCanceledException
                            and not ArgumentNullException)
        {
            throw new InvalidOperationException(
                exception.Message, exception);
        }
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the
    /// the function throws an exception that is not an
    /// <see cref="InvalidOperationException"/> or an
    /// <see cref="OperationCanceledException"/> or an
    /// <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <exception cref="InvalidOperationException">The function throws an 
    /// exception that is not an <see cref="InvalidOperationException"/> or an
    /// <see cref="OperationCanceledException"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="func"/> 
    /// is null.</exception>"
    /// <exception cref="OperationCanceledException">The operation 
    /// was canceled.</exception>"
    public static async ValueTask ThrowInvalidOperationException(
        this ValueTask func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            await func.ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException
                            and not OperationCanceledException
                            and not ArgumentNullException)
        {
            throw new InvalidOperationException(
                exception.Message, exception);
        }
    }

    /// <summary>
    /// Returns the full message of the exception including the inner 
    /// exceptions.
    /// </summary>
    /// <returns>The full message of the exception.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="exception"/>
    /// is null.</exception>
    public static string GetFullMessage(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        StringBuilder message = new();
        _ = message.AppendLine(exception.Message);

        while (exception.InnerException is not null)
        {
            exception = exception.InnerException;
            _ = message.AppendLine(exception.Message);
        }

        return message.ToString();
    }
}
