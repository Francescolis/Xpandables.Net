﻿
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

    // TODO : find a way to remove try catch
    internal static IServiceCollection DecorateOpenGenerics(
           this IServiceCollection services,
           Type serviceType,
           Type decoratorType)
    {
        foreach (Type[] argument in services.GetArgumentTypes(serviceType))
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var closedServiceType = serviceType.MakeGenericType(argument);
                var closedDecoratorType = decoratorType.MakeGenericType(argument);

                services.DecorateDescriptors(
                    closedServiceType,
                    descriptor => descriptor.DecorateDescriptor(closedDecoratorType));
            }
            catch
            {
                // violated generic constraints
                continue;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        return services;
    }

    internal static Type[] GetGenericParameterTypeConstraints(this Type serviceType)
        => serviceType
            .GetGenericArguments()
            .SelectMany(s => s.GetGenericParameterConstraints())
            .ToArray();

    internal static Type[][] GetArgumentTypes(
        this IServiceCollection services,
        Type serviceType)
        => services
            .Where(x => !x.ServiceType.IsGenericTypeDefinition
                && IsSameGenericType(x.ServiceType, serviceType)
                && !typeof(Delegate).IsAssignableFrom(x.ServiceType))
            .Select(x => x.ServiceType.GenericTypeArguments)
            .Distinct(new ArgumentTypeComparer())
            .ToArray();

    internal sealed class ArgumentTypeComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[]? x, Type[]? y)
        {
            return (x, y) switch
            {
                (null, null) => true,
                (null, _) => false,
                (_, null) => false,
                _ => x.SequenceEqual(y)
            };
        }

        public int GetHashCode(Type[] obj)
            => obj.Aggregate(0, (current, type) => current ^ type.GetHashCode());
    }

    internal static bool IsSameGenericType(Type t1, Type t2)
        => t1.IsGenericType
            && t2.IsGenericType
            && t1.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition();

    internal static IServiceCollection DecorateDescriptors(
        this IServiceCollection services,
        Type serviceType,
        Func<ServiceDescriptor, ServiceDescriptor> decorator)
    {
        foreach (var descriptor in services.GetServiceDescriptors(serviceType))
        {
            var index = services.IndexOf(descriptor);
            services[index] = decorator(descriptor);
        }

        return services;
    }

    internal static ServiceDescriptor DecorateDescriptor(
         this ServiceDescriptor descriptor,
         Type decoratorType)
         => descriptor.WithFactory(
             provider => ActivatorUtilities
                .CreateInstance(
                    provider,
                    decoratorType,
                    provider.GetInstance(descriptor)));

    internal static ServiceDescriptor DecorateDescriptor<TService>(
         this ServiceDescriptor descriptor,
         Func<TService, IServiceProvider, TService> decorator)
         where TService : class
         => descriptor.WithFactory(
             provider => decorator(
                 (TService)provider.GetInstance(descriptor),
                 provider));

    internal static ServiceDescriptor DecorateDescriptor<TService>(
        this ServiceDescriptor descriptor,
        Func<TService, TService> decorator)
        where TService : class
        => descriptor.WithFactory(
            provider => decorator((TService)provider.GetInstance(descriptor)));

    internal static ServiceDescriptor[] GetServiceDescriptors(
         this IServiceCollection services,
         Type serviceType)
         => services
            .Where(service => service.ServiceType == serviceType)
        .ToArray();

    internal static ServiceDescriptor WithFactory(
        this ServiceDescriptor descriptor,
        Func<IServiceProvider, object> factory)
    {
        _ = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _ = factory ?? throw new ArgumentNullException(nameof(factory));

        return ServiceDescriptor
            .Describe(
                descriptor.ServiceType,
                factory,
                descriptor.Lifetime);
    }

    internal static object GetInstance(
        this IServiceProvider serviceProvider,
        ServiceDescriptor descriptor)
    {
        _ = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _ = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationType != null)
            return ActivatorUtilities
                .GetServiceOrCreateInstance(
                    serviceProvider,
                    descriptor.ImplementationType);

        if (descriptor.ImplementationFactory is { })
            return descriptor.ImplementationFactory(serviceProvider);

        throw new InvalidOperationException(
            $"Unable to get instance from descriptor {descriptor.ServiceType.Name}.");
    }
}
