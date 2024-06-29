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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Decorators;

/// <summary>
/// Defines a marker interface for the command aggregate decorator.
/// </summary>
public interface ICommandAggregate
{
    /// <summary>
    /// Determines whether the aspect should continue when the aggregate 
    /// is not found.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.
    /// Usefull when you are creating
    /// a new aggregate.</remarks>
    bool ContinueWhenNotFound { get; }
}

/// <summary>
/// This class represents a decorator that is used to intercept commands 
/// targeting aggregates, by supplying the aggregate instance to the command.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <param name="decoratee">The command handler to decorate.</param>
/// <param name="aggregateStore">The aggregate store</param>
public sealed class AggregateCommandDecorator<TCommand, TAggregate>(
    ICommandHandler<TCommand, TAggregate> decoratee,
    IAggregateStore<TAggregate> aggregateStore) :
    ICommandHandler<TCommand, TAggregate>
    where TAggregate : class, IAggregate
    where TCommand : class, ICommand<TAggregate>, ICommandAggregate
{
    ///<inheritdoc/>
    public async ValueTask<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        IOperationResult<TAggregate> aggregateOperation = await aggregateStore
            .ReadAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        if ((aggregateOperation.IsFailure
               && !aggregateOperation.IsNotFoundStatusCode())
               || (aggregateOperation.IsNotFoundStatusCode()
                   && !command.ContinueWhenNotFound))
        {
            return aggregateOperation;
        }

        if (aggregateOperation.IsSuccess)
        {
            command.Aggregate = aggregateOperation.Result;
        }

        IOperationResult decorateeOperation = await decoratee
            .HandleAsync(command, cancellationToken)
            .ConfigureAwait(false);

        if (command.Aggregate.IsNotEmpty)
        {
            if ((await aggregateStore
                .AppendAsync(command.Aggregate.Value, cancellationToken)
                .ConfigureAwait(false)) is { IsFailure: true } appendOperation)
            {
                return appendOperation;
            }
        }

        return decorateeOperation;
    }
}
