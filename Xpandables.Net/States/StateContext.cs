﻿
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
namespace Xpandables.Net.States;

/// <summary>
/// The <see cref="StateContext{TState}"/> maintains a reference to an instance 
/// of a State subclass, which represents the current state of the Context.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public abstract class StateContext<TState> : IStateContext<TState>
    where TState : class, IState
{
    ///<inheritdoc/>
    public TState CurrentState { get; private set; } = default!;

    /// <summary>
    /// Constructs a new instance of the state context with its initial state.
    /// </summary>
    /// <param name="state">The initial state to be used.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="state"/> is null.</exception>
    protected StateContext(TState state) => TransitionToState(state);

    ///<inheritdoc/>
    public void TransitionToState(TState state)
    {
        _ = state ?? throw new ArgumentNullException(nameof(state));

        CurrentState = state;
        state.EnterState(this);
    }
}
