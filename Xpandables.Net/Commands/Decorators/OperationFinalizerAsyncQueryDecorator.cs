
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
using System.Runtime.CompilerServices;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Commands.Decorators;
internal sealed class OperationFinalizerAsyncQueryDecorator<TQuery, TResult>(
    IAsyncQueryHandler<TQuery, TResult> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : IAsyncQueryHandler<TQuery, TResult>, IDecorator
    where TQuery : notnull, IAsyncQuery<TResult>,
    IOperationFinalizerDecorator
{
    public async IAsyncEnumerable<TResult> HandleAsync(
        TQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using IAsyncEnumerator<TResult> asyncEnumerator = decoratee
            .HandleAsync(query, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task


        TResult? result = default;
        bool finalResult = false;

        for (bool resultExist = true; resultExist;)
        {
            try
            {
                resultExist = await asyncEnumerator
                    .MoveNextAsync()
                    .ConfigureAwait(false);

                if (resultExist)
                    result = asyncEnumerator.Current;
            }
            catch (Exception exception)
            {
                if (operationResultFinalizer.CallFinalizerOnException)
                {
                    IOperationResult<TResult> finalizerResult
                        = operationResultFinalizer
                        .Finalizer
                        .Invoke(exception.ToOperationResult())
                        .ToOperationResult<TResult>();

                    if (finalizerResult.IsFailure)
                        throw new OperationResultException(finalizerResult);

                    if (finalizerResult.Result is not null)
                    {
                        result = finalizerResult.Result;
                        resultExist = true;
                        finalResult = true;
                    }
                    else
                    {
                        resultExist = false;
                    }
                }
                else
                {
                    throw;
                }
            }

            if (resultExist)
            {
                yield return result!;
                if (finalResult)
                    yield break;
            }
            else
            {
                yield break;
            }
        }
    }
}
