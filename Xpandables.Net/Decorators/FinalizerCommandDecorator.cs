
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

namespace Xpandables.Net.Decorators;
internal sealed class FinalizerCommandDecorator<TCommand>(
    ICommandHandler<TCommand> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : ICommandHandler<TCommand>, IDecorator
    where TCommand : notnull, ICommand, IOperationFinalizerDecorator
{
    public async Task<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IOperationResult result = await decoratee
                .HandleAsync(command, cancellationToken)
                .ConfigureAwait(false);

            return operationResultFinalizer.Finalizer is not null
                ? operationResultFinalizer.Finalizer.Invoke(result)
                : result;
        }
        catch (OperationResultException resultException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer
                    .Invoke(resultException.Operation)
                : resultException.Operation;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer
                    .Invoke(exception.ToOperationResult())
                : OperationResults
                .InternalError()
                .WithTitle("OperationFinalizerCommandDecorator")
                .WithException(exception)
                .Build();
        }
    }
}
