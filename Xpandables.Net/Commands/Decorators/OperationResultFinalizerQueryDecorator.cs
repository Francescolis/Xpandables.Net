
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
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Commands.Decorators;
internal sealed class OperationResultFinalizerQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee,
    IOperationResultFinalizer operationResultFinalizer)
    : IQueryHandler<TQuery, TResult>
    where TQuery : notnull, IQuery<TResult>, IOperationResultFinalizerDecorator
{
    public async ValueTask<IOperationResult<TResult>> HandleAsync(
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
            if (operationResultFinalizer.CallFinalizerOnException)
            {
                return operationResultFinalizer
                    .Finalizer.Invoke(resultException.OperationResult)
                    .ToOperationResult<TResult>();
            }

            return resultException.OperationResult
                .ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            if (operationResultFinalizer.CallFinalizerOnException)
            {
                return operationResultFinalizer
                    .Finalizer.Invoke(exception.ToOperationResult())
                    .ToOperationResult<TResult>();
            }

            return OperationResults
                .InternalError<TResult>()
                .WithTitle("OperationResultFinalizerQueryDecorator")
                .WithError(
                    nameof(OperationResultFinalizerQueryDecorator<TQuery, TResult>),
                    exception)
                .Build();
        }
    }
}
