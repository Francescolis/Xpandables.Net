
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

/// <summary>
/// Provides a set of static methods for registering decorators.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ensures that the supplied <typeparamref name="TDecorator"/> decorator is returned, wrapping the
    /// original registered <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <typeparamref name="TDecorator"/>. Multiple decorators may be applied
    /// to the same <typeparamref name="TService"/>. By default, a new <typeparamref name="TDecorator"/>
    /// instance will be returned on each request (according the
    /// <see langword="Transient">Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order in which they are registered
    /// is the order they get applied in. This means that the decorator that gets registered first, gets
    /// applied first, which means that the next registered decorator, will wrap the first decorator, which
    /// wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the given
    /// <typeparamref name="TDecorator"/>.</typeparam>
    /// <typeparam name="TDecorator">The decorator type that will be used to wrap the original service type.
    /// </typeparam>
    /// <typeparam name="TMarker">The marker type interface that must be implemented by the <typeparamref name="TService"/></typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService, TDecorator, TMarker>(
        this IServiceCollection services)
        where TService : class
        where TDecorator : class, TService
        where TMarker : class
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.XTryDecorate(typeof(TService), typeof(TDecorator), typeof(TMarker));
    }

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator is returned, wrapping the
    /// original registered <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. Multiple decorators may be applied
    /// to the same <typeparamref name="TService"/>. By default, a new <paramref name="decorator"/> function
    /// instance will be returned on each request (according the
    /// <see langword="Transient">Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order in which they are registered
    /// is the order they get applied in. This means that the decorator that gets registered first, gets
    /// applied first, which means that the next registered decorator, will wrap the first decorator, which
    /// wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the given
    /// <paramref name="decorator"/>.</typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="decorator">The decorator function type that will be used to wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService>(
        this IServiceCollection services,
        Func<TService, IServiceProvider, TService> decorator)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(decorator);

        return services.DecorateDescriptors(
            typeof(TService),
            serviceDescriptor => serviceDescriptor.DecorateDescriptor(decorator));
    }

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator is returned, wrapping the
    /// original registered <paramref name="serviceType"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. Multiple decorators may be applied
    /// to the same <paramref name="serviceType"/>. By default, a new <paramref name="decorator"/> function
    /// instance will be returned on each request (according the
    /// <see langword="Transient">Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order in which they are registered
    /// is the order they get applied in. This means that the decorator that gets registered first, gets
    /// applied first, which means that the next registered decorator, will wrap the first decorator, which
    /// wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the given
    /// <paramref name="decorator"/>.</param>
    /// <param name="decorator">The decorator function type that will be used to wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Func<object, IServiceProvider, object> decorator)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(decorator);

        return services.DecorateDescriptors(
            serviceType,
            serviceDescriptor => serviceDescriptor.DecorateDescriptor(decorator));
    }

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator is returned, wrapping the
    /// original registered <typeparamref name="TService"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. Multiple decorators may be applied
    /// to the same <typeparamref name="TService"/>. By default, a new <paramref name="decorator"/> function
    /// instance will be returned on each request (according the
    /// <see langword="Transient">Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order in which they are registered
    /// is the order they get applied in. This means that the decorator that gets registered first, gets
    /// applied first, which means that the next registered decorator, will wrap the first decorator, which
    /// wraps the original service type.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">The service type that will be wrapped by the given
    /// <paramref name="decorator"/>.</typeparam>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="decorator">The decorator function type that will be used to wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate<TService>(
        this IServiceCollection services,
        Func<TService, TService> decorator)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(decorator);

        return services.DecorateDescriptors(
            typeof(TService),
            serviceDescriptor => serviceDescriptor.DecorateDescriptor(decorator));
    }

    /// <summary>
    /// Ensures that the supplied <paramref name="decorator"/> function decorator is returned, wrapping the
    /// original registered <paramref name="serviceType"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decorator"/> function. Multiple decorators may be applied
    /// to the same <paramref name="serviceType"/>. By default, a new <paramref name="decorator"/> function
    /// instance will be returned on each request (according the
    /// <see langword="Transient">Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order in which they are registered
    /// is the order they get applied in. This means that the decorator that gets registered first, gets
    /// applied first, which means that the next registered decorator, will wrap the first decorator, which
    /// wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the given
    /// <paramref name="decorator"/>.</param>
    /// <param name="decorator">The decorator function type that will be used to wrap the original service type.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="decorator"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Func<object, object> decorator)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(decorator);

        return services.DecorateDescriptors(
            serviceType,
            serviceDescriptor => serviceDescriptor.DecorateDescriptor(decorator));
    }

    /// <summary>
    /// Ensures that the supplied <paramref name="decoratorType"/> decorator is returned, wrapping the
    /// original registered <paramref name="serviceType"/>, by injecting that service type into the
    /// constructor of the supplied <paramref name="decoratorType"/>. Multiple decorators may be applied
    /// to the same <paramref name="serviceType"/>. By default, a new <paramref name="decoratorType"/>
    /// instance will be returned on each request (according the
    /// <see langword="Transient">Transient</see> lifestyle), independently of the lifestyle of the
    /// wrapped service.
    /// <para>
    /// Multiple decorators can be applied to the same service type. The order in which they are registered
    /// is the order they get applied in. This means that the decorator that gets registered first, gets
    /// applied first, which means that the next registered decorator, will wrap the first decorator, which
    /// wraps the original service type.
    /// </para>
    /// </summary>
    /// <param name="services">The collection of services to act on.</param>
    /// <param name="serviceType">The service type that will be wrapped by the given decorator.</param>
    /// <param name="decoratorType">The decorator type that will be used to wrap the original service type.</param>
    /// <param name="markerType">The marker type interface that must be implemented by the <paramref name="serviceType"/>
    /// in order to be decorated.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="serviceType"/> argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="decoratorType"/> argument is <c>null</c>.</exception>
    public static IServiceCollection XTryDecorate(
        this IServiceCollection services,
        Type serviceType,
        Type decoratorType,
        Type markerType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(decoratorType);
        ArgumentNullException.ThrowIfNull(markerType);

        if (!markerType.IsInterface)
            throw new ArgumentException($"The {nameof(markerType)} must be an interface.");

        return serviceType.GetTypeInfo().IsGenericTypeDefinition
            && decoratorType.GetTypeInfo().IsGenericTypeDefinition
            ? services.DecorateOpenGenerics(serviceType, decoratorType, markerType)
            : services.DecorateDescriptors(
                serviceType,
                serviceDescriptor => serviceDescriptor.DecorateDescriptor(decoratorType));
    }
}
