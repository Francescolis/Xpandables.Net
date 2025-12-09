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

using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering Result header writers in an HTTP application's dependency
/// injection container.
/// </summary>
/// <remarks>Use this class to add default or custom implementations of <see cref="IResultHeaderWriter"/> to the
/// service collection, enabling automatic inclusion of result information in HTTP response headers. These
/// methods are typically called during application startup to configure header writing behavior for HTTP
/// responses.</remarks>
public static class IResultHeaderWriterExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the default X-Execution-Result header writer to the service collection for use in HTTP responses.
        /// </summary>
        /// <remarks>This method registers <see cref="ResultHeaderWriter"/> as the implementation
        /// for writing X-Execution-Result headers. Call this method during application startup to enable automatic
        /// inclusion of execution result information in HTTP response headers.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the X-Execution-Result header writer registered.</returns>
        public IServiceCollection AddXResultHeaderWriter()
            => services.AddXResultHeaderWriter<ResultHeaderWriter>();

        /// <summary>
        /// Registers a scoped implementation of <see cref="IResultHeaderWriter"/> using the specified type in
        /// the service collection.
        /// </summary>
        /// <remarks>Use this method to configure dependency injection for custom execution result header
        /// writers. Each request will receive a new instance of <typeparamref
        /// name="TResultHeaderWriter"/>.</remarks>
        /// <typeparam name="TResultHeaderWriter">The type that implements <see cref="IResultHeaderWriter"/> and will be registered as a scoped
        /// service. Must have a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the registration applied.</returns>
        public IServiceCollection AddXResultHeaderWriter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResultHeaderWriter>()
            where TResultHeaderWriter : class, IResultHeaderWriter
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IResultHeaderWriter, TResultHeaderWriter>();
            return services;
        }
    }

}
