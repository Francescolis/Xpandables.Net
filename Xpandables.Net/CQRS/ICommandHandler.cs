
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.CQRS;

/// <summary>
/// This interface is used as a marker for commands when using the asynchronous command pattern.
/// Class implementation is used with the <see cref="ICommandHandler{TCommand}"/> where
/// "TCommand" is a class that implements <see cref="ICommand"/>.
/// This can also be enhanced with some useful decorators.
/// </summary>
public interface ICommand : ICQRS { }

/// <summary>
/// Represents a method signature to be used to apply <see cref="ICommandHandler{TCommand}"/> implementation.
/// </summary>
/// <typeparam name="TCommand">Type of the command to act on.</typeparam>
/// <param name="command">The command instance to act on.</param>
/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
/// <returns>A value that represents an <see cref="IOperationResult"/>.</returns>
/// <exception cref="ArgumentNullException">The <paramref name="command"/> is null.</exception>
public delegate ValueTask<IOperationResult> CommandHandler<in TCommand>(
    TCommand command, CancellationToken cancellationToken = default)
    where TCommand : notnull, ICommand;

/// <summary>
/// Provides with a method to asynchronously handle a command of specific type.
/// The implementation must be thread-safe when working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TCommand">Type of the command to act on.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : notnull, ICommand
{
    /// <summary>
    /// Asynchronously handles the specified command.
    /// </summary>
    /// <param name="command">The command instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="command"/> is null.</exception>
    /// <returns>A value that represents an <see cref="IOperationResult"/>.</returns>
    ValueTask<IOperationResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
