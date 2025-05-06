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
/// Represents a state in a state machine.
/// </summary>
public interface IState
{
    /// <summary>
    /// Method to be called when entering the state context.
    /// </summary>
    /// <param name="context">The context of the state.</param>
    void EnterStateContext(IStateContext context);
}

/// <summary>
/// Represents a state in a state machine with a specific context type.
/// </summary>
/// <typeparam name="TStateContext">The type of the state context.</typeparam>
public interface IState<in TStateContext> : IState
    where TStateContext : class, IStateContext
{
    /// <summary>
    /// Method to be called when entering the state with a specific context type.
    /// </summary>
    /// <param name="context">The context of the state.</param>
    void EnterStateContext(TStateContext context);

    [EditorBrowsable(EditorBrowsableState.Never)]
    void IState.EnterStateContext(IStateContext context) =>
        EnterStateContext((TStateContext)context);
}