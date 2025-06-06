﻿/*******************************************************************************
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

using Xpandables.Net.States;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents the base class for an aggregate root with state management.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate root.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
public abstract class AggregateState<TAggregate, TState> :
    Aggregate,
    IStateContext<TState>
    where TAggregate : AggregateState<TAggregate, TState>
    where TState : class, IState
{
    /// <inheritdoc/>
    // ReSharper disable once MemberCanBePrivate.Global
    public TState CurrentState { get; protected set; } = null!;

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="AggregateState{TAggregateRoot,TState}"/> class.
    /// </summary>
    /// <param name="initialState">The initial state of the aggregate.</param>
    protected AggregateState(TState initialState) =>
        TransitionToState(initialState);

    /// <inheritdoc/>
    public void TransitionToState(TState state)
    {
        CurrentState = state ?? throw new ArgumentNullException(nameof(state));
        CurrentState.EnterStateContext(this);
    }
}