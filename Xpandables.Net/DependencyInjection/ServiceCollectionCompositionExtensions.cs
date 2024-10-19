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
}
