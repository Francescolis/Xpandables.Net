
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
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.CQRS.Decorators;

/// <summary>
/// Represents a method signature to be used to apply persistence behavior to a command task.
/// </summary>
/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
/// <returns>A task that represents an <see cref="OperationResult"/>.</returns>
/// <exception cref="InvalidOperationException">The persistence operation failed to execute.</exception>
public delegate ValueTask<OperationResult> PersistenceCommandHandler(CancellationToken cancellationToken);

/// <summary>
/// This class allows the application author to add persistence support to command control flow.
/// The target command should implement the <see cref="IPersistenceDecorator"/> interface in order to activate the behavior.
/// The class decorates the target command handler with an definition of <see cref="PersistenceCommandHandler"/> 
/// that get called after the main one in the same control flow only.
/// </summary>
/// <typeparam name="TCommand">Type of command.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="PersistenceCommandDecorator{TCommand}"/> class with
/// the decorated handler and the unit of work to act on.
/// </remarks>
/// <param name="persistenceCommandHandler">The persistence delegate to apply persistence.</param>
/// <param name="decoratee">The decorated command handler.</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> 
/// or <paramref name="persistenceCommandHandler"/>
/// is null.</exception>
public sealed class PersistenceCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    PersistenceCommandHandler persistenceCommandHandler) : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, IPersistenceDecorator
{
    private readonly ICommandHandler<TCommand> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly PersistenceCommandHandler _persistenceCommandHandler = persistenceCommandHandler
            ?? throw new ArgumentNullException(nameof(persistenceCommandHandler));

    /// <summary>
    /// Asynchronously handles the specified command and persists changes to store if there is no exception or error.
    /// </summary>
    /// <param name="command">The command instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="command"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object of <see cref="IOperationResult"/>.</returns>
    public async ValueTask<OperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            OperationResult commandResult = await _decoratee
                .HandleAsync(command, cancellationToken)
                .ConfigureAwait(false);

            if (commandResult.IsFailure)
                return commandResult;

            return await _persistenceCommandHandler(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.ActionSpecifiedFailedSeeException
                    .StringFormat(nameof(PersistenceCommandDecorator<TCommand>)))
                .WithError(nameof(PersistenceCommandDecorator<TCommand>), exception)
                .Build();
        }
    }
}
