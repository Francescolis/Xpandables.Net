
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

namespace Xpandables.Net.Commands;

internal sealed class CommandHandlerWrapper<TCommand, TAggregate>(
    ICommandHandler<TCommand, TAggregate> decoratee)
    : ICommandHandlerWrapper<TAggregate>
    where TAggregate : class, IAggregate
    where TCommand : class, ICommand<TAggregate>
{
    public async ValueTask<IOperationResult> HandleAsync(
        ICommand<TAggregate> command,
        CancellationToken cancellationToken = default)
        => await decoratee.HandleAsync(
            (TCommand)command,
            cancellationToken)
        .ConfigureAwait(false);
}