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
using System.ComponentModel;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Commands;
/// <summary>
/// Defines a handler for a command of type <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : class, ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a command of type <typeparamref name="TCommand"/> 
/// with a dependency of type <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface ICommandHandler<in TCommand, TDependency> : ICommandHandler<TCommand>
    where TCommand : class, ICommandDecider<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Handles the specified command asynchronously with the given dependency.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="dependency">The dependency required to handle the command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        TCommand command,
        TDependency dependency,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<IExecutionResult> ICommandHandler<TCommand>.HandleAsync(
        TCommand command,
        CancellationToken cancellationToken) =>
        HandleAsync(command, (TDependency)command.Dependency, cancellationToken);
}
