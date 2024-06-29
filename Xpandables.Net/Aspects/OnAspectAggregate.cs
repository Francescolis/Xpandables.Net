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
using Xpandables.Net.Commands;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Aspects;

/// <summary>
/// This class represents an aspect that is used to intercept commands 
/// targeting aggregates, by supplying the aggregate instance to the command.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <param name="aggregateStore">The aggregate store</param>
public sealed class OnAspectAggregate<TCommand, TAggregate>(
    IAggregateStore<TAggregate> aggregateStore) :
    OnAspect<AspectAggregateAttribute<TCommand, TAggregate>>
    where TAggregate : class, IAggregate
    where TCommand : class, ICommand<TAggregate>
{
    ///<inheritdoc/>
    protected override async Task InterceptCoreAsync(
        IInvocation invocation)
    {
        TCommand command = invocation
            .Arguments[0]
            .Value!
            .AsRequired<TCommand>();

        CancellationToken ct = invocation
            .Arguments[1]
            .Value.As<CancellationToken>();

        IOperationResult<TAggregate> aggregateOperation = await aggregateStore
            .ReadAsync(command.KeyId, ct)
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
            command.Aggregate = aggregateOperation.Result;
        }

        _ = invocation
            .Arguments[0]
            .ChangeValueTo(command);

        invocation.Proceed();

        if (command.Aggregate.IsNotEmpty)
        {
            if ((await aggregateStore
                .AppendAsync(command.Aggregate.Value, ct)
                .ConfigureAwait(false)) is { IsFailure: true } appendOperation)
            {
                invocation.SetReturnValue(appendOperation);
            }
        }
    }
}
