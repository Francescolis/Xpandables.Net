
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

using Xpandables.Net.Interceptions;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register interceptors services.
/// </summary>
public static class ServiceCollectionInterceptorExtensions
{
    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping 
    /// the original registered <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">The service type interface for which 
    /// implementation will be wrapped by the given 
    /// <typeparamref name="TInterceptor"/>.</typeparam>
    /// <typeparam name="TInterceptor">The interceptor type that will 
    /// be used to wrap the original service type.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentException">The 
    /// <typeparamref name="TInterface"/> must be an interface.</exception>
    public static IServiceCollection AddXInterceptor<TInterface, TInterceptor>(
        this IServiceCollection services)
        where TInterface : class
        where TInterceptor : class, IInterceptor
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!typeof(TInterface).IsInterface)
            throw new ArgumentException(
                $"{typeof(TInterface).Name} must be an interface.");

        _ = services.AddTransient<TInterceptor>();
        _ = services.XTryDecorate<TInterface>((instance, provider) =>
        {
            TInterceptor interceptor = provider.GetRequiredService<TInterceptor>();
            return InterceptorFactory.CreateProxy(interceptor, instance);
        });

        return services;
    }

    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping the 
    /// original registered <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="interfaceType">The interface service type that will 
    /// be wrapped by the given <paramref name="interceptorType"/>.</param>
    /// <param name="interceptorType">The interceptor type that will be 
    /// used to wrap the original service type
    /// and should implement <see cref="IInterceptor"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="interfaceType"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="interceptorType"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="interceptorType"/> 
    /// must implement <see cref="IInterceptor"/>.</exception>
    public static IServiceCollection AddXInterceptor(
        this IServiceCollection services,
        Type interfaceType,
        Type interceptorType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(interfaceType);
        ArgumentNullException.ThrowIfNull(interceptorType);

        if (!interfaceType.IsInterface)
            throw new ArgumentException(
                $"{interfaceType.Name} must be an interface.");

        if (!typeof(IInterceptor).IsAssignableFrom(interceptorType))
            throw new ArgumentException(
                $"{nameof(interceptorType)} must implement" +
                $" {nameof(IInterceptor)}.");

        _ = services.AddTransient(interceptorType);
        _ = services.XTryDecorate(interfaceType, (instance, provider) =>
        {
            IInterceptor interceptor = (IInterceptor)provider
            .GetRequiredService(interceptorType);
            return InterceptorFactory
            .CreateProxy(interfaceType, interceptor, instance);
        });

        return services;
    }

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
    public static IServiceCollection AddXInterceptorAttributes(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        var decoratedInterfaces = assemblies
            .SelectMany(ass => ass.GetExportedTypes())
            .Where(type => type.IsAbstract
                && type.IsInterface
                && type.GetCustomAttributes(true)
                    .OfType<InterceptorAttribute>().Any())
            .Select(type => new
            {
                Type = type,
                Attribute = type.GetCustomAttributes(true)
                    .OfType<InterceptorAttribute>().First()
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
