
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
/// Represents a method signature to be used to apply transactional behavior to a command task.
/// </summary>
/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
/// <param name="transactionalCommandTask">The command task definition to be executed under transaction.</param>
/// <returns>A task that represents an object of <see cref="IOperationResult"/>.</returns>
public delegate ValueTask<IOperationResult> TransactionCommandHandler(
    CancellationToken cancellationToken,
    Func<ValueTask<IOperationResult>> transactionalCommandTask);

/// <summary>
/// This class allows the application author to add transaction 
/// support to command control flow.
/// The target command should implement the 
/// <see cref="ITransactionDecorator"/> in order to activate the behavior.
/// The class decorates the target command handler with 
/// a definition of <see cref="TransactionCommandHandler"/>.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TransactionCommandDecorator{TCommand}"/> class
/// with the handler to be decorated and the transaction scope provider.
/// </remarks>
/// <param name="decoratee">The decorated command handler.</param>
/// <param name="transactionDelegate">The transaction scope provider.</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="transactionDelegate"/> is null.</exception>
public sealed class TransactionCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    TransactionCommandHandler transactionDelegate) : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, ITransactionDecorator
{
    private readonly ICommandHandler<TCommand> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly TransactionCommandHandler _transactionDelegate = transactionDelegate
        ?? throw new ArgumentNullException(nameof(transactionDelegate));

    /// <summary>
    /// Asynchronously handles the specified command applying a transaction scope if available and if there is no error.
    /// </summary>
    /// <param name="command">The command instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="command" /> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object of <see cref="IOperationResult"/>.</returns>
    public async ValueTask<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _transactionDelegate(
                cancellationToken,
                () => _decoratee.HandleAsync(command, cancellationToken))
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.ActionSpecifiedFailedSeeException
                    .StringFormat(nameof(TransactionCommandDecorator<TCommand>)))
                .WithError(nameof(TransactionCommandDecorator<TCommand>), exception)
                .Build();
        }
    }
}
