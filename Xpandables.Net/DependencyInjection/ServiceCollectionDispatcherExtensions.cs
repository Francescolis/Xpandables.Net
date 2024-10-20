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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Wrappers;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding dispatcher services to the 
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionDispatcherExtensions
{
    /// <summary>
    /// Adds a dispatcher of type <typeparamref name="TDispatcher"/> to 
    /// the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDispatcher">The type of the dispatcher to add.</typeparam>
    /// <param name="services">The service collection to add the dispatcher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDispatcher<TDispatcher>(
        this IServiceCollection services)
        where TDispatcher : class, IDispatcher =>
        services.AddScoped<IDispatcher, TDispatcher>();

    /// <summary>
    /// Adds a default dispatcher to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the dispatcher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDispatcher(
        this IServiceCollection services) =>
        services.AddXDispatcher<Dispatcher>();

    /// <summary>
    /// Adds dispatcher wrappers to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the dispatcher 
    /// wrappers to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDispatcherWrappers(
        this IServiceCollection services) =>
        services.AddTransient(typeof(QueryHandlerWrapper<,>))
            .AddTransient(typeof(QueryAsyncHandlerWrapper<,>))
            .AddTransient(typeof(CommandHandlerWrapper<>));

    /// <summary>
    /// Adds dispatcher handlers to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the dispatcher 
    /// handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDispatcherHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        IEnumerable<Type> handlers = assemblies.SelectMany(assembly =>
            assembly.GetExportedTypes()
                .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsSealed: true }
                && type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryAsyncHandler<,>)))));

        IEnumerable<IGrouping<Type, Type>> groupedHandlers = handlers.GroupBy(handler =>
        {
            Type interfaceType = handler.GetInterfaces().First(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                i.GetGenericTypeDefinition() == typeof(IQueryAsyncHandler<,>)));

            return interfaceType;
        });

        foreach (IGrouping<Type, Type> group in groupedHandlers)
        {
            Type interfaceType = group.Key;
            Type[] handlerTypes = [.. group];
            Type interfaceGenericType = interfaceType.GetGenericTypeDefinition();

            foreach (Type handlerType in handlerTypes)
            {
                if (interfaceGenericType == typeof(ICommandHandler<>))
                {
                    _ = services.AddScoped(interfaceType, handlerType);
                }
                else if (interfaceGenericType == typeof(IQueryHandler<,>))
                {
                    _ = services.AddScoped(interfaceType, handlerType);
                }
                else if (interfaceGenericType == typeof(IQueryAsyncHandler<,>))
                {
                    _ = services.AddScoped(interfaceType, handlerType);
                }
            }
        }

        return services;
    }
}
