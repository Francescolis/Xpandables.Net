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
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for adding service exports to 
/// an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionCompositionExtensions
{
    /// <summary>
    /// Adds service exports to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the exports to.</param>
    /// <param name="configuration">The configuration to use for the services.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXServiceExports(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddXServiceExports(configuration, _ => { });

    /// <summary>
    /// Adds service exports to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the exports to.</param>
    /// <param name="configuration">The configuration to use for the services.</param>
    /// <param name="configureOptions">The action to configure the export options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXServiceExports(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ExportOptions> configureOptions)
    {
        ExportOptions options = new();
        configureOptions(options);

        ServiceExportExtensions.ApplyServiceExports<IAddServiceExport>(
            options, exports =>
        {
            foreach (IAddServiceExport export in exports)
            {
                export.AddServices(services, configuration);
            }
        });

        return services;
    }

    /// <summary>
    /// Adds services to the <see cref="IServiceCollection"/> from the 
    /// specified assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance 
    /// used to configure the services.</param>
    /// <param name="assemblies">The assemblies to scan for services to add.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls 
    /// can be chained.</returns>
    public static IServiceCollection AddXServices(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

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
                    && t == typeof(IAddService)))];

        foreach (Type type in types)
        {
            if (Activator.CreateInstance(type) is IAddService addService)
            {
                addService.AddServices(services, configuration);
            }
        }

        return services;
    }
}
