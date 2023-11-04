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
internal sealed class OperationResultQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee, IOperationResultContext operationResultContext)
    : IQueryHandler<TQuery, TResult>
    where TQuery : notnull, IQuery<TResult>, IOperationResultDecorator
{
    private readonly IQueryHandler<TQuery, TResult> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly IOperationResultContext _operationResultContext
        = operationResultContext ?? throw new ArgumentNullException(nameof(operationResultContext));

    public async ValueTask<OperationResult<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default)
    {
        OperationResult<TResult> result = await _decoratee
            .HandleAsync(query, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess && _operationResultContext.OnSuccess.IsNotEmpty)
            return _operationResultContext
                .OnSuccess
                .Value
                .ToOperationResult<TResult>();

        if (result.IsFailure && _operationResultContext.OnFailure.IsNotEmpty)
            return _operationResultContext
                .OnFailure
                .Value
                .ToOperationResult<TResult>();

        return result;
    }
}
