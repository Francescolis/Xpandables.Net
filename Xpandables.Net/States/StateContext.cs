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
namespace Xpandables.Net.States;
/// <summary>
/// Represents the context for managing state transitions.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public abstract class StateContext<TState> : IStateContext<TState>
    where TState : class, IState
{
    /// <summary>
    /// Gets the current state.
    /// </summary>
    public TState CurrentState { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateContext{TState}"/> 
    /// class with the specified initial state.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    protected StateContext(TState initialState) =>
        TransitionToState(initialState);

    /// <summary>
    /// Transitions to the specified state.
    /// </summary>
    /// <param name="state">The state to transition to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the state 
    /// is null.</exception>
    public void TransitionToState(TState state)
    {
        CurrentState = state ?? throw new ArgumentNullException(nameof(state));
        CurrentState.EnterStateContext(this);
    }
}
