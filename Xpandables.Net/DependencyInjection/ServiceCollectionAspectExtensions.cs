﻿/*******************************************************************************
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
using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Registers the aspect services.
/// </summary>
public static partial class ServiceCollectionAspectExtensions
{
    /// <summary>
    /// Ensures that all classes decorated with derived 
    /// <see cref="AspectAttribute"/> class will be decorated with the
    /// expected <see cref="OnAspect"/> implementation, wrapping all original 
    /// implementation registered class type found in the specified collection 
    /// of assemblies.
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

        assemblies
           .SelectMany(ass => ass.GetExportedTypes())
           .Where(type => type.IsSealed
                   && type.IsClass
                   && type.GetCustomAttributes(true)
                       .OfType<AspectAttribute>().Any())
           .Select(type => new
           {
               Attributes = type
                   .GetCustomAttributes(true)
                   .OfType<AspectAttribute>()
                   .ToList(),
               type
                   .GetCustomAttributes(true)
                   .OfType<AspectAttribute>()
                   .First()
                   .InterfaceType,
               Type = type
           })
           .ForEach(found =>
           {
               foreach (AspectAttribute attribute in found.Attributes
                .OrderByDescending(o => o.Order))
               {
                   _ = services.XTryDecorate(
                       found.InterfaceType,
                       (instance, provider) =>
                       {
                           IInterceptor interceptor = attribute.Create(provider);
                           return InterceptorFactory
                                 .CreateProxy(
                                    found.InterfaceType,
                                    interceptor,
                                    instance);
                       });
               }
           });

        //foreach (var decoInterf in decoratedInterfaces)
        //{
        //    foreach (Type type in assemblies
        //        .SelectMany(ass => ass.GetExportedTypes())
        //        .Where(type => !type.IsAbstract
        //            && !type.IsInterface
        //            && type.IsClass
        //            && decoInterf.InterfaceType.IsAssignableFrom(type)))
        //    {
        //        _ = services.XTryDecorate(decoInterf.InterfaceType,
        //            (instance, provider) =>
        //            {
        //                IInterceptor interceptor = decoInterf.Attribute
        //                    .Create(provider);
        //                return InterceptorFactory
        //                    .CreateProxy(decoInterf.InterfaceType, interceptor, instance);
        //            });
        //    }
        //}

        return services;
    }

}
