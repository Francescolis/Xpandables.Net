﻿/*******************************************************************************
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
namespace Xpandables.Net.Responsibilities;

/// <summary>
/// This interface is used as a marker for command.
/// Class implementation is used with the <see cref="ICommandHandler{TCommand}"/> 
/// where "TCommand" is a record that implements <see cref="ICommand"/>.
/// This can also be enhanced with some useful decorators.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents a command used in a Decider pattern process.
/// </summary>
/// <remarks>Make sure to provide with a registration of the dependency 
/// provider <see cref="IDeciderDependencyProvider"/>.</remarks>
public interface IDeciderCommand : ICommand
{
    /// <summary>
    /// Gets the dependency type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the key identifier used to identify an instance of the dependency type.
    /// </summary>
    object KeyId { get; }

    internal object Dependency { get; set; }
}

/// <summary>
/// Represents a command uses in a Decider pattern process.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
/// <remarks>Make sure to provide with a registration of the dependency 
/// provider <see cref="IDeciderDependencyProvider"/>.</remarks>
public interface IDeciderCommand<TDependency> : IDeciderCommand
    where TDependency : class
{
}