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
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection.Exports;


namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for registering services with an <see cref="IServiceCollection"/> using
/// configuration and assembly scanning.
/// </summary>
/// <remarks>These extension methods enable modular service registration by discovering and adding services that
/// implement specific export interfaces. Some methods perform assembly scanning and may be affected by trimming when
/// publishing applications; ensure that all required types are preserved if trimming is enabled.</remarks>
public static class IServiceExportWebExtensions
{
    extension(WebApplication application)
    {
        /// <summary>
        /// Uses the service exports with default options.
        /// </summary>
        /// <returns>The web application with applied service exports.</returns>
        [RequiresAssemblyFiles]
        public WebApplication UseXServiceExports()
        {
            ArgumentNullException.ThrowIfNull(application);
            return application.UseXServiceExports(_ => { });
        }

        /// <summary>
        /// Uses the service exports with specified options.
        /// </summary>
        /// <param name="configureOptions">The action to configure export options.</param>
        /// <returns>The web application with applied service exports.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        [RequiresAssemblyFiles()]
        public WebApplication UseXServiceExports(Action<ExportOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(application);
            ArgumentNullException.ThrowIfNull(configureOptions);

            ExportOptions options = new();
            configureOptions(options);

            IServiceExportExtensions.ApplyServiceExports<IUseServiceExport>(
                options, exports =>
                {
                    foreach (IUseServiceExport export in exports)
                    {
                        export.UseServices(application);
                    }
                });

            return application;
        }

        /// <summary>
        /// Uses the specified assemblies to apply services to the web application.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for services.</param>
        /// <returns>The web application with applied services.</returns>
        [RequiresUnreferencedCode("This method may be trimmed.")]
        public WebApplication UseXServices(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(application);
            ArgumentNullException.ThrowIfNull(assemblies);

            assemblies = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

            List<Type> types = [.. assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is
                {
                    IsAbstract: false,
                    IsInterface: false,
                    IsGenericType: false
                }
                && Array.Exists(type.GetInterfaces(),
                    t => !t.IsGenericType
                    && t == typeof(IUseService)))];

            foreach (Type type in types)
            {
                if (Activator.CreateInstance(type) is IUseService useService)
                {
                    useService.UseServices(application);
                }
            }

            return application;
        }
    }
}