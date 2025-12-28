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
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring event context middleware in a WebApplication request pipeline.
/// </summary>
/// <remarks>Use these extension methods to add event context handling middleware to the application's request
/// pipeline. This enables propagation of event context information, which can be useful for distributed tracing,
/// correlation, or custom event processing scenarios.</remarks>
public static class IEventContextAppExtensions
{
    /// <summary>
    /// Adds the specified event context middleware to the application's request pipeline.
    /// </summary>
    /// <param name="app">The WebApplication instance to configure.</param>
    extension(WebApplication app)
    {
        /// <summary>
        /// Adds the specified event context middleware to the application's request pipeline.
        /// </summary>
        /// <remarks>Use this method to register custom middleware that provides event context handling
        /// for incoming HTTP requests. The middleware type must be registered in the application's dependency injection
        /// container.</remarks>
        /// <typeparam name="TEventContextMiddleware">The middleware type to add to the pipeline. Must implement the IMiddleware interface and have public
        /// constructors and methods.</typeparam>
        /// <returns>The current WebApplication instance for method chaining.</returns>
        public WebApplication UseXEventContextMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] TEventContextMiddleware>()
            where TEventContextMiddleware : class, IMiddleware
        {
            ArgumentNullException.ThrowIfNull(app);

            if (app.Services.GetService<TEventContextMiddleware>() is null)
                throw new InvalidOperationException(
                    $"{typeof(TEventContextMiddleware).Name} is not registered. " +
                    $"Please ensure AddXEventContextMiddleware<{typeof(TEventContextMiddleware).Name}>() is called during service registration.");

            app.UseMiddleware<TEventContextMiddleware>();
            return app;
        }

        /// <summary>
        /// Adds the Event context middleware to the application's request pipeline.
        /// </summary>
        /// <remarks>Call this method during application startup to ensure that event context information
        /// is available throughout the request pipeline. This is typically used to support distributed tracing or
        /// correlation scenarios.</remarks>
        /// <returns>The <see cref="WebApplication"/> instance with the Event context middleware configured. This enables
        /// event context propagation for subsequent middleware and services.</returns>
        public WebApplication UseXEventContextMiddleware()
        {
            ArgumentNullException.ThrowIfNull(app);
            return app.UseXEventContextMiddleware<EventContextMiddleware>();
        }
    }
}
