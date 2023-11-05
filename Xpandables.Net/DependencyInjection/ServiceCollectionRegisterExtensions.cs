
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;
using Xpandables.Net.Optionals;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides with methods to register services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers services to service collection using 
    /// <see cref="IServiceRegister.RegisterServices(IServiceCollection, IConfiguration)"/> 
    /// in all the implementation found in the specified assemblies.
    /// <para>The implementation classes must declare a parameterless constructor.</para>
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configuration">The current application configuration.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddXRegisters(
        this IServiceCollection services,
        IConfiguration? configuration,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
            assemblies = [Assembly.GetCallingAssembly()];

        var serviceRegisters = assemblies.SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                           && type is { IsInterface: false, IsGenericType: false }
                           && type.GetInterfaces()
                            .Exists(inter => !inter.IsGenericType && inter == typeof(IServiceRegister)))
            .Select(type => type)
            .ToList();

        foreach (var serviceRegister in serviceRegisters)
        {
            Activator.CreateInstance(serviceRegister)
                .AsOptional()
                .Bind(sce => (sce as IServiceRegister).AsOptional())
                .Map(route =>
                {
                    route.RegisterServices(services);
                    if (configuration is not null)
                        route.RegisterServices(services, configuration);
                });
        }

        return services;
    }

    /// <summary>
    /// Ensures that any <see cref="Lazy{T}"/> requested service will 
    /// return <see cref="LazyResolved{T}"/> wrapping the original registered type.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXLazy(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(Lazy<>), typeof(LazyResolved<>));
        return services;
    }
}
