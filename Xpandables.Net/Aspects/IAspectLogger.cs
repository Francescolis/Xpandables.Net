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
using Xpandables.Net.Interceptions;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Defines a logging state used to log the state of target process on entry.
/// </summary>
public record LoggingStateEntry
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    public required string ClassName { get; init; }

    /// <summary>
    /// Gets the method name.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// Contains the arguments (position in signature, names and 
    /// values) with which the method has been invoked.
    /// This argument is provided only for target method with parameters.
    /// </summary>
    public required IParameterCollection Arguments { get; init; }
}

/// <summary>
/// Defines a logging state used to log the state of target process on success.
/// </summary>
public record LoggingStateSuccess : LoggingStateEntry
{
    /// <summary>
    /// Gets the return value of the target method.
    /// </summary>
    /// <remarks>This value is provided only for non-void methods.</remarks>
    public required object? ReturnValue { get; init; }
}

/// <summary>
/// Defines a logging state used to log the state of target process on exit.
/// </summary>
public record LoggingStateExit : LoggingStateSuccess
{
    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public required Exception? Exception { get; init; }

    /// <summary>
    /// Gets the execution time of the target method.
    /// </summary>
    public required TimeSpan ElapsedTime { get; init; }
}

/// <summary>
/// Defines a logging state used to log the state of target process on exception.
/// </summary>
public record LoggingStateFailure : LoggingStateEntry
{
    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public required Exception Exception { get; init; }
}

/// <summary>
/// Represents a marker interface that allows the class implementation to be
/// recognized as an aspect logging and provides the method to handle the state
/// of the logging. 
/// </summary>
public interface IAspectLogger : IAspect
{
    /// <summary>
    /// Allows to handle the state of the logging on entry.
    /// </summary>
    /// <param name="state">The logging state on entry.</param>
    void OnEntry(LoggingStateEntry state);

    /// <summary>
    /// Allows to handle the state of the logging on success.
    /// </summary>
    /// <param name="state">The logging state on success.</param>
    void OnSuccess(LoggingStateSuccess state);

    /// <summary>
    /// Allows to handle the state of the logging on exit.
    /// </summary>
    /// <param name="state">The logging state on exit.</param>
    void OnExit(LoggingStateExit state);

    /// <summary>
    /// Allows to handle the state of the logging on failure.
    /// </summary>
    /// <param name="state">The logging state on failure.</param>
    void OnFailure(LoggingStateFailure state);
}
