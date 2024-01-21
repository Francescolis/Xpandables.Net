
/************************************************************************************************************
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
************************************************************************************************************/
using System.Diagnostics;
using System.Reflection;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// Provides the implementation of the <see cref="IInvocation" /> interface.
/// </summary>
internal sealed record class Invocation : IInvocation
{
    public MethodInfo InvocationMethod { get; }
    public object InvocationInstance { get; }
    public IParameterCollection Arguments { get; }
    public Exception? Exception { get; private set; }
    public object? ReturnValue { get; private set; }
    public TimeSpan ElapsedTime { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Invocation"/> with the arguments needed for invocation.
    /// </summary>
    /// <param name="targetMethod">The target method.</param>
    /// <param name="targetInstance">The target instance being called.</param>
    /// <param name="argsValue">Arguments for the method, if necessary.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="targetMethod"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="targetInstance"/> is null.</exception>
    internal Invocation(MethodInfo targetMethod, object targetInstance, params object?[]? argsValue)
    {
        InvocationMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
        InvocationInstance = targetInstance ?? throw new ArgumentNullException(nameof(targetInstance));
        Arguments = new ParameterCollection(targetMethod, argsValue);
    }

    public IInvocation AddException(Exception? exception)
    {
        Exception = exception;
        return this;
    }

    public IInvocation AddReturnValue(object? returnValue)
    {
        ReturnValue = returnValue;
        return this;
    }

    public IInvocation AddElapsedTime(TimeSpan elapsedTime)
    {
        ElapsedTime = elapsedTime;
        return this;
    }

    public void Proceed()
    {
        Stopwatch watch = Stopwatch.StartNew();
        watch.Start();

        try
        {
            ReturnValue = InvocationMethod.Invoke(
                                InvocationInstance,
                                Arguments.Select(arg => arg.Value).ToArray());

            if (ReturnValue is Task { Exception: { } } taskException)
                Exception = taskException.Exception.GetBaseException();
        }
        catch (Exception exception) when (exception is TargetException
                                      or ArgumentNullException
                                      or TargetInvocationException
                                      or TargetParameterCountException
                                      or MethodAccessException
                                      or InvalidOperationException
                                      or NotSupportedException)
        {
            Exception = exception;
        }
        finally
        {
            watch.Stop();
            ElapsedTime = watch.Elapsed;
        }
    }
}
