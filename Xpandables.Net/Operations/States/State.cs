/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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
************************************************************************************************************/

namespace Xpandables.Net.Operations.States;

/// <summary>
/// The base State class declares methods that all Concrete State should
/// implement and also provides a back reference to the Context object,
/// associated with the State. This back reference can be used by States to
/// transition the Context to another State.
/// </summary>
/// <typeparam name="TStateContext">The type of the context.</typeparam>
public abstract class State<TStateContext> : IState<TStateContext>
    where TStateContext : class, IStateContext
{
    /// <summary>
    /// Gets the state context.
    /// </summary>
    protected TStateContext Context { get; private set; } = default!;

    ///<inheritdoc/>
    public void EnterState(TStateContext context)
    {
        OnEnteringStateContext(context);
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// When overridden in derived class, allows author to modify the state context at runtime before it get applied.
    /// </summary>
    /// <param name="context">The target state context to act with.</param>
    protected virtual void OnEnteringStateContext(TStateContext context) { }
}
