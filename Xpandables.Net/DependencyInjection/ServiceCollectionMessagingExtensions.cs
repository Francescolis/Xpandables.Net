
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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Aggregates.IntegrationEvents;
using Xpandables.Net.Decorators;
using Xpandables.Net.Operations.Messaging;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register messaging services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    internal readonly static MethodInfo AddCommandHandlerMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXCommandHandler))!;
    internal readonly static MethodInfo AddQueryHandlerMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXQueryHandler))!;
    internal readonly static MethodInfo AddAsyncQueryHandlerMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXAsyncQueryHandler))!;
    internal readonly static MethodInfo AddDomainEventHandlerMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXDomainEventHandler))!;
    internal readonly static MethodInfo AddIntegrationEventHandlerMethod = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddXIntegrationEventHandler))!;

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
    /// Registers the <typeparamref name="TTransientPublisher"/> type as <see cref="ITransientPublisher"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TTransientPublisher">The type that implements <see cref="ITransientPublisher"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientPublisher<TTransientPublisher>(this IServiceCollection services)
        where TTransientPublisher : class, ITransientPublisher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITransientPublisher, TTransientPublisher>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="ITransientPublisher"/> implementation to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientPublisher(this IServiceCollection services)
        => services.AddXTransientPublisher<TransientPublisher>();

    /// <summary>
    /// Registers the <typeparamref name="TTransientSubscriber"/> type as <see cref="ITransientSubscriber"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TTransientSubscriber">The type that implements <see cref="ITransientSubscriber"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientSubscriber<TTransientSubscriber>(this IServiceCollection services)
        where TTransientSubscriber : class, ITransientSubscriber
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITransientSubscriber, TTransientSubscriber>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="ITransientSubscriber"/> implementation to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransientSubscriber(this IServiceCollection services)
        => services.AddXTransientPublisher<TransientPublisherSubscriber>();

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
    /// Registers the <typeparamref name="TDomainEventHandler"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of the domain event</typeparam>
    /// <typeparam name="TAggregateId">the type of aggregate Id.</typeparam>
    /// <typeparam name="TDomainEventHandler">The type of the domain event handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates the domain event handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDomainEventHandler<TDomainEvent, TAggregateId, TDomainEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TDomainEventHandler>? implementationHandlerFactory = default)
        where TDomainEventHandler : class, IDomainEventHandler<TDomainEvent, TAggregateId>
        where TDomainEvent : notnull, IDomainEvent<TAggregateId>
        where TAggregateId : struct, IAggregateId<TAggregateId>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime
            <IDomainEventHandler<TDomainEvent, TAggregateId>, TDomainEventHandler>(
            implementationHandlerFactory);

        services.AddScoped<DomainEventHandler<TDomainEvent>>(
            provider => provider
                .GetRequiredService<IDomainEventHandler<TDomainEvent, TAggregateId>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IDomainEventHandler{TDomainEvent, TAggregateId}"/> 
    /// implementations to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXDomainEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IDomainEventHandler<,>),
            AddDomainEventHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Adds the <typeparamref name="TIntegrationEventHandler"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TIntegrationEvent">The type of the integration event.</typeparam>
    /// <typeparam name="TIntegrationEventHandler">The type of the integration event handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates the integration event handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventHandler<TIntegrationEvent, TIntegrationEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TIntegrationEventHandler>? implementationHandlerFactory = default)
        where TIntegrationEventHandler : class, IIntegrationEventHandler<TIntegrationEvent>
        where TIntegrationEvent : notnull, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(services);

        services.DoRegisterTypeServiceLifeTime
            <IIntegrationEventHandler<TIntegrationEvent>, TIntegrationEventHandler>(
            implementationHandlerFactory);

        services.AddScoped<IntegrationEventHandler<TIntegrationEvent>>(
            provider => provider
                .GetRequiredService<IIntegrationEventHandler<TIntegrationEvent>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Adds the <see cref="IIntegrationEventHandler{TIntegrationEvent}"/> 
    /// implementations to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IIntegrationEventHandler<>),
            AddIntegrationEventHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers and configures the <see cref="ICommandHandler{TCommand}"/>, <see cref="IIntegrationEventHandler{TNotification}"/>,
    /// <see cref="IQueryHandler{TQuery, TResult}"/>, <see cref="IDomainEventHandler{TAggregateId, TDomainEvent}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. If not set, the calling assembly will be used.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="HandlerOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXHandlers(
        this IServiceCollection services,
        Action<HandlerOptions> configureOptions,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        services.AddXCommandHandlers(assemblies);
        services.AddXQueryHandlers(assemblies);
        services.AddXAsyncQueryHandlers(assemblies);
        services.AddXDomainEventHandlers(assemblies);
        services.AddXIntegrationEventHandlers(assemblies);

        return services.AddXHandlersOptions(configureOptions, assemblies);
    }

    /// <summary>
    /// Registers and configures the <see cref="ICommandHandler{TCommand}"/>, <see cref="IIntegrationEventHandler{TNotification}"/>,
    /// <see cref="IQueryHandler{TQuery, TResult}"/>, <see cref="IDomainEventHandler{TAggregateId, TDomainEvent}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies) => services.AddXHandlers(_ => { }, assemblies);

    /// <summary>
    /// Configures the <see cref="ICommandHandler{TCommand}"/>, <see cref="IIntegrationEventHandler{TNotification}"/>,
    /// <see cref="IQueryHandler{TQuery, TResult}"/>, <see cref="IDomainEventHandler{TAggregateId, TDomainEvent}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="HandlerOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXHandlersOptions(
        this IServiceCollection services,
        Action<HandlerOptions> configureOptions,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        var definedOptions = new HandlerOptions();
        configureOptions.Invoke(definedOptions);

        if (definedOptions.IsPersistenceEnabled)
        {
            services.AddXPersistenceCommandDecorator();
            services.AddXPersistenceIntegrationEventDecorator();
        }

        if (definedOptions.IsTransactionEnabled)
        {
            services.AddXTransactionCommandDecorator();
        }

        if (definedOptions.IsValidatorEnabled)
        {
            services.AddXValidatorDecorators();
        }

        if (definedOptions.IsVisitorEnabled)
        {
            services.AddXVisitorDecorators();
        }

        if (definedOptions.IsOperationResultContextEnabled)
        {
            services.AddXOperationResultContextDecorator();
        }

        return services;
    }

    /// <summary>
    /// Registers persistence behavior to commands that are decorated with 
    /// the <see cref="IPersistenceDecorator"/> to the services
    /// with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXPersistenceCommandDecorator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(ICommandHandler<>), typeof(PersistenceCommandDecorator<>));

        return services;
    }

    /// <summary>
    /// Registers persistence behavior to integration events that are decorated 
    /// with the <see cref="IPersistenceDecorator"/> to the services
    /// with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXPersistenceIntegrationEventDecorator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(IIntegrationEventHandler<>), typeof(PersistenceIntegrationEventDecorator<>));

        return services;
    }

    /// <summary>
    /// Registers transaction scope behavior to commands that 
    /// are decorated with the <see cref="ITransactionDecorator"/>
    /// to the services
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTransactionCommandDecorator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(ICommandHandler<>), typeof(TransactionCommandDecorator<>));

        return services;
    }

    /// <summary>
    /// Registers the persistence command handler delegate to be used to 
    /// apply persistence to commands decorated with <see cref="IPersistenceDecorator"/>.
    /// The delegate is use by the <see cref="PersistenceCommandDecorator{TCommand}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="persistenceHandlerBuilder">The persistence command handler factory.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// or <paramref name="persistenceHandlerBuilder"/> is null.</exception>
    public static IServiceCollection AddXPersistenceCommandHandler(
        this IServiceCollection services,
        Func<IServiceProvider, PersistenceCommandHandler> persistenceHandlerBuilder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(persistenceHandlerBuilder);

        services.TryAddScoped(persistenceHandlerBuilder);

        return services;
    }

    /// <summary>
    /// Registers the persistence integration event handler delegate to be used to 
    /// apply persistence to integration events decorated with <see cref="IPersistenceDecorator"/>.
    /// The delegate is use by the <see cref="PersistenceIntegrationEventDecorator{TNotification}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="persistenceHandlerBuilder">The persistence integration event handler factory.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> or 
    /// <paramref name="persistenceHandlerBuilder"/> is null.</exception>
    public static IServiceCollection AddXPersistenceIntegrationEventHandler(
        this IServiceCollection services,
        Func<IServiceProvider, PersistenceIntegrationEventHandler> persistenceHandlerBuilder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(persistenceHandlerBuilder);

        services.TryAddScoped(persistenceHandlerBuilder);

        return services;
    }

    /// <summary>
    /// Registers the transaction command handler delegate to be used to apply 
    /// transaction to commands decorated with <see cref="ITransactionDecorator"/>.
    /// The delegate is use by the <see cref="TransactionCommandDecorator{TCommand}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="transactionHandlerBuilder">The transaction command handler factory.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// or <paramref name="transactionHandlerBuilder"/> is null.</exception>
    public static IServiceCollection AddXTransactionCommandHandler(
        this IServiceCollection services,
        Func<IServiceProvider, TransactionCommandHandler> transactionHandlerBuilder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(transactionHandlerBuilder);

        services.TryAddScoped(transactionHandlerBuilder);

        return services;
    }
}
