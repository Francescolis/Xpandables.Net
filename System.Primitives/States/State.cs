/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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

namespace System.States;

/// <summary>
/// Represents an abstract base class for implementing state objects within a state machine pattern.
/// </summary>
/// <remarks>This class serves as a marker for generic constraint.
/// Real state types should derive from the generic <see cref="State{TStateContext}"/> class.</remarks>
public abstract class State : IState
{
	/// <inheritdoc/>
	public abstract void EnterStateContext(IStateContext context);
	/// <inheritdoc/>
	public abstract void ExitStateContext(IStateContext context);
}

/// <summary>
/// Represents an abstract state with a context of type 
/// <typeparamref name="TStateContext"/>.
/// </summary>
/// <typeparam name="TStateContext">The type of the state context.</typeparam>
public abstract class State<TStateContext> : State, IState<TStateContext>
	where TStateContext : class, IStateContext
{
	/// <summary>
	/// Gets the context associated with the state.
	/// </summary>
	protected TStateContext Context { get; private set; } = null!;

	[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "<Pending>")]
	void IState<TStateContext>.EnterStateContext(TStateContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		Context = context;
		OnEnteringStateContext(context);
	}

	/// <inheritdoc/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed override void EnterStateContext(IStateContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		if (context is not TStateContext typedContext)
		{
			throw new InvalidOperationException(
				$"Expected context of type {typeof(TStateContext).FullName}, but received {context.GetType().FullName}.");
		}

		((IState<TStateContext>)this).EnterStateContext(typedContext);
	}

	[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "<Pending>")]
	void IState<TStateContext>.ExitStateContext(TStateContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		OnExitingStateContext(context);
	}

	/// <inheritdoc/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed override void ExitStateContext(IStateContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		if (context is not TStateContext typedContext)
		{
			throw new InvalidOperationException(
				$"Expected context of type {typeof(TStateContext).FullName}, but received {context.GetType().FullName}.");
		}

		((IState<TStateContext>)this).ExitStateContext(typedContext);
	}

	/// <summary>
	/// Called when entering the state context.
	/// </summary>
	/// <param name="context">The state context.</param>
	/// <remarks>Override this method to add custom logic when entering the 
	/// state context.</remarks>
	protected virtual void OnEnteringStateContext(TStateContext context) { }

	/// <summary>
	/// Called when exiting the state context.
	/// </summary>
	/// <param name="context">The state context.</param>
	/// <remarks>Override this method to add custom logic when exiting the 
	/// state context.</remarks>
	protected virtual void OnExitingStateContext(TStateContext context) { }
}
