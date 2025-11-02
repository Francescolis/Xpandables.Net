/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for applying decorator patterns to services registered in an <see
/// cref="IServiceCollection"/>. These methods enable wrapping existing service registrations with decorator types or
/// functions, supporting both generic and non-generic scenarios, and allowing multiple decorators to be applied in a
/// controlled order.
/// </summary>
/// <remarks>Decorators registered using these extensions are applied in the order of registration: the first
/// registered decorator wraps the original service, the second wraps the first decorator, and so on. By default,
/// decorators are registered with a transient lifetime, regardless of the lifetime of the underlying service. These
/// methods support both type-based and delegate-based decorators, and include overloads for open generic services with
/// marker interfaces. All methods throw <see cref="ArgumentNullException"/> if required arguments are null.</remarks>
public static class IServiceDecoratorExtensions
{
    extension(IServiceCollection services)
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
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        /// <exception cref="ArgumentNullException">If the 
        /// services argument is <c>null</c>.</exception>
        [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
        [RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
        public IServiceCollection XTryDecorate<TService, TDecorator, TMarker>()
            where TService : class
            where TDecorator : class, TService
            where TMarker : class
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.XTryDecorate(
                typeof(TService),
                typeof(TDecorator),
                typeof(TMarker));
        }

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
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        /// <exception cref="ArgumentNullException">If the 
        /// services argument is <c>null</c>.</exception>
        public IServiceCollection XTryDecorate<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDecorator>()
            where TService : class
            where TDecorator : class, TService
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.XTryDecorate(
                typeof(TService),
                typeof(TDecorator));
        }

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
        public IServiceCollection XTryDecorate<TService>(
            Func<TService, IServiceProvider, TService> decorator)
            where TService : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(decorator);

            return services.DecorateDescriptors(
                typeof(TService),
                serviceDescriptor =>
                    serviceDescriptor.DecorateDescriptor(decorator));
        }

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
        public IServiceCollection XTryDecorate(
            Type serviceType,
            Func<object, IServiceProvider, object> decorator)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(decorator);

            return services.DecorateDescriptors(
                serviceType,
                serviceDescriptor =>
                    serviceDescriptor.DecorateDescriptor(decorator));
        }

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
        public IServiceCollection XTryDecorate<TService>(
            Func<TService, TService> decorator)
            where TService : class
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(decorator);

            return services.DecorateDescriptors(
                typeof(TService),
                serviceDescriptor =>
                    serviceDescriptor.DecorateDescriptor(decorator));
        }

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
        public IServiceCollection XTryDecorate(
            Type serviceType,
            Func<object, object> decorator)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(decorator);

            return services.DecorateDescriptors(
                serviceType,
                serviceDescriptor =>
                    serviceDescriptor.DecorateDescriptor(decorator));
        }

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
        [RequiresDynamicCode("Calls Xpandables.Net.IServiceDecoratorExtensions.TryMakeGenericType(out Type, out Exception, params Type[])")]
        [RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
        public IServiceCollection XTryDecorate(
            Type serviceType,
            Type decoratorType,
            Type markerType)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(decoratorType);
            ArgumentNullException.ThrowIfNull(markerType);

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
        public IServiceCollection XTryDecorate(
            Type serviceType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type decoratorType)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(decoratorType);

            return services.DecorateDescriptors(
                    serviceType,
                    serviceDescriptor =>
                        serviceDescriptor.DecorateDescriptor(decoratorType));
        }
    }

    [RequiresDynamicCode("Calls Xpandables.Net.IServiceDecoratorExtensions.TryMakeGenericType(out Type, out Exception, params Type[])")]
    [RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
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

    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    [RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
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
         [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type decoratorType)
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