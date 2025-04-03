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
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.Executions;

/// <summary>
/// Extension methods for converting actions and tasks to <see cref="ExecutionResult"/>.
/// </summary>
public static class ExecutionResultExecutionExtensions
{
    /// <summary>
    /// Converts an <see cref="Action"/> to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the result of 
    /// the action.</returns>
    public static ExecutionResult ToExecutionResult(this Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
            return ExecutionResults.Success();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts an <see cref="Action{T}"/> to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument passed to the action.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="args">The argument to pass to the action.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the result 
    /// of the action.</returns>
    public static ExecutionResult ToExecutionResult<T>(this Action<T> action, T args)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action(args);
            return ExecutionResults.Ok().Build();
        }
        catch (ValidationException validationException)
        {
            return validationException.ToExecutionResult();
        }
        catch (ExecutionResultException executionException)
        {
            return executionException.ExecutionResult;
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Task"/> to an <see cref="ExecutionResult"/>
    /// asynchronously.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{IOperationResult}"/> representing the result
    /// of the task.</returns>
    public static async Task<ExecutionResult> ToExecutionResultAsync(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await task.ConfigureAwait(false);
            return ExecutionResults.Ok().Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Task{TResult}"/> to an 
    /// <see cref="ExecutionResult{TResult}"/>
    /// asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{T}"/> representing the result of the task.</returns>
    public static async Task<ExecutionResult<TResult>> ToExecutionResultAsync<TResult>(
        this Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            TResult result = await task.ConfigureAwait(false);
            return ExecutionResults
                .Ok(result)
                .Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception
                .ToExecutionResult()
                .ToExecutionResult<TResult>();
        }
    }

    /// <summary>
    /// Converts a <see cref="Func{TResult}"/> to an 
    /// <see cref="ExecutionResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the 
    /// function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>An <see cref="ExecutionResult{TResult}"/> representing the 
    /// result of the function.</returns>
    public static ExecutionResult<TResult> ToExecutionResult<TResult>(this Func<TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            TResult result = func();
            return ExecutionResults
                .Ok(result)
                .Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception
                .ToExecutionResult()
                .ToExecutionResult<TResult>();
        }
    }
}
