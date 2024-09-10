
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
namespace Xpandables.Net;

/// <summary>
/// Provides a set of <see langword="static"/> methods for tasks.
/// </summary>
public static class TaskExtensions
{
    private static readonly TaskFactory TaskFactory
        = new(CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

    /// <summary>
    /// Executes an async Task method which has a void return value synchronously
    /// <para>Use : AsyncExtensions.RunSync(() => AsyncMethod());</para>
    /// </summary>
    /// <param name="task">Task method to execute</param>
    public static void RunSync(this Func<Task> task) => TaskFactory
            .StartNew(task)
            .Unwrap()
            .GetAwaiter()
            .GetResult();

    /// <summary>
    /// Executes an async Task{T} method which has a T return type synchronously
    /// <para>Use : T result = AsyncExtensions.RunSync(() => AsyncMethod{T}());</para>
    /// </summary>
    /// <typeparam name="TResult">Return Type</typeparam>
    /// <param name="task">Task{T} method to execute</param>
    /// <returns></returns>
    public static TResult RunSync<TResult>(this Func<Task<TResult>> task) => TaskFactory
            .StartNew(task)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
}
