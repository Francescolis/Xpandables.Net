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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Decorators;

/// <summary>
/// This class represents a decorator that is used to intercept requests 
/// targeting aggregates, by supplying the aggregate instance to the request.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <param name="decoratee">The request handler to decorate.</param>
/// <param name="aggregateStore">The aggregate store</param>
public sealed class RequestAggregateHandlerDecorator<TRequest, TAggregate>(
    IRequestAggregateHandler<TRequest, TAggregate> decoratee,
    IAggregateAccessor<TAggregate> aggregateStore) :
    IRequestAggregateHandler<TRequest, TAggregate>, IDecorator
    where TAggregate : class, IAggregate
    where TRequest : class, IRequestAggregate<TAggregate>, IAggregateDecorator
{
    ///<inheritdoc/>
    public async Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        IOperationResult<TAggregate> aggregateOperation = await aggregateStore
            .PeekAsync(request.KeyId, cancellationToken)
            .ConfigureAwait(false);

        if ((aggregateOperation.IsFailure
               && !aggregateOperation.IsNotFoundStatusCode())
               || (aggregateOperation.IsNotFoundStatusCode()
                   && !request.ContinueWhenNotFound))
        {
            return aggregateOperation;
        }

        if (aggregateOperation.IsSuccess)
        {
            request.Aggregate = aggregateOperation.Result;
        }

        IOperationResult decorateeOperation = await decoratee
            .HandleAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (request.Aggregate.IsNotEmpty)
        {
            if ((await aggregateStore
                .AppendAsync(request.Aggregate.Value, cancellationToken)
                .ConfigureAwait(false)) is { IsFailure: true } appendOperation)
            {
                return appendOperation;
            }
        }

        return decorateeOperation;
    }
}
