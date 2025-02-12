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
namespace Xpandables.Net.Executions.Deciders;

/// <summary>
/// Represents a decider used in a Decider pattern process.
/// </summary>
public interface IDecider
{
    /// <summary>
    /// Gets the dependency type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the key identifier used to identify an instance of the dependency type.
    /// </summary>
    object KeyId { get; }

    /// <summary>
    /// Gets the value of the dependency.
    /// </summary>
    /// <remarks>For internal use only.</remarks>
    internal object Dependency { get; set; }
}

/// <summary>
/// Represents a decider used in a Decider pattern process.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDecider<TDependency> : IDecider
    where TDependency : class
{
    /// <summary>
    /// Gets the value of the dependency.
    /// </summary>
    public new Type Type => typeof(TDependency);
}
