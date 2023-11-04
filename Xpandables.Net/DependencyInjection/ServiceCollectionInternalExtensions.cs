﻿/************************************************************************************************************
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

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;
internal static class ServiceCollectionInternalExtensions
{
    internal static IServiceCollection DoRegisterInterfaceWithMethodFromAssemblies(
         this IServiceCollection services,
         Type interType,
         MethodInfo method,
         Assembly[] assemblies)
    {
        var genericTypes = DoGetGenericTypeMatchingServiceType(interType, assemblies);

        foreach (var generic in genericTypes)
        {
            foreach (var interf in generic.Interfaces)
            {
                Type[] paramTypes = [.. interf.GetGenericArguments()];
                Type methodType = generic.Type;
                MethodInfo methodGeneric = method.MakeGenericMethod([.. paramTypes, methodType]);

                methodGeneric.Invoke(null, [services, null]);
            }
        }

        return services;
    }

    internal readonly record struct GenericTypes(Type Type, IEnumerable<Type> Interfaces);
    internal static IEnumerable<GenericTypes> DoGetGenericTypeMatchingServiceType(
           Type serviceType,
           Assembly[] assemblies)
    {
        return assemblies.SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                           && !type.IsInterface
                           && !type.IsGenericType
                           && Array.Exists(
                               type.GetInterfaces(),
                                inter => inter.IsGenericType && inter.GetGenericTypeDefinition() == serviceType))
            .Select(type => new GenericTypes(
                type,
                type.GetInterfaces()
                    .Where(inter => inter.IsGenericType
                        && inter.GetGenericTypeDefinition() == serviceType)));
    }

    internal static IServiceCollection DoRegisterTypeServiceLifeTime<TInterface, TImplementation>(
         this IServiceCollection services,
         Func<IServiceProvider, TImplementation>? implFactory = default,
         ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
         where TInterface : class
         where TImplementation : class, TInterface
    {
        if (implFactory is not null)
            services.Add(
                new ServiceDescriptor(
                    typeof(TInterface),
                    implFactory,
                    serviceLifetime));
        else
            services.Add(
                new ServiceDescriptor(
                    typeof(TInterface),
                    typeof(TImplementation),
                    serviceLifetime));

        return services;
    }
}
