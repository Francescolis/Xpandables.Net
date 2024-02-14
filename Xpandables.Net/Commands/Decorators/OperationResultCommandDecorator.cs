
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

namespace Xpandables.Net.Commands.Decorators;
internal sealed class OperationResultCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee, IOperationResultFinalizer operationResultContext)
    : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, IOperationResultDecorator
{
    private readonly ICommandHandler<TCommand> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly IOperationResultFinalizer _operationResultContext = operationResultContext
        ?? throw new ArgumentNullException(nameof(operationResultContext));

    public async ValueTask<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        IOperationResult result = await _decoratee
            .HandleAsync(command, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (_operationResultContext.Finalizer is not null)
                result = _operationResultContext.Finalizer.Invoke(result);

            return result;
        }
        catch (Exception exception) when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithTitle(nameof(OperationResultCommandDecorator<TCommand>))
                .WithError(nameof(OperationResultCommandDecorator<TCommand>), exception)
                .Build();
        }
    }
}
