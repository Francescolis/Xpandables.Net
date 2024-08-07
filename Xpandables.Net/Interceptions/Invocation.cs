﻿
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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// Provides the implementation of the <see cref="IInvocation" /> interface.
/// </summary>
internal record class Invocation : IInvocation
{
    internal ExceptionDispatchInfo? _exceptionDispatchInfo;
    public MethodInfo Method { get; }
    public object Target { get; }
    public Type InterfaceType { get; }
    public IParameterCollection Arguments { get; }
    public Exception? Exception => _exceptionDispatchInfo?.SourceException;
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

    public void SetException(Exception? exception)
        => _exceptionDispatchInfo = exception is null
            ? null
            : ExceptionDispatchInfo.Capture(exception);

    public void SetReturnValue(object? returnValue)
    {
        _exceptionDispatchInfo = null;

        ReturnValue = returnValue;

        Type returnType = ReturnType;

        if (ReturnValue is not null)
        {
            if (returnType.IsAssignableFrom(ReturnValue.GetType()))
            {
                return;
            }

            if (returnType.IsGenericType)
            {
                Type argumentType = returnType.GetGenericArguments()[0];
                if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    MethodInfo fromResultMethod = typeof(Task)
                        .GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(argumentType);

                    ReturnValue = fromResultMethod.Invoke(null, [ReturnValue]);
                }

                if (returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    Type genericValueTaskType = typeof(ValueTask<>)
                        .MakeGenericType(argumentType);

                    ConstructorInfo constructorInfo = genericValueTaskType
                        .GetConstructor([argumentType])!;

                    ReturnValue = constructorInfo.Invoke([ReturnValue]);
                }
            }
        }
    }

    public void SetElapsedTime(TimeSpan elapsedTime)
        => ElapsedTime = elapsedTime;

    public void Proceed()
    {
        Stopwatch watch = Stopwatch.StartNew();

        try
        {
            ReturnValue = Method
                .Invoke(
                    Target,
                    Arguments.Select(arg => arg.Value).ToArray());

            if (ReturnValue is Task { Exception: { } } taskException)
            {
                _exceptionDispatchInfo = ExceptionDispatchInfo
                    .Capture(taskException.Exception);
            }

            if (ReThrowException)
            {
                _exceptionDispatchInfo?.Throw();
            }
        }
        catch (Exception exception)
            when (ReThrowException)
        {
            _exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
        }
        finally
        {
            watch.Stop();
            ElapsedTime = watch.Elapsed;
        }
    }
}