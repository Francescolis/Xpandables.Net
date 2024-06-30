
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
internal sealed class FinalizerQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : IQueryHandler<TQuery, TResult>, IDecorator
    where TQuery : notnull, IQuery<TResult>, IOperationFinalizerDecorator
{
    public async Task<IOperationResult<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IOperationResult<TResult> result = await decoratee
                .HandleAsync(query, cancellationToken)
                .ConfigureAwait(false);

            return operationResultFinalizer.Finalizer is not null
                ? operationResultFinalizer.Finalizer
                    .Invoke(result)
                    .ToOperationResult<TResult>()
                : result;
        }
        catch (OperationResultException resultException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer.Invoke(resultException.Operation)
                    .ToOperationResult<TResult>()
                : resultException.Operation
                .ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer.Invoke(exception.ToOperationResult())
                    .ToOperationResult<TResult>()
                : OperationResults
                .InternalError<TResult>()
                .WithTitle("OperationFinalizerQueryDecorator")
                .WithException(exception)
                .Build();
        }
    }
}
