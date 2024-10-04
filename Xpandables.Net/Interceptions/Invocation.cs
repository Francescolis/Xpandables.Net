
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
using System.Diagnostics;
using System.Reflection;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// Provides the implementation of the <see cref="IInvocation" /> interface.
/// </summary>
internal record class Invocation : IInvocation
{
    public MethodInfo Method { get; }
    public object Target { get; }
    public Type InterfaceType { get; }
    public IParameterCollection Arguments { get; }
    public Exception? Exception { get; internal set; }
    public bool ReThrowException { get; set; }
    public Type ReturnType => Method.ReturnType;
    public object? ReturnValue { get; internal set; }
    public TimeSpan ElapsedTime { get; internal set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Invocation"/> with 
    /// the arguments needed for invocation.
    /// </summary>
    /// <param name="targetMethod">The target method.</param>
    /// <param name="targetInstance">The target instance being called.</param>
    /// <param name="interfaceType">The interface type.</param>
    /// <param name="argsValue">Arguments for the method, if necessary.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="targetMethod"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="targetInstance"/> is null.</exception>
    internal Invocation(
        MethodInfo targetMethod,
        object targetInstance,
        Type interfaceType,
        params object?[]? argsValue)
    {
        Method = targetMethod
            ?? throw new ArgumentNullException(nameof(targetMethod));
        Target = targetInstance
            ?? throw new ArgumentNullException(nameof(targetInstance));
        InterfaceType = interfaceType;
        Arguments = new ParameterCollection(targetMethod, argsValue);
    }

    public void SetException(Exception? exception) => Exception = exception;

    public void SetReturnValue<T>(T? returnValue)
    {
        SetException(null);

        ReturnValue = returnValue;

        if (ReturnValue is null)
        {
            return;
        }

        // If it's an async method, check for Task, Task<T>, ValueTask, and ValueTask<T>
        if (typeof(Task).IsAssignableFrom(ReturnType))
        {
            Task task = (Task)ReturnValue;

            if (task.Status != TaskStatus.RanToCompletion)
            {
                task.ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (ReturnType.IsGenericType)
            {
                Type argumentType = ReturnType.GetGenericArguments()[0];

                if (ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    MethodInfo fromResultMethod = typeof(Task)
                        .GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(argumentType);

                    object? result = task.GetType().GetProperty("Result")!.GetValue(task);
                    ReturnValue = fromResultMethod.Invoke(null, [result]);
                }
            }
            else
            {
                ReturnValue = Task.CompletedTask;
            }
        }
        else if (typeof(ValueTask).IsAssignableFrom(ReturnType))
        {
            ValueTask valueTask = (ValueTask)ReturnValue;
            valueTask.ConfigureAwait(false).GetAwaiter().GetResult();

            if (ReturnType.IsGenericType)
            {
                Type argumentType = ReturnType.GetGenericArguments()[0];

                if (ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    Type genericValueTaskType = typeof(ValueTask<>)
                        .MakeGenericType(argumentType);

                    ConstructorInfo constructorInfo = genericValueTaskType
                        .GetConstructor([argumentType])!;

                    object? result = valueTask.GetType().GetProperty("Result")!.GetValue(valueTask);
                    ReturnValue = constructorInfo.Invoke([result]);
                }
            }
            else
            {
                ReturnValue = ValueTask.CompletedTask;
            }
        }
    }

    public void SetElapsedTime(TimeSpan elapsedTime) => ElapsedTime = elapsedTime;

    public void Proceed()
    {
        Stopwatch watch = Stopwatch.StartNew();

        try
        {
            object? result = Method
                .Invoke(
                    Target,
                    Arguments.Select(arg => arg.Value).ToArray());

            SetReturnValue(result);
        }
        catch (TargetInvocationException exception)
        {
            SetException(exception.InnerException);
            if (ReThrowException)
            {
                throw;
            }
        }
        catch (Exception exception)
        {
            SetException(exception);
            if (ReThrowException)
            {
                throw;
            }
        }
        finally
        {
            watch.Stop();
            SetElapsedTime(watch.Elapsed);
        }
    }
    private static object? HandleTask<T>(T? result)
    {
        if (result is Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();

            if (task.GetType().IsGenericType)
            {
                PropertyInfo property = task.GetType().GetProperty("Result")!;
                object? value = property.GetValue(task)!;
                return Task.FromResult(value);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        return result;
    }

    private static object? HandleValueTask<T>(T? result)
    {
        if (result is ValueTask task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
            if (task.GetType().IsGenericType)
            {
                PropertyInfo property = task.GetType().GetProperty("Result")!;
                object? value = property.GetValue(task)!;
                return ValueTask.FromResult(value);
            }
            else
            {
                return ValueTask.CompletedTask;
            }
        }
        return result;
    }
}