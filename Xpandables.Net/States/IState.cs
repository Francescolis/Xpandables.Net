
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
/// Represents a state within a state machine or similar context.
/// </summary>
/// <remarks>Implement this interface to define specific behavior for entering a state.</remarks>
public interface IState
{
    /// <summary>
    /// Method to be called when entering the state context.
    /// </summary>
    /// <param name="context">The context of the state.</param>
    void EnterStateContext(IStateContext context);

    /// <summary>
    /// Method to be called when exiting the state context.
    /// </summary>
    /// <param name="context">The context of the state.</param>
    void ExitStateContext(IStateContext context);
}

/// <summary>
/// Defines a state with a specific context type for use in a state machine pattern.
/// </summary>
/// <typeparam name="TStateContext">The type of the context associated with the state, 
/// which must implement <see cref="IStateContext"/>.</typeparam>
public interface IState<in TStateContext> : IState
    where TStateContext : class, IStateContext
{
    /// <summary>
    /// Method to be called when entering the state with a specific context type.
    /// </summary>
    /// <param name="context">The context of the state.</param>
    void EnterStateContext(TStateContext context);

    /// <summary>
    /// Method to be called when exiting the state with a specific context type.
    /// </summary>
    /// <param name="context">The context of the state.</param>
    void ExitStateContext(TStateContext context);

    [EditorBrowsable(EditorBrowsableState.Never)]
    void IState.EnterStateContext(IStateContext context) =>
        EnterStateContext((TStateContext)context);

    [EditorBrowsable(EditorBrowsableState.Never)]
    void IState.ExitStateContext(IStateContext context) =>
        ExitStateContext((TStateContext)context);
}