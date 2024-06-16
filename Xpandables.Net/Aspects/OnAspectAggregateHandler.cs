﻿/*******************************************************************************
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

namespace Xpandables.Net.Aspects;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TAggregateCommand"></typeparam>
public sealed class OnAspectAggregateHandler<TAggregate, TAggregateCommand> :
    OnAspect<AspectAggregateHandlerAttribute<TAggregate, TAggregateCommand>>
    where TAggregate : class, IAggregate
    where TAggregateCommand : notnull, IAggregateCommand
{
    ///<inheritdoc/>
    protected override void InterceptCore(
        IInvocation invocation) => throw new NotImplementedException();
}
