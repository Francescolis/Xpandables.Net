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
namespace Xpandables.Net.Operations;
public static partial class OperationResultExtensions
{
    /// <summary>
    /// Converts an <see cref="Action"/> to an <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>An <see cref="IOperationResult"/> representing the result of 
    /// the action.</returns>
    public static IOperationResult ToOperationResult(this Action action)
    {
        try
        {
            action();
            return OperationResults.Ok().Build();
        }
        catch (Exception exception)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Task"/> to an <see cref="IOperationResult"/>
    /// asynchronously.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{IOperationResult}"/> representing the result
    /// of the task.</returns>
    public static async Task<IOperationResult> ToOperationResultAsync(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
            return OperationResults.Ok().Build();
        }
        catch (Exception exception)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Task{TResult}"/> to an 
    /// <see cref="IOperationResult{TResult}"/>
    /// asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{T}"/> representing the result of the task.</returns>
    public static async Task<IOperationResult<TResult>> ToOperationResultAsync<TResult>(
        this Task<TResult> task)
    {
        try
        {
            TResult result = await task.ConfigureAwait(false);
            return OperationResults
                .Ok(result)
                .Build();
        }
        catch (Exception exception)
        {
            return exception
                .ToOperationResult()
                .ToOperationResult<TResult>();
        }
    }

    /// <summary>
    /// Converts a <see cref="Func{TResult}"/> to an 
    /// <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the 
    /// function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>An <see cref="IOperationResult{TResult}"/> representing the 
    /// result of the function.</returns>
    public static IOperationResult<TResult> ToOperationResult<TResult>(
        this Func<TResult> func)
    {
        try
        {
            TResult result = func();
            return OperationResults
                .Ok(result)
                .Build();
        }
        catch (Exception exception)
        {
            return exception
                .ToOperationResult()
                .ToOperationResult<TResult>();
        }
    }
}
