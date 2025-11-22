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
using AspNetCore.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring MVC options related to Controller within an ASP.NET Core application's
/// service collection.
/// </summary>
/// <remarks>Use the methods in this class to register custom MVC configuration for Controller during application
/// startup. These extensions facilitate integration with the dependency injection system and enable fluent
/// configuration of controller-specific options.</remarks>
public static class ControllerExtensions
{
    /// <summary>
    /// Adds configuration for Controller to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds MVC options configuration for Controller to the service collection.
        /// </summary>
        /// <remarks>Call this method during application startup to enable custom MVC options for
        /// XController. This method registers the necessary configuration as a singleton service.</remarks>
        /// <returns>The service collection with Controller MVC options configured. The same instance as the input is returned
        /// for chaining.</returns>
        public IServiceCollection AddXControllerMvcOptions()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IConfigureOptions<MvcOptions>, ControllerResulMvcOptions>();

            return services;
        }
    }
}
