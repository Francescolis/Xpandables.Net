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
#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering and configuring endpoint routes in an ASP.NET Core application.
/// </summary>
/// <remarks>These extension methods enable the discovery and registration of endpoint route implementations from
/// specified assemblies, as well as the configuration of the application's request pipeline to use those routes. Use
/// these methods to simplify the integration of modular endpoint routing patterns in your application.</remarks>
public static class IEndpointRouteExtensions
{
    /// <summary>
    /// Adds endpoint routes from the specified assemblies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the endpoint routes to.</param>
    /// <param name="assemblies">The assemblies to scan for endpoint routes. 
    /// If no assemblies are specified, the calling assembly is used.</param>
    /// <returns>The updated service collection.</returns>
    [RequiresUnreferencedCode("This method may be trimmed.")]
    public static IServiceCollection AddXEndpointRoutes(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        assemblies = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

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

        foreach (var endpointType in endpointTypes)
        {
            services.Add(new ServiceDescriptor(
                typeof(IEndpointRoute),
                endpointType,
                ServiceLifetime.Transient));
        }

        return services;
    }

    /// <summary>
    /// Configures the application to use endpoint routes defined in the 
    /// service collection.
    /// </summary>
    /// <param name="application">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication UseXEndpointRoutes(this WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        IEnumerable<IEndpointRoute> endpointRoutes = application.Services
            .GetServices<IEndpointRoute>();

        foreach (var route in endpointRoutes)
        {
            route.AddRoutes(application);
        }

        return application;
    }
}
