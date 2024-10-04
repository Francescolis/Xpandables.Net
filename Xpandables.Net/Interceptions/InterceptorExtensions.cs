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
namespace Xpandables.Net.Interceptions;

/// <summary>
/// A collection of extension methods for the <see cref="IInterceptor"/> 
/// interface.
/// </summary>
public static class InterceptorExtensions
{
    /// <summary>
    /// Returns the real instance of the invocation target.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <returns>The real instance of the invocation target.</returns>
    public static object GetRealInstance(this IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        object target = invocation.Target;

        while (target is InterceptorProxy proxy)
        {
            target = proxy.Instance;
        }

        return target;
    }

    /// <summary>
    /// Gets the real return value of the invocation.
    /// </summary>
    /// <param name="invocation">The method argument to be called.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="invocation"/>
    /// is null.</exception>
    /// <returns>The real return value of the invocation.</returns>
    public static object? GetRealReturnValue(this IInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);

        dynamic? awaitable = invocation.ReturnValue;
        if (awaitable is null)
        {
            return null;
        }

        Type returnType = invocation.Method.ReturnType;
        bool isAsync = returnType.IsTaskType();

        if (isAsync)
        {
            Task task = awaitable;
            task.Wait();
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        return awaitable;
    }

    /// <summary>
    /// Determines whether the type is a task type.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> 
    /// is null.</exception>
    /// <returns><see langword="true"/> if the type is a task type; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsTaskType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type == typeof(Task)
            || (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
