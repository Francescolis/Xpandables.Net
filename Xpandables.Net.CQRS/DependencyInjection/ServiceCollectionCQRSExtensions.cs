
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
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.CQRS;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register cqrs services.
/// </summary>
public static class ServiceCollectionCQRSExtensions
{
    internal readonly static MethodInfo AddCommandHandlerMethod = typeof(ServiceCollectionCQRSExtensions).GetMethod(nameof(AddXCommandHandler))!;
    internal readonly static MethodInfo AddQueryHandlerMethod = typeof(ServiceCollectionCQRSExtensions).GetMethod(nameof(AddXQueryHandler))!;
    internal readonly static MethodInfo AddAsyncQueryHandlerMethod = typeof(ServiceCollectionCQRSExtensions).GetMethod(nameof(AddXAsyncQueryHandler))!;

    /// <summary>
    /// Registers the <typeparamref name="TDispatcher"/> type as <see cref="IDispatcher"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TDispatcher">The type that implements <see cref="IDispatcher"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDispatcher<TDispatcher>(this IServiceCollection services)
        where TDispatcher : class, IDispatcher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IDispatcher, TDispatcher>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IDispatcher"/> implementation to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDispatcher(this IServiceCollection services)
        => services.AddXDispatcher<Dispatcher>();

    /// <summary>
    /// Registers the <typeparamref name="TCommandHandler"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler using the <see cref="CommandHandler{TCommand}"/> delegate.</remarks>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TCommandHandler">The type of the command handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates the command handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TCommandHandler>? implementationHandlerFactory = default)
        where TCommandHandler : class, ICommandHandler<TCommand>
        where TCommand : notnull, ICommand
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime<ICommandHandler<TCommand>, TCommandHandler>(
            implementationHandlerFactory);

        services.AddScoped<CommandHandler<TCommand>>(
            provider => provider
                .GetRequiredService<ICommandHandler<TCommand>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers all <see cref="ICommandHandler{TCommand}"/> implementations found in 
    /// the assemblies to the services with scope life time.
    /// </summary>
    /// <remarks>You can refer to the command handler using the <see cref="CommandHandler{TCommand}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXCommandHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(ICommandHandler<>),
            AddCommandHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the query handler wrapper necessary to resolve handlers using type inference.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXQueryHandlerWrapper(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(QueryHandlerWrapper<,>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TQueryHandler"/> to the services with scope 
    /// life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler using the <see cref="QueryHandler{TQuery, TResult}"/> delegate.</remarks>
    /// <typeparam name="TQuery">Type of the query that will be used as argument.</typeparam>
    /// <typeparam name="TResult">Type of the result of the query.</typeparam>
    /// <typeparam name="TQueryHandler">The type of the query handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationQueryFactory">The factory that creates the query handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXQueryHandler<TQuery, TResult, TQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TQueryHandler>? implementationQueryFactory = default)
        where TQuery : notnull, IQuery<TResult>
        where TQueryHandler : class, IQueryHandler<TQuery, TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime<IQueryHandler<TQuery, TResult>, TQueryHandler>(
            implementationQueryFactory);

        services.AddScoped<QueryHandler<TQuery, TResult>>(
            provider => provider
                .GetRequiredService<IQueryHandler<TQuery, TResult>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IQueryHandler{TQuery, TResult}"/> implementations to 
    /// the services with scoped life time.
    /// </summary>
    /// <remarks>You can refer to the command handler 
    /// using the <see cref="QueryHandler{TQuery, TResult}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// if not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    /// <remarks>The query wrapper is also registered.</remarks>
    public static IServiceCollection AddXQueryHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        services.AddXQueryHandlerWrapper();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IQueryHandler<,>),
            AddQueryHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the async query handler wrapper necessary to 
    /// resolve handlers using type inference.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAsyncQueryHandlerWrapper(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(AsyncQueryHandlerWrapper<,>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TAsyncQueryHandler"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler 
    /// using the <see cref="AsyncQueryHandler{TQuery, TResult}"/> delegate.</remarks>
    /// <typeparam name="TAsyncQuery">Type of the query that will be used as argument.</typeparam>
    /// <typeparam name="TResult">Type of the result of the query.</typeparam>
    /// <typeparam name="TAsyncQueryHandler">The type of the query handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationAsyncQueryFactory">The factory that creates the query handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAsyncQueryHandler<TAsyncQuery, TResult, TAsyncQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TAsyncQueryHandler>? implementationAsyncQueryFactory = default)
        where TAsyncQuery : notnull, IAsyncQuery<TResult>
        where TAsyncQueryHandler : class, IAsyncQueryHandler<TAsyncQuery, TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime<IAsyncQueryHandler<TAsyncQuery, TResult>, TAsyncQueryHandler>(
            implementationAsyncQueryFactory);

        services.AddScoped<AsyncQueryHandler<TAsyncQuery, TResult>>(
            provider => provider
                .GetRequiredService<IAsyncQueryHandler<TAsyncQuery, TResult>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IAsyncQueryHandler{TQuery, TResult}"/> 
    /// implementations to the services with scoped life time.
    /// </summary>
    /// <remarks>You can refer to the command handler 
    /// using the <see cref="AsyncQueryHandler{TQuery, TResult}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// if not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    /// <remarks>The query wrapper is also registered.</remarks>
    public static IServiceCollection AddXAsyncQueryHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        services.AddXAsyncQueryHandlerWrapper();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IAsyncQueryHandler<,>),
            AddAsyncQueryHandlerMethod,
            assemblies);
    }


    /// <summary>
    /// Registers and configures the <see cref="ICommandHandler{TCommand}"/>, <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXCQRSHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
            assemblies = [Assembly.GetCallingAssembly()];

        services.AddXCommandHandlers(assemblies);
        services.AddXQueryHandlers(assemblies);
        services.AddXAsyncQueryHandlers(assemblies);

        return services;
    }
}
