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
using Xpandables.Net.States;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides a base class for aggregates that manage their state using a state machine pattern. Enables controlled
/// transitions between states and exposes events for state changes.
/// </summary>
/// <remarks>This class facilitates state management for aggregates by encapsulating state transitions and
/// providing thread-safe access to the current state. It raises events before and after state transitions, allowing
/// subscribers to react to state changes. Derived classes can override transition validation and lifecycle hooks to
/// implement custom logic. State transitions are validated using <see cref="CanTransitionTo(TState)"/>, and both pre-
/// and post-transition hooks are available for extensibility.</remarks>
/// <typeparam name="TState">The type of the state managed by the aggregate. Must implement <see cref="IState"/>.</typeparam>
public abstract class AggregateState<TState> : Aggregate, IStateContext<TState>
    where TState : class, IState
{
    private readonly Lock _stateLock = new();
    private volatile TState _currentState = null!;

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="AggregateState{TState}"/> class.
    /// </summary>
    /// <param name="initialState">The initial state of the aggregate.</param>
    protected AggregateState(TState initialState) =>
        TransitionToState(initialState);

    /// <inheritdoc/>
    public TState CurrentState
    {
        get
        {
            lock (_stateLock)
            {
                return _currentState;
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler<StateTransitionEventArgs>? StateTransitioning;

    /// <inheritdoc/>
    public event EventHandler<StateTransitionEventArgs>? StateTransitioned;

    /// <inheritdoc/>
    public void TransitionToState(TState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        lock (_stateLock)
        {
            var previousState = _currentState;
            var transitionArgs = new StateTransitionEventArgs
            {
                PreviousState = previousState,
                NewState = state,
                TransitionTime = DateTimeOffset.UtcNow
            };

            if (!CanTransitionTo(state))
            {
                throw new InvalidOperationException(
                    $"Invalid state transition from {previousState?.GetType().Name ?? "null"} to {state.GetType().Name}");
            }

            OnStateTransitioning(previousState, state);
            StateTransitioning?.Invoke(this, transitionArgs);

            previousState?.ExitStateContext(this);

            _currentState = state;
            state.EnterStateContext(this);

            OnStateTransitioned(previousState, state);
            StateTransitioned?.Invoke(this, transitionArgs);
        }
    }

    /// <summary>
    /// Determines if transition to the specified state is allowed.
    /// Override to implement custom transition validation logic.
    /// </summary>
    /// <param name="newState">The target state.</param>
    /// <returns>True if transition is allowed, false otherwise.</returns>
    protected virtual bool CanTransitionTo(TState newState) => true;

    /// <summary>
    /// Called before state transition. Override to implement pre-transition logic.
    /// </summary>
    /// <param name="currentState">The current state.</param>
    /// <param name="newState">The new state.</param>
    protected virtual void OnStateTransitioning(TState? currentState, TState newState) { }

    /// <summary>
    /// Called after successful state transition. Override to implement post-transition logic.
    /// Override this method to push domain events for state changes.
    /// </summary>
    /// <param name="previousState">The previous state.</param>
    /// <param name="currentState">The current state.</param>
    protected virtual void OnStateTransitioned(TState? previousState, TState currentState) { }
}
