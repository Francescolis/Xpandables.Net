
/************************************************************************************************************
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
************************************************************************************************************/
using System.Reflection;

using Microsoft.AspNetCore.Builder;

using Xpandables.Net.Operations;
using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register Xpandables services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all the routes via the implementations 
    /// of <see cref="IEndpointRoute.AddRoutes(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)"/> 
    /// found in the specified assemblies.
    /// <para>The implementation classes must declare a parameterless constructor.</para>
    /// </summary>
    /// <param name="builder">The application configuration.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder"/> is null.</exception>
    public static WebApplication MapXEndpointRoutes(
        this WebApplication builder,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        List<Type> endpointRouteTypes = assemblies.SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                && !type.IsInterface
                && !type.IsGenericType
                && type.GetInterfaces()
                    .Exists(inter => !inter.IsGenericType && inter == typeof(IEndpointRoute)))
            .Select(type => type)
            .ToList();

        foreach (Type endpointRouteType in endpointRouteTypes)
        {
            _ = (Activator.CreateInstance(endpointRouteType) as IEndpointRoute)
                .AsOptional()
                .Map(route => route.AddRoutes(builder));
        }

        return builder;
    }
}
