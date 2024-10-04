
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
using System.Reflection;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// Provides with the structure for an interception event.
/// </summary>
public interface IInvocation
{
    /// <summary>
    /// Contains the invocation target method info.
    /// </summary>
    MethodInfo Method { get; }

    /// <summary>
    /// Contains the invocation target instance.
    /// </summary>
    object Target { get; }

    /// <summary>
    /// Contains the implemented interface type.
    /// </summary>
    Type InterfaceType { get; }

    /// <summary>
    /// Contains the arguments (position in signature, names and 
    /// values) with which the method has been invoked.
    /// This argument is provided only for target element with parameters.
    /// </summary>
    IParameterCollection Arguments { get; }

    /// <summary>
    /// Gets the exception handled on executing a method.
    /// You can edit this value in order to return a custom exception or null.
    /// If you set this value to null, the process will resume normally and
    /// take care to provide a <see cref="ReturnValue"/> if necessary.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Determines whether the exception should be rethrown.
    /// This allows the author to manually set or remove an exception.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.</remarks>
    bool ReThrowException { get; set; }

    /// <summary>
    /// Gets the executed method return value, only provided 
    /// for non-void method and when no exception handled.
    /// </summary>
    object? ReturnValue { get; }

    /// <summary>
    /// Get the elapsed time execution for the underlying method.
    /// </summary>
    TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Gets the invocation method return type.
    /// </summary>
    Type ReturnType { get; }

    /// <summary>
    /// Sets the exception value.
    /// If you set this value to null, the process will resume normally and
    /// take care to provide a <see cref="ReturnValue" /> if necessary.
    /// </summary>
    /// <remarks>Before setting an exception, set the <see cref="ReThrowException"/>
    /// to <see langword="true"/> or set a new <see cref="ReturnValue"/>.
    /// Otherwise, a <see cref="NullReferenceException"/> will be thrown.</remarks>
    /// <param name="exception">The exception value.</param>
    void SetException(Exception? exception);

    /// <summary>
    /// Sets the executed method return value, only for non-void method.
    /// Be aware to match the return value type.
    /// Otherwise it will throw an exception.
    /// </summary>
    /// <remarks>Setting a <see cref="ReturnValue"/> will clear any exception.</remarks>
    /// <typeparam name="T">The return value type.</typeparam>
    /// <param name="returnValue">The return value to be used.</param>
    void SetReturnValue<T>(T? returnValue);

    /// <summary>
    /// Sets the executed method elapsed time.
    /// </summary>
    /// <param name="elapsedTime">The method elapsed.</param>
    void SetElapsedTime(TimeSpan elapsedTime);

    /// <summary>
    /// Executes the underlying method.
    /// </summary>
    void Proceed();
}