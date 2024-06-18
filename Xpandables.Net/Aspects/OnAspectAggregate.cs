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
/// 
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TAggregateCommand"></typeparam>
public sealed class OnAspectAggregate<TAggregate, TAggregateCommand>(
    IAggregateStore<TAggregate> aggregateStore) :
    OnAsyncAspect<AspectAggregateAttribute<TAggregate, TAggregateCommand>>
    where TAggregate : class, IAggregate
    where TAggregateCommand : notnull, IAggregateCommand
{
    ///<inheritdoc/>
    protected override async Task InterceptCoreAsync(
        IInvocation invocation)
    {
        TAggregateCommand command = invocation
            .Arguments
            .Select(s => s.Value.As<TAggregateCommand>())
            .OfType<TAggregateCommand>()
            .First();

        CancellationToken ct = invocation
            .Arguments
            .Select(s => s.Value.As<CancellationToken>())
            .OfType<CancellationToken>()
            .First();

        IOperationResult<TAggregate> operationResult = await aggregateStore
            .ReadAsync(command.AggregateId, ct)
            .ConfigureAwait(false);

        if (operationResult.IsFailure)
        {
            invocation.SetReturnValue(operationResult);
            return;
        }

        _ = invocation
            .Arguments
            .First(f => f.Type == typeof(TAggregate))
            .ChangeValueTo(operationResult.Result);

        invocation.Proceed();

        if (invocation.Exception is not null)
        {
            return;
        }

        IOperationResult appendResult = await aggregateStore.AppendAsync(
            operationResult.Result, ct)
            .ConfigureAwait(false);

        if (appendResult.IsFailure)
        {
            invocation.SetReturnValue(appendResult);
        }
    }
}
