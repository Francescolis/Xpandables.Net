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
using Xpandables.Net.Operations.Messaging;

namespace Xpandables.Net.Decorators;
internal sealed class OperationResultContextCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee, IOperationResultContextFinalizer operationResultContext)
    : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, IOperationResultContextDecorator
{
    private readonly ICommandHandler<TCommand> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly IOperationResultContextFinalizer _operationResultContext = operationResultContext
        ?? throw new ArgumentNullException(nameof(operationResultContext));

    public async ValueTask<OperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        OperationResult result = await _decoratee
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
                .WithTitle(nameof(OperationResultContextCommandDecorator<TCommand>))
                .WithError(nameof(OperationResultContextCommandDecorator<TCommand>), exception)
                .Build();
        }
    }
}
