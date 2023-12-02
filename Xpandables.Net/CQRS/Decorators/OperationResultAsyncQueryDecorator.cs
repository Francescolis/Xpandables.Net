
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
using System.Runtime.CompilerServices;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.CQRS.Decorators;
internal sealed class OperationResultAsyncQueryDecorator<TQuery, TResult>(
    IAsyncQueryHandler<TQuery, TResult> decoratee, IOperationResultFinalizer operationResultContext)
    : IAsyncQueryHandler<TQuery, TResult>
    where TQuery : notnull, IAsyncQuery<TResult>, IOperationResultDecorator
{
    private readonly IAsyncQueryHandler<TQuery, TResult> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly IOperationResultFinalizer _operationResultContext =
        operationResultContext ?? throw new ArgumentNullException(nameof(operationResultContext));

    public async IAsyncEnumerable<TResult> HandleAsync(
        TQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var asyncEnumerator = _decoratee
            .HandleAsync(query, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        for (var resultExist = true; resultExist;)
        {
            try
            {
                resultExist = await asyncEnumerator.MoveNextAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (_operationResultContext.Finalizer is not null)
                {
                    if (_operationResultContext.Finalizer(exception.ToOperationResult()) is { IsFailure: true } failure)
                        throw new OperationResultException(failure);
                }
                else
                {
                    throw;
                }
            }

            if (resultExist)
                yield return asyncEnumerator.Current;
            else
            {
                yield break;
            }
        }
    }
}
