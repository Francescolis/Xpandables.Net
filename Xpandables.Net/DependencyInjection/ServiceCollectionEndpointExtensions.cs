
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register Xpandables services.
/// </summary>
public static class ServiceCollectionEndpointExtensions
{
    internal const string IEndpointRouteName = "IEndpointRoute";
    internal const string XpandablesNetAspNetCore = "Xpandables.Net.AspNetCore";
    internal static bool ResolveEndpointFromServiceCollection;

    /// <summary>
    /// Registers all the classes that implement the 
    /// <see langword="IEndpointRoute"/> that will be resolved by the 
    /// <see langword="MapXEndpointRoutes"/> to add endpoint to the application.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXEndpointRoutes(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        if (AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(ass => ass.GetName().Name == XpandablesNetAspNetCore)
            ?.GetExportedTypes()
            .FirstOrDefault(t => t.Name == IEndpointRouteName) is not { } type)
            throw new InvalidOperationException(
                I18nXpandables.PathAssemblyUnavailable,
                new ArgumentException(XpandablesNetAspNetCore));

        Type endpointRouteInterfaceType = type;

        List<Type> endpointRouteTypes = assemblies
            .SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                && !type.IsInterface
                && !type.IsGenericType
                && type.IsClass
                && type.IsSealed
                && type.GetInterfaces()
                    .Exists(inter => !inter.IsGenericType
                        && inter == endpointRouteInterfaceType))
            .Select(type => type)
            .ToList();

        foreach (Type endpointRouteType in endpointRouteTypes)
        {
            services.Add(
                new ServiceDescriptor(
                    endpointRouteInterfaceType,
                    endpointRouteType,
                    ServiceLifetime.Transient));
        }

        ResolveEndpointFromServiceCollection = true;

        return services;
    }
}
