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
/// Describes the retry state.
/// </summary>
public sealed record RetryState
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

    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public required Exception Exception { get; init; }

    /// <summary>
    /// Gets the delay between retries.
    /// </summary>
    public required TimeSpan Delay { get; init; }

    /// <summary>
    /// Gets the maximum number of retries.
    /// </summary>
    public required int MaxRetries { get; init; }

    /// <summary>
    /// Gets the attempt number.
    /// </summary>
    public required int Attempt { get; init; }
}

/// <summary>
/// Represents a marker interface that allows the class implementation to be
/// recognized as an aspect retry and provides the method to handle the state
/// of the retry.
/// </summary>
public interface IAspectRetry : IAspect
{
    /// <summary>
    /// Handles the state during the retry.
    /// </summary>
    /// <param name="state">The retry state.</param>
    void OnRetry(RetryState state);
}
