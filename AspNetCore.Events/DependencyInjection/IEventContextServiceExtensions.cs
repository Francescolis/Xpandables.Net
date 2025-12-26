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
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering event context middleware with an <see cref="IServiceCollection"/> for
/// dependency injection.
/// </summary>
public static class IEventContextServiceExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the specified event context middleware type to the service collection for dependency injection.
        /// </summary>
        /// <typeparam name="TEventContextMiddleware">The type of the event context middleware to register. Must be a reference type with a public constructor.</typeparam>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddXEventContextMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEventContextMiddleware>()
            where TEventContextMiddleware : class, IMiddleware
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddTransient<TEventContextMiddleware>();
            return services;
        }

        /// <summary>
        /// Adds the default XEvent context middleware to the application's service collection.
        /// </summary>
        /// <remarks>Call this method during application startup to ensure that event context information
        /// is available throughout the request pipeline. This is typically used to support distributed tracing or
        /// correlation scenarios.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the XEvent context middleware registered. This enables
        /// event context propagation for subsequent middleware and services.</returns>
        public IServiceCollection AddXEventContextMiddleware()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXEventContextMiddleware<EventContextMiddleware>();
        }
    }
}
