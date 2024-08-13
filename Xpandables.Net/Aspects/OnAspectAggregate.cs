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
using Xpandables.Net.Distribution;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aspects;

/// <summary>
/// This class represents an aspect that is used to intercept commands 
/// targeting aggregates, by supplying the aggregate instance to the request.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <param name="accessor">The aggregate store</param>
public sealed class OnAspectAggregate<TRequest, TAggregate>(
    IAggregateAccessor<TAggregate> accessor) :
    OnAspect<AspectAggregateAttribute<TRequest, TAggregate>>
    where TAggregate : class, IAggregate
    where TRequest : class, IRequestAggregate<TAggregate>
{
    ///<inheritdoc/>
    protected override async Task InterceptCoreAsync(
        IInvocation invocation)
    {
        TRequest request = invocation
            .Arguments[0]
            .Value!
            .AsRequired<TRequest>();

        CancellationToken ct = invocation
            .Arguments[1]
            .Value.As<CancellationToken>();

        IOperationResult<TAggregate> aggregateOperation = await accessor
            .PeekAsync(request.KeyId, ct)
            .ConfigureAwait(false);

        if ((aggregateOperation.IsFailure
            && !aggregateOperation.IsNotFoundStatusCode())
            || (aggregateOperation.IsNotFoundStatusCode()
                && !AspectAttribute.ContinueWhenNotFound))
        {
            invocation.SetReturnValue(aggregateOperation);
            return;
        }

        if (aggregateOperation.IsSuccess)
        {
            request.Aggregate = aggregateOperation.Result;
        }

        _ = invocation
            .Arguments[0]
            .ChangeValueTo(request);

        invocation.Proceed();

        if (request.Aggregate.IsNotEmpty)
        {
            if ((await accessor
                .AppendAsync(request.Aggregate.Value, ct)
                .ConfigureAwait(false)) is { IsFailure: true } appendOperation)
            {
                invocation.SetReturnValue(appendOperation);
            }
        }
    }
}
