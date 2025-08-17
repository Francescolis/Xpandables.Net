
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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for adding endpoint routes to the service collection.
/// </summary>
public static class ServiceCollectionEndpointExtensions
{
    /// <summary>
    /// Adds endpoint routes from the specified assemblies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the endpoint routes to.</param>
    /// <param name="assemblies">The assemblies to scan for endpoint routes. 
    /// If no assemblies are specified, the calling assembly is used.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEndpointRoutes(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        List<Type> endpointTypes = [.. assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is
            {
                IsInterface: false,
                IsGenericType: false,
                IsClass: true,
                IsSealed: true
            }
                && typeof(IEndpointRoute).IsAssignableFrom(type))];

        endpointTypes.ForEach(type =>
            services.Add(new ServiceDescriptor(
                typeof(IEndpointRoute),
                type,
                ServiceLifetime.Transient)));

        return services;
    }

    /// <summary>
    /// Configures the application to use endpoint routes defined in the 
    /// service collection.
    /// </summary>
    /// <param name="application">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication UseXEndpointRoutes(
        this WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        IEnumerable<IEndpointRoute> endpointRoutes = application.Services
            .GetServices<IEndpointRoute>();

        endpointRoutes.ForEach(route => route.AddRoutes(application));

        return application;
    }
}
