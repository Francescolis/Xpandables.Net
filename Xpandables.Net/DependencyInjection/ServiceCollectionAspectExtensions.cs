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

using Xpandables.Net.Aspects;
using Xpandables.Net.Interceptions;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Registers the aspect services.
/// </summary>
public static partial class ServiceCollectionAspectExtensions
{
    /// <summary>
    /// Ensures that all interfaces decorated with derived 
    /// <see cref="InterceptorAttribute"/> class, 
    /// the <see cref="InterceptorAttribute.Create(IServiceProvider)"/> 
    /// interceptor is returned, wrapping all original implementation 
    /// registered class type found in the specified collection of assemblies.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXAspectBehaviors(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        var decoratedInterfaces = assemblies
            .SelectMany(ass => ass.GetExportedTypes())
            .Where(type => type.IsSealed
                && !type.IsInterface
                && type.GetCustomAttributes(true)
                    .OfType<AspectAttribute>().Any())
            .Select(type => new
            {
                Attribute = type
                    .GetCustomAttributes(true)
                    .OfType<AspectAttribute>()
                    .First(),
                Type = type
                    .GetCustomAttributes(true)
                    .OfType<AspectAttribute>()
                    .First()
                    .InterfaceType
            });

        foreach (var decoInterf in decoratedInterfaces)
        {
            foreach (Type type in assemblies
                .SelectMany(ass => ass.GetExportedTypes())
                .Where(type => !type.IsAbstract
                    && !type.IsInterface
                    && type.IsClass
                    && decoInterf.Type.IsAssignableFrom(type)))
            {
                _ = services.XTryDecorate(decoInterf.Type,
                    (instance, provider) =>
                    {
                        IInterceptor interceptor = decoInterf.Attribute
                            .Create(provider);
                        return InterceptorFactory
                            .CreateProxy(decoInterf.Type, interceptor, instance);
                    });
            }
        }

        return services;
    }

}
