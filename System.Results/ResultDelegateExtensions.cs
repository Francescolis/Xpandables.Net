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
namespace System.Results;

/// <summary>
/// Provides extension methods for executing delegates and tasks, capturing their results and exceptions in an
/// Result object.
/// </summary>
/// <remarks>These extension methods simplify error handling by wrapping delegate and task operation in an
/// Result, which encapsulates success, failure, and exception information. This approach enables consistent
/// handling of operation outcomes without the need for explicit try-catch blocks. The methods support both synchronous
/// and asynchronous operations, and are designed to work with delegates and tasks that may throw exceptions. For
/// asynchronous methods, exceptions are captured and converted to Result instances, allowing callers to
/// inspect the result and any associated errors.</remarks>
public static class ResultDelegateExtensions
{
    /// <summary>
    /// Attempts to execute the specified action and returns a Result indicating success or failure.
    /// </summary>
    /// <param name="action">The action to execute. Cannot be null.</param>
    /// <returns>A Result that represents the outcome of the action. Indicates whether the action completed
    /// successfully or an error occurred.</returns>
    public static Result Try(this Action action)
    {
        return Try(() => action());
    }

    /// <summary>
    /// Attempts to execute the specified function and returns a Result containing the outcome and result
    /// value.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the function.</typeparam>
    /// <param name="func">The function to execute. Cannot be null.</param>
    /// <returns>An <see cref="Result{TValue}"/> that contains the result of the function operation and information about success or
    /// failure.</returns>
    public static Result<TResult> Try<TResult>(this Func<TResult> func)
    {
        return Try(func);
    }

    /// <summary>
    /// Attempts to execute the specified action with the provided argument and returns a Result indicating
    /// success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the argument to be passed to the action.</typeparam>
    /// <param name="action">The action to execute. Cannot be null.</param>
    /// <param name="args">The argument to pass to the action when it is invoked.</param>
    /// <returns>A Result that represents the outcome of the attempted operation. Indicates whether the action
    /// completed successfully or an error occurred.</returns>
    public static Result Try<T>(this Action<T> action, T args)
    {
        return Try(() => action(args));
    }

    /// <summary>
    /// Attempts to execute the specified asynchronous task and returns an Result indicating the outcome.
    /// </summary>
    /// <remarks>If the task throws an ResultException, the associated Result is returned.
    /// For other exceptions, the exception is converted to an Result. This method enables consistent error
    /// handling for asynchronous operations.</remarks>
    /// <param name="task">The task to execute asynchronously. Cannot be null.</param>
    /// <returns>An Result representing the result of the task operation. Returns an Result with success status
    /// if the task completes successfully; otherwise, returns an Result describing the error.</returns>
    public static async Task<Result> TryAsync(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await task.ConfigureAwait(false);
            return Result.Success();
        }
        catch (ResultException executionException)
        {
            return executionException.Result;
        }
        catch (Exception exception)
            when (exception is not ResultException)
        {
            return exception.ToFailureResult();
        }
    }

    /// <summary>
    /// Attempts to execute the specified asynchronous task and returns an Result containing the outcome,
    /// capturing both successful results and exceptions.
    /// </summary>
    /// <remarks>If the task completes successfully, the returned Result contains the result. If the
    /// task throws an exception, the Result encapsulates the exception information, allowing callers to handle
    /// errors without throwing exceptions. This method is useful for scenarios where exception handling should be
    /// managed through result objects rather than control flow.</remarks>
    /// <typeparam name="TValue">The type of the value produced by the asynchronous task.</typeparam>
    /// <param name="task">The task to execute asynchronously. Cannot be null.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either the successful result of the task or details about any exception
    /// that occurred during operation.</returns>
    public static async Task<Result<TValue>> TryAsync<TValue>(
        this Task<TValue> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            TValue result = await task.ConfigureAwait(false);
            return Result.Success(result);
        }
        catch (ResultException executionException)
        {
            return executionException.Result;
        }
        catch (Exception exception)
            when (exception is not ResultException)
        {
            return exception.ToFailureResult();
        }
    }
}
