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
namespace System.OperationResults;

/// <summary>
/// Provides extension methods for executing delegates and tasks, capturing their results and exceptions in an
/// OperationResult object.
/// </summary>
/// <remarks>These extension methods simplify error handling by wrapping delegate and task operation in an
/// OperationResult, which encapsulates success, failure, and exception information. This approach enables consistent
/// handling of operation outcomes without the need for explicit try-catch blocks. The methods support both synchronous
/// and asynchronous operations, and are designed to work with delegates and tasks that may throw exceptions. For
/// asynchronous methods, exceptions are captured and converted to OperationResult instances, allowing callers to
/// inspect the result and any associated errors.</remarks>
public static class OperationResultDelegateExtensions
{
    /// <summary>
    /// Attempts to execute the specified action and returns an OperationResult indicating success or failure.
    /// </summary>
    /// <param name="action">The action to execute. Cannot be null.</param>
    /// <returns>An OperationResult that represents the outcome of the action. Indicates whether the action completed
    /// successfully or an error occurred.</returns>
    public static OperationResult Try(this Action action)
    {
        return Try(() => action());
    }

    /// <summary>
    /// Attempts to execute the specified function and returns an OperationResult containing the outcome and result
    /// value.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the function.</typeparam>
    /// <param name="func">The function to execute. Cannot be null.</param>
    /// <returns>An <see cref="OperationResult{TResult}"/> that contains the result of the function operation and information about success or
    /// failure.</returns>
    public static OperationResult<TResult> Try<TResult>(this Func<TResult> func)
    {
        return Try(func);
    }

    /// <summary>
    /// Attempts to execute the specified action with the provided argument and returns an OperationResult indicating
    /// success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the argument to be passed to the action.</typeparam>
    /// <param name="action">The action to execute. Cannot be null.</param>
    /// <param name="args">The argument to pass to the action when it is invoked.</param>
    /// <returns>An OperationResult that represents the outcome of the attempted operation. Indicates whether the action
    /// completed successfully or an error occurred.</returns>
    public static OperationResult Try<T>(this Action<T> action, T args)
    {
        return Try(() => action(args));
    }

    /// <summary>
    /// Attempts to execute the specified asynchronous task and returns an OperationResult indicating the outcome.
    /// </summary>
    /// <remarks>If the task throws an ExecutionResultException, the associated OperationResult is returned.
    /// For other exceptions, the exception is converted to an OperationResult. This method enables consistent error
    /// handling for asynchronous operations.</remarks>
    /// <param name="task">The task to execute asynchronously. Cannot be null.</param>
    /// <returns>An OperationResult representing the result of the task operation. Returns an OperationResult with success status
    /// if the task completes successfully; otherwise, returns an OperationResult describing the error.</returns>
    public static async Task<OperationResult> TryAsync(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await task.ConfigureAwait(false);
            return OperationResult.Ok().Build();
        }
        catch (OperationResultException executionException)
        {
            return executionException.OperationResult;
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Attempts to execute the specified asynchronous task and returns an OperationResult containing the outcome,
    /// capturing both successful results and exceptions.
    /// </summary>
    /// <remarks>If the task completes successfully, the returned OperationResult contains the result. If the
    /// task throws an exception, the OperationResult encapsulates the exception information, allowing callers to handle
    /// errors without throwing exceptions. This method is useful for scenarios where exception handling should be
    /// managed through result objects rather than control flow.</remarks>
    /// <typeparam name="TResult">The type of the result produced by the asynchronous task.</typeparam>
    /// <param name="task">The task to execute asynchronously. Cannot be null.</param>
    /// <returns>An <see cref="OperationResult{TResult}"/> representing either the successful result of the task or details about any exception
    /// that occurred during operation.</returns>
    public static async Task<OperationResult<TResult>> TryAsync<TResult>(
        this Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            TResult result = await task.ConfigureAwait(false);
            return OperationResult
                .Ok(result)
                .Build();
        }
        catch (OperationResultException executionException)
        {
            return executionException.OperationResult.ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult().ToOperationResult<TResult>();
        }
    }
}
