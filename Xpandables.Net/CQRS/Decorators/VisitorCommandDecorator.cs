
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
using Xpandables.Net.Visitors;

namespace Xpandables.Net.CQRS.Decorators;

/// <summary>
/// This class allows the application author to add visitor support to command control flow.
/// The target command should implement the <see cref="IVisitable{TVisitable}"/> interface in order to activate the behavior.
/// The class decorates the target command handler with an implementation of <see cref="ICompositeVisitor{TElement}"/>
/// and applies all visitors found to the target command before the command get handled. You should provide with implementation
/// of <see cref="IVisitor{TElement}"/>.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="VisitorCommandDecorator{TCommand}"/> class with
/// the handler to be decorated and the composite visitor.
/// </remarks>
/// <param name="decoratee">the decorated command handler.</param>
/// <param name="visitor">the visitor to be applied.</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The <paramref name="visitor"/> is null.</exception>
public sealed class VisitorCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    ICompositeVisitor<TCommand> visitor) : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, IVisitable
{
    private readonly ICommandHandler<TCommand> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly ICompositeVisitor<TCommand> _visitor = visitor
        ?? throw new ArgumentNullException(nameof(visitor));

    /// <summary>
    /// Asynchronously applies visitor and handles the specified command.
    /// </summary>
    /// <param name="command">The command instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="command"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object of <see cref="IOperationResult"/>.</returns>
    public async ValueTask<IOperationResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await command.AcceptAsync(_visitor).ConfigureAwait(false);

        return await _decoratee.HandleAsync(command, cancellationToken).ConfigureAwait(false);
    }
}