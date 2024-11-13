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
/// Represents an abstract state with a context of type 
/// <typeparamref name="TStateContext"/>.
/// </summary>
/// <typeparam name="TStateContext">The type of the state context.</typeparam>
public abstract class State<TStateContext> : IState<TStateContext>
    where TStateContext : class, IStateContext
{
    /// <summary>
    /// Gets the context associated with the state.
    /// </summary>
    protected TStateContext Context { get; private set; } = default!;

#pragma warning disable CA1033 // Interface methods should be callable by child types
    void IState<TStateContext>.EnterStateContext(TStateContext context)
#pragma warning restore CA1033 // Interface methods should be callable by child types
    {
        OnEnteringStateContext(context);
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Called when entering the state context.
    /// </summary>
    /// <param name="context">The state context.</param>
    /// <remarks>Override this method to add custom logic when entering the 
    /// state context.</remarks>
    protected virtual void OnEnteringStateContext(TStateContext context)
    {
    }
}
