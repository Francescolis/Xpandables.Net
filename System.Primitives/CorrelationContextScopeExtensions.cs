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
namespace System;

/// <summary>
/// Provides extension methods for managing correaltion context scopes using a correaltion context accessor.
/// </summary>
/// <remarks>This class contains static extension methods that facilitate the temporary association of a correlation
/// context with the current execution scope. These methods are typically used to propagate correlation context information
/// across asynchronous operations, ensuring that contextual data is available where needed. The previous correlation context
/// is automatically restored when the scope is disposed.</remarks>
public static class CorrelationContextScopeExtensions
{
	/// <summary>
	/// Begins a new correlation context scope, setting the specified context as the current scope for the duration of the
	/// returned disposable object.
	/// </summary>
	/// <param name="accessor">The correlation context accessor used to manage the current correlation context. Cannot be null.</param>
	extension(ICorrelationContextAccessor accessor)
	{
		/// <summary>
		/// Begins a new correlation context scope, setting the specified context as the current scope for the duration of the
		/// returned disposable object.
		/// </summary>
		/// <remarks>Use this method to temporarily set a specific correlation context for operations that
		/// require contextual information. The previous context is restored when the returned disposable is disposed.
		/// This method is typically used in scenarios where correlation context propagation is required across asynchronous
		/// operations.</remarks>
		/// <param name="context">The correlation context to associate with the new scope. Cannot be null.</param>
		/// <returns>An <see cref="IDisposable"/> that, when disposed, restores the previous correlation context.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the underlying accessor is not an <see cref="AsyncLocalCorrelationContextAccessor"/>.</exception>
		public IDisposable BeginScope(CorrelationContext context)
		{
			ArgumentNullException.ThrowIfNull(accessor);

			if (accessor is not AsyncLocalCorrelationContextAccessor asyncLocalAccessor)
			{
				throw new InvalidOperationException(
					$"'{nameof(accessor)}' must be an '{nameof(AsyncLocalCorrelationContextAccessor)}' to use '{nameof(BeginScope)}'.");
			}

			CorrelationContext prior = asyncLocalAccessor.Current;
			asyncLocalAccessor.SetCurrent(context);
			return new RestoreScope(asyncLocalAccessor, prior);
		}
	}

	private sealed class RestoreScope(AsyncLocalCorrelationContextAccessor accessor, CorrelationContext prior) : IDisposable
	{
		private readonly AsyncLocalCorrelationContextAccessor _accessor = accessor;
		private readonly CorrelationContext _prior = prior;
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			// Clear the async-local slot when restoring to default to avoid
			// keeping a stale value in long-lived execution contexts.
			if (_prior == default)
			{
				_accessor.ClearCurrent();
			}
			else
			{
				_accessor.SetCurrent(_prior);
			}
		}
	}
}
