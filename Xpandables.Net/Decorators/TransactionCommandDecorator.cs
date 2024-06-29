
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
using Xpandables.Net.Commands;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Transactions;

namespace Xpandables.Net.Decorators;

/// <summary>
/// This class allows the application author to add transaction 
/// support to command control flow.
/// The target command should implement the 
/// <see cref="ITransactionDecorator"/> in order to activate the behavior.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="TransactionCommandDecorator{TCommand}"/> class
/// with the handler to be decorated and the transaction scope provider.
/// </remarks>
/// <param name="decoratee">The decorated command handler.</param>
/// <param name="transactional">The transaction process to use.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="transactional"/> is null.</exception>
public sealed class TransactionCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    ICommandTransactional transactional) :
    ICommandHandler<TCommand>, IDecorator
    where TCommand : notnull, ICommand, ITransactionDecorator
{
    /// <summary>
    /// Asynchronously handles the specified command applying 
    /// a transaction scope if available and if there is no error.
    /// </summary>
    /// <param name="command">The command instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="command" /> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed. 
    /// See inner exception.</exception>
    /// <returns>A task that represents an object 
    /// of <see cref="IOperationResult"/>.</returns>
    public async ValueTask<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using ITransactional disposable = await transactional
                .TransactionAsync(cancellationToken)
                .ConfigureAwait(false);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            try
            {
                disposable.Result = await decoratee
                    .HandleAsync(command, cancellationToken)
                    .ConfigureAwait(false);

                return disposable.Result;
            }
            catch (OperationResultException decorateeResultException)
            {
                disposable.Result = decorateeResultException.Operation;
                return disposable.Result;
            }
            catch (Exception decorateeException)
                when (decorateeException is not ArgumentNullException)
            {
                disposable.Result = OperationResults
                    .BadRequest()
                    .WithException(decorateeException)
                    .Build();

                return disposable.Result;
            }
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.ActionSpecifiedFailedSeeException
                    .StringFormat(nameof(TransactionCommandDecorator<TCommand>)))
                .WithException(exception)
                .Build();
        }
    }
}
