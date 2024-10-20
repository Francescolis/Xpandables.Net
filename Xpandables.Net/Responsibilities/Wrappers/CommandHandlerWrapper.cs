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

using Xpandables.Net.Decorators;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities.Wrappers;

/// <summary>
/// A wrapper for handling commands with decorators.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
public sealed class CommandHandlerWrapper<TCommand>(
    ICommandHandler<TCommand> decoratee,
    IEnumerable<IPipelineDecorator<TCommand, IOperationResult>> decorators) :
    ICommandHandlerWrapper
    where TCommand : class, ICommand
{
    /// <summary>
    /// Handles the command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public Task<IOperationResult> HandleAsync(
        ICommand command,
        CancellationToken cancellationToken = default)
    {
        Task<IOperationResult> Handler() =>
            decoratee.HandleAsync((TCommand)command, cancellationToken);

        Task<IOperationResult> result = decorators
            .Reverse()
            .Aggregate<IPipelineDecorator<TCommand, IOperationResult>,
            RequestHandlerDelegate<IOperationResult>>(
                Handler,
                (next, decorator) => () => decorator.HandleAsync(
                    (TCommand)command,
                    next,
                    cancellationToken))();

        return result;
    }
}