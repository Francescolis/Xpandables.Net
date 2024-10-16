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
using System.ComponentModel;

namespace Xpandables.Net.States;

/// <summary>
/// Represents a context that holds the current state and allows 
/// transitioning to a new state.
/// </summary>
public interface IStateContext
{
    /// <summary>
    /// Gets the current state.
    /// </summary>
    IState CurrentState { get; }

    /// <summary>
    /// Transitions to the specified state.
    /// </summary>
    /// <param name="state">The state to transition to.</param>
    void TransitionToState(IState state);
}

/// <summary>
/// Represents a context that holds the current state of 
/// type <typeparamref name="TState"/> and allows transitioning to a new state.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IStateContext<TState> : IStateContext
    where TState : class, IState
{
    /// <summary>
    /// Gets the current state of type <typeparamref name="TState"/>.
    /// </summary>
    new TState CurrentState { get; }

    /// <summary>
    /// Transitions to the specified state of type <typeparamref name="TState"/>.
    /// </summary>
    /// <param name="state">The state to transition to.</param>
    void TransitionToState(TState state);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IState IStateContext.CurrentState => CurrentState;

    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStateContext.TransitionToState(IState state) =>
        TransitionToState((TState)state);
}