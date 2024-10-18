
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.States;

namespace Xpandables.Net.Events.Defaults;

/// <summary>
/// Represents the context for an aggregate state with a specific aggregate and 
/// state type.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TAggregateState">The type of the aggregate state.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="AggregateStateContext{TAggregate, TAggregateState}"/> class.
/// </remarks>
/// <param name="startState">The initial state of the aggregate.</param>
public abstract class AggregateStateContext<TAggregate, TAggregateState>(
    TAggregateState startState) :
    AggregateStateContext<TAggregate, TAggregateState, Guid>(startState)
    where TAggregate : AggregateStateContext<TAggregate, TAggregateState>
    where TAggregateState : class, IState
{
}
