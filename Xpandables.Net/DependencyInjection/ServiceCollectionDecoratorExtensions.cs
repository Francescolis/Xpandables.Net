
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for decorating 
/// services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionDecoratorExtensions
{
    /// <summary>
    /// Ensures that the supplied (generic) <typeparamref name="TDecorator"/> 
    /// decorator is returned, wrapping the original registered 
    /// <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <typeparamref name="TDecorator"/>. Multiple 
    /// decorators may be applied to the same <typeparamref name="TService"/>. 
    /// By default, a new <typeparamref name="TDecorator"/> instance will be 
    /// returned on each request (according the <see langword="Transient">
    /// Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. 
    /// The order in which they are registered is the order they get applied in.
    /// This means that the decorator that gets registered first, gets applied 
    /// first, which means that the next registered decorator, will wrap the 
    /// first decorator, which wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the 
    /// given
    /// <typeparamref name="TDecorator"/>.</typeparam>
    /// <typeparam name="TDecorator">The decorator type that will be used to
    /// wrap the original service type.
    /// </typeparam>
    /// <typeparam name="TMarker">The marker type interface that must be 
    /// implemented by the generic arguments of <typeparamref name="TService"/>
    /// . Only used for generic decorators.</typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService, TDecorator, TMarker>(
        this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService
        where TMarker : class =>
        services.XTryDecorate(
            typeof(TService),
            typeof(TDecorator),
            typeof(TMarker));

    /// <summary>
    /// Ensures that the supplied (non-generic) <typeparamref name="TDecorator"/> 
    /// decorator is returned, wrapping the original registered 
    /// <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <typeparamref name="TDecorator"/>. Multiple 
    /// decorators may be applied to the same <typeparamref name="TService"/>. 
    /// By default, a new <typeparamref name="TDecorator"/> instance will be 
    /// returned on each request (according the <see langword="Transient">
    /// Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. 
    /// The order in which they are registered is the order they get applied in.
    /// This means that the decorator that gets registered first, gets applied 
    /// first, which means that the next registered decorator, will wrap the 
    /// first decorator, which wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the 
    /// given
    /// <typeparamref name="TDecorator"/>.</typeparam>
    /// <typeparam name="TDecorator">The decorator type that will be used to
    /// wrap the original service type.
    /// </typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService, TDecorator>(
        this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService =>
        services.XTryDecorate(
            typeof(TService),
            typeof(TDecorator));

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function 
    /// decorator is returned, wrapping the original registered 
    /// <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. 
    /// Multiple decorators may be applied to the same 
    /// <typeparamref name="TService"/>. By default, a new 
    /// <paramref name="decorator"/> function instance will be returned on each 
    /// request (according the <see langword="Transient">Transient</see> 
    /// lifestyle), independently of the lifestyle of the wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order 
    /// in which they are registered is the order they get applied in. This 
    /// means that the decorator that gets registered first, gets applied first, 
    /// which means that the next registered decorator, will wrap the first 
    /// decorator, which wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the 
    /// given <paramref name="decorator"/>.</typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="decorator">The decorator function type that will be used to 
    /// wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decorator)
        where TService : class =>
        services.DecorateDescriptors(
            typeof(TService),
            serviceDescriptor =>
                serviceDescriptor.DecorateDescriptor(decorator));

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator
    /// is returned, wrapping the original registered 
    /// <paramref name="serviceType"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. 
    /// Multiple decorators may be applied to the same 
    /// <paramref name="serviceType"/>. By default, a new 
    /// <paramref name="decorator"/> function instance will be returned on each 
    /// request (according the <see langword="Transient">Transient</see> 
    /// lifestyle), independently of the lifestyle of the wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order 
    /// in which they are registered the order they get applied in. This means 
    /// that the decorator that gets registered first, gets applied first, which 
    /// means that the next registered decorator, will wrap the first decorator, 
    /// which wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the 
    /// given <paramref name="decorator"/>.</param>
    /// <param name="decorator">The decorator function type that will be used 
    /// to wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Func<object, IServiceProvider, object> decorator) =>
        services.DecorateDescriptors(
            serviceType,
            serviceDescriptor =>
                serviceDescriptor.DecorateDescriptor(decorator));

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator 
    /// is returned, wrapping the original registered 
    /// <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. 
    /// Multiple decorators may be applied to the same 
    /// <typeparamref name="TService"/>. By default, a new 
    /// <paramref name="decorator"/> function instance will be returned on each 
    /// request (according the <see langword="Transient">Transient</see> 
    /// lifestyle), independently of the lifestyle of the wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order 
    /// in which they are registered the order they get applied in. This means 
    /// that the decorator that gets registered first, gets applied first, which 
    /// means that the next registered decorator, will wrap the first decorator, 
    /// which wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the 
    /// given <paramref name="decorator"/>.</typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="decorator">The decorator function type that will be used 
    /// to wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService>(
        this IServiceCollection services,
        Func<TService, TService> decorator)
        where TService : class =>
        services.DecorateDescriptors(
            typeof(TService),
            serviceDescriptor =>
                serviceDescriptor.DecorateDescriptor(decorator));

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator 
    /// is returned, wrapping the original registered 
    /// <paramref name="serviceType"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. 
    /// Multiple decorators may be applied to the same 
    /// <paramref name="serviceType"/>. By default, a new 
    /// <paramref name="decorator"/> function instance will be returned on each 
    /// request (according the <see langword="Transient">Transient</see> 
    /// lifestyle), independently of the lifestyle of the wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order 
    /// in which they are registered is the order they get applied in. This 
    /// means that the decorator that gets registered first, gets applied first, 
    /// which means that the next registered decorator, will wrap the first 
    /// decorator, which wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the 
    /// given <paramref name="decorator"/>.</param>
    /// <param name="decorator">The decorator function type that will be used to 
    /// wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Func<object, object> decorator) =>
        services.DecorateDescriptors(
            serviceType,
            serviceDescriptor =>
                serviceDescriptor.DecorateDescriptor(decorator));

    /// <summary>
    /// Ensures that the supplied (generic) <paramref name="decoratorType"/> 
    /// decorator is returned, wrapping the original registered 
    /// <paramref name="serviceType"/>, 
    /// by injecting that service type into the constructor of the supplied 
    /// <paramref name="decoratorType"/>. Multiple decorators may be applied
    /// to the same <paramref name="serviceType"/>. By default, a new 
    /// <paramref name="decoratorType"/> instance will be returned on each 
    /// request (according the <see langword="Transient">Transient</see> 
    /// lifestyle), independently of the lifestyle of the wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order 
    /// in which they are registered is the order they get applied in. This 
    /// means that the decorator that gets registered first, gets applied first, 
    /// which means that the next registered decorator, will wrap the first 
    /// decorator, which wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the 
    /// given decorator.</param>
    /// <param name="decoratorType">The decorator type that will be used to 
    /// wrap the original service type.</param>
    /// <param name="markerType">The marker type interface that must be 
    /// implemented by the generic arguments of <paramref name="serviceType"/>
    /// in order to be decorated.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="decoratorType"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Type decoratorType,
        Type markerType)
    {
        if (!markerType.IsInterface)
        {
            throw new ArgumentException(
                $"The {nameof(markerType)} must be an interface.");
        }

        return serviceType.GetTypeInfo().IsGenericTypeDefinition
            && decoratorType.GetTypeInfo().IsGenericTypeDefinition
            ? services.DecorateOpenGenerics(
                    serviceType, decoratorType, markerType)
            : services;
    }

    /// <summary>
    /// Ensures that the supplied (non-generic) <paramref name="decoratorType"/> 
    /// decorator is returned, wrapping the original registered 
    /// <paramref name="serviceType"/>, by injecting that service type into the 
    /// constructor of the supplied <paramref name="decoratorType"/>. 
    /// Multiple decorators may be applied
    /// to the same <paramref name="serviceType"/>. By default, a new 
    /// <paramref name="decoratorType"/> instance will be returned on each 
    /// request (according the <see langword="Transient">Transient</see> 
    /// lifestyle), independently of the lifestyle of the wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order 
    /// in which they are registered is the order they get applied in. This 
    /// means that the decorator that gets registered first, gets applied first, 
    /// which means that the next registered decorator, will wrap the first 
    /// decorator, which wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the 
    /// given decorator.</param>
    /// <param name="decoratorType">The decorator type that will be used to 
    /// wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the 
    /// <paramref name="decoratorType"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Type decoratorType) =>
        services.DecorateDescriptors(
                serviceType,
                serviceDescriptor =>
                    serviceDescriptor.DecorateDescriptor(decoratorType));

    internal static IServiceCollection DecorateOpenGenerics(
       this IServiceCollection services,
       Type serviceType,
       Type decoratorType,
       Type markerType)
    {
        IEnumerable<Type[]> arguments = services
            .GetArgumentTypes(serviceType);

        foreach (Type[] argument in arguments)
        {
            if (!argument.Any(markerType.IsAssignableFrom))
            {
                continue;
            }

            if (serviceType
                .TryMakeGenericType(
                    out Type? closedServiceType, out _, argument)
                && decoratorType
                    .TryMakeGenericType(
                    out Type? closedDecoratorType, out _, argument))
            {
                _ = services.DecorateDescriptors(
                    closedServiceType,
                    descriptor => descriptor
                        .DecorateDescriptor(closedDecoratorType));
            }
        }

        return services;
    }

    internal static bool TryMakeGenericType(
        this Type type,
        [MaybeNullWhen(returnValue: false)] out Type genericType,
        [MaybeNullWhen(returnValue: true)] out Exception typeException,
        params Type[] typeArguments)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        try
        {
            typeException = default;
            genericType = type.MakeGenericType(typeArguments);
            return true;
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                            or ArgumentException
                            or NotSupportedException)
        {
            typeException = exception;
            genericType = default;
            return false;
        }
    }


    internal static Type[] GetGenericParameterTypeConstraints(
        this Type serviceType)
        => [.. serviceType
            .GetGenericArguments()
            .SelectMany(s => s.GetGenericParameterConstraints())];

    internal static Type[][] GetArgumentTypes(
        this IServiceCollection services,
        Type serviceType)
        => [.. services
            .Where(x => !x.ServiceType.IsGenericTypeDefinition
                && IsSameGenericType(x.ServiceType, serviceType)
                && !typeof(Delegate).IsAssignableFrom(x.ServiceType))
            .Select(x => x.ServiceType.GenericTypeArguments)
            .Distinct(new ArgumentTypeComparer())];

    internal sealed class ArgumentTypeComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[]? x, Type[]? y)
            => (x, y) switch
            {
                (null, null) => true,
                (null, _) => false,
                (_, null) => false,
                _ => x.SequenceEqual(y)
            };

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
        foreach (ServiceDescriptor descriptor in services
            .GetServiceDescriptors(serviceType))
        {
            int index = services.IndexOf(descriptor);
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
         => [.. services.Where(service => service.ServiceType == serviceType)];

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
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationType != null)
        {
            return ActivatorUtilities
                .GetServiceOrCreateInstance(
                    serviceProvider,
                    descriptor.ImplementationType);
        }

        if (descriptor.ImplementationFactory is { })
        {
            return descriptor.ImplementationFactory(serviceProvider);
        }

        throw new InvalidOperationException(
            $"Unable to get instance from descriptor {descriptor.ServiceType.Name}.");
    }
}
