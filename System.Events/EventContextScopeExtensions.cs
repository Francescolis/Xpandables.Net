/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
namespace System.Events;

/// <summary>
/// Provides extension methods for managing event context scopes using an event context accessor.
/// </summary>
/// <remarks>This class contains static extension methods that facilitate the temporary association of an event
/// context with the current execution scope. These methods are typically used to propagate event context information
/// across asynchronous operations, ensuring that contextual data is available where needed. The previous event context
/// is automatically restored when the scope is disposed.</remarks>
public static class EventContextScopeExtensions
{
    /// <summary>
    /// Begins a new event context scope, setting the specified context as the current scope for the duration of the
    /// returned disposable object.
    /// </summary>
    /// <param name="accessor">The event context accessor used to manage the current event context. Cannot be null.</param>
    extension(IEventContextAccessor accessor)
    {
        /// <summary>
        /// Begins a new event context scope, setting the specified context as the current scope for the duration of the
        /// returned disposable object.
        /// </summary>
        /// <remarks>Use this method to temporarily set a specific event context for operations that
        /// require contextual information. The previous context is restored when the returned disposable is disposed.
        /// This method is typically used in scenarios where event context propagation is required across asynchronous
        /// operations.</remarks>
        /// <param name="context">The event context to associate with the new scope. Cannot be null.</param>
        /// <returns>An <see cref="IDisposable"/> that, when disposed, restores the previous event context.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the underlying accessor is not an <see cref="AsyncLocalEventContextAccessor"/>.</exception>
        public IDisposable BeginScope(EventContext context)
        {
            ArgumentNullException.ThrowIfNull(accessor);

            if (accessor is not AsyncLocalEventContextAccessor asyncLocalAccessor)
            {
                throw new InvalidOperationException(
                    $"'{nameof(accessor)}' must be an '{nameof(AsyncLocalEventContextAccessor)}' to use '{nameof(BeginScope)}'.");
            }

			EventContext prior = asyncLocalAccessor.Current;
            asyncLocalAccessor.SetCurrent(context);
            return new RestoreScope(asyncLocalAccessor, prior);
        }
    }

    private sealed class RestoreScope(AsyncLocalEventContextAccessor accessor, EventContext prior) : IDisposable
    {
        private readonly AsyncLocalEventContextAccessor _accessor = accessor;
        private readonly EventContext _prior = prior;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
			{
				return;
			}

			_disposed = true;

            _accessor.SetCurrent(_prior);
        }
    }
}