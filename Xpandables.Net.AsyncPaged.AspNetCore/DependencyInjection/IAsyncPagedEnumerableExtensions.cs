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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Xpandables.Net.Collections.Generic;
using Xpandables.Net.Http;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
/// instance.
/// </summary>
/// <remarks>This class contains static methods that extend the functionality of <see cref="IServiceCollection"/>,
/// enabling additional service registration patterns and convenience features commonly used in dependency injection
/// scenarios.</remarks>
public static class IAsyncPagedEnumerableExtensions
{
    /// <summary>
    /// Provides extensions methds for <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to ac on.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configures support for asynchronous paged enumerables in MVC by registering required options and services.
        /// </summary>
        /// <remarks>Call this method during application startup to enable model binding and serialization
        /// for types implementing <see cref="IAsyncPagedEnumerable{T}"/> in MVC controllers. This configuration is
        /// required for proper handling of paged asynchronous data in API responses.</remarks>
        /// <returns>The current <see cref="IServiceCollection"/> instance with asynchronous paged enumerable support configured.</returns>
        public IServiceCollection ConfigureIAsyncPagedEnumerableMvcOptions()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddEnumerable(
                ServiceDescriptor
                .Transient<IConfigureOptions<MvcOptions>, AsyncPagedEnumerableMvcOptions>());

            return services;
        }
    }
}
