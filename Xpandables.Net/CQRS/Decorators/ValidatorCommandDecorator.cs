
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
using Xpandables.Net.Validators;

namespace Xpandables.Net.CQRS.Decorators;

/// <summary>
/// This class allows the application author to add validation 
/// support to command control flow.
/// The target command should implement the <see cref="IValidateDecorator"/> interface 
/// in order to activate the behavior.
/// The class decorates the target command handler with an implementation 
/// of <see cref="ICompositeValidator{TArgument}"/>
/// and applies all validators found to the target command before the 
/// command get handled if there is no error.
/// You should provide with implementation
/// of <see cref="IValidator{TArgument}"/> for validation.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidatorCommandDecorator{TCommand}"/> class
/// with the handler to be decorated and the composite validator.
/// </remarks>
/// <param name="decoratee">The command handler to be decorated.</param>
/// <param name="validator">The validator instance.</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The <paramref name="validator"/> is null.</exception>
public sealed class ValidatorCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    ICompositeValidator<TCommand> validator) : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, IValidateDecorator
{
    private readonly ICommandHandler<TCommand> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly ICompositeValidator<TCommand> _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Asynchronously validates the command before handling if there is no error.
    /// </summary>
    /// <param name="command">The command instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="command" /> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an <see cref="OperationResult"/>.</returns>
    public async ValueTask<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        IOperationResult operation = await _validator
            .ValidateAsync(command)
            .ConfigureAwait(false);

        if (operation.IsFailure)
            return operation;

        return await _decoratee
            .HandleAsync(command, cancellationToken)
            .ConfigureAwait(false);
    }
}