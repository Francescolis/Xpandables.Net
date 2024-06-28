
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
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Commands;
using Xpandables.Net.Decorators;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Visitors;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register command services.
/// </summary>
public static class ServiceCollectionCommandQueriesExtensions
{
    internal readonly static MethodInfo AddCommandHandlerMethod =
        typeof(ServiceCollectionCommandQueriesExtensions)
        .GetMethod(nameof(AddXCommandHandler))!;

    internal readonly static MethodInfo AddAggregateCommandHandlerMethod =
        typeof(ServiceCollectionCommandQueriesExtensions)
        .GetMethod(nameof(AddXAggregateCommandHandler))!;

    internal readonly static MethodInfo AddQueryHandlerMethod =
        typeof(ServiceCollectionCommandQueriesExtensions)
        .GetMethod(nameof(AddXQueryHandler))!;

    internal readonly static MethodInfo AddAsyncQueryHandlerMethod =
        typeof(ServiceCollectionCommandQueriesExtensions)
        .GetMethod(nameof(AddXAsyncQueryHandler))!;

    /// <summary>
    /// Registers the <typeparamref name="TDispatcher"/> type 
    /// as <see cref="IDispatcher"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TDispatcher">The type that implements 
    /// <see cref="IDispatcher"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDispatcher<TDispatcher>(
        this IServiceCollection services)
        where TDispatcher : class, IDispatcher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IDispatcher, TDispatcher>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="IDispatcher"/> implementation to 
    /// the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDispatcher(
        this IServiceCollection services)
        => services.AddXDispatcher<Dispatcher>();

    /// <summary>
    /// Registers the <typeparamref name="TCommandHandler"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler using the 
    /// <see cref="CommandHandler{TCommand, TAggregate}"/> delegate.</remarks>
    /// <typeparam name="TAggregate">The type of aggregate.</typeparam>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TCommandHandler">The type of the command 
    /// handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that 
    /// creates the command handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAggregateCommandHandler
        <TCommand, TAggregate, TCommandHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TCommandHandler>? implementationHandlerFactory
        = default)
        where TAggregate : class, IAggregate
        where TCommandHandler : class, ICommandHandler<TCommand, TAggregate>
        where TCommand : class, ICommand<TAggregate>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <ICommandHandler<TCommand, TAggregate>, TCommandHandler>(
            implementationHandlerFactory);

        _ = services.AddScoped<CommandHandler<TCommand, TAggregate>>(
            provider => provider
                .GetRequiredService<ICommandHandler<TCommand, TAggregate>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers all <see cref="ICommandHandler{TCommand, TAggregate}"/> 
    /// implementations found in 
    /// the assemblies to the services with scope life time.
    /// </summary>
    /// <remarks>You can refer to the command handler using the 
    /// <see cref="CommandHandler{TCommand}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXAggregateCommandHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXCommandHandlerWrapper();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(ICommandHandler<,>),
            AddAggregateCommandHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the command handler wrapper necessary to resolve 
    /// handlers using type inference.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXCommandHandlerWrapper(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(CommandHandlerWrapper<,>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TCommandHandler"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler using the 
    /// <see cref="CommandHandler{TCommand}"/> delegate.</remarks>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TCommandHandler">The type of the command 
    /// handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that 
    /// creates the command handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TCommandHandler>? implementationHandlerFactory
        = default)
        where TCommandHandler : class, ICommandHandler<TCommand>
        where TCommand : notnull, ICommand
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <ICommandHandler<TCommand>, TCommandHandler>(
            implementationHandlerFactory);

        _ = services.AddScoped<CommandHandler<TCommand>>(
            provider => provider
                .GetRequiredService<ICommandHandler<TCommand>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers all <see cref="ICommandHandler{TCommand}"/> 
    /// implementations found in 
    /// the assemblies to the services with scope life time.
    /// </summary>
    /// <remarks>You can refer to the command handler using the 
    /// <see cref="CommandHandler{TCommand}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXCommandHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(ICommandHandler<>),
            AddCommandHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the query handler wrapper necessary to resolve 
    /// handlers using type inference.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXQueryHandlerWrapper(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(QueryHandlerWrapper<,>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TQueryHandler"/> to the 
    /// services with scope 
    /// life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler using the 
    /// <see cref="QueryHandler{TQuery, TResult}"/> delegate.</remarks>
    /// <typeparam name="TQuery">Type of the query that will 
    /// be used as argument.</typeparam>
    /// <typeparam name="TResult">Type of the result of the query.</typeparam>
    /// <typeparam name="TQueryHandler">The type of the query handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationQueryFactory">The factory that 
    /// creates the query handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXQueryHandler<TQuery, TResult, TQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TQueryHandler>? implementationQueryFactory
        = default)
        where TQuery : notnull, IQuery<TResult>
        where TQueryHandler : class, IQueryHandler<TQuery, TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <IQueryHandler<TQuery, TResult>, TQueryHandler>(
            implementationQueryFactory);

        _ = services.AddScoped<QueryHandler<TQuery, TResult>>(
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
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    /// <remarks>The query wrapper is also registered.</remarks>
    public static IServiceCollection AddXQueryHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies =
                [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXQueryHandlerWrapper();

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
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXAsyncQueryHandlerWrapper(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(AsyncQueryHandlerWrapper<,>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TAsyncQueryHandler"/> to the 
    /// services with  scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the command handler 
    /// using the <see cref="AsyncQueryHandler{TQuery, TResult}"/> delegate
    /// .</remarks>
    /// <typeparam name="TAsyncQuery">Type of the query that will be used as 
    /// argument.</typeparam>
    /// <typeparam name="TResult">Type of the result of the query.</typeparam>
    /// <typeparam name="TAsyncQueryHandler">The type of the query handler
    /// .</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationAsyncQueryFactory">The factory that creates 
    /// the query handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXAsyncQueryHandler
        <TAsyncQuery, TResult, TAsyncQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TAsyncQueryHandler>?
        implementationAsyncQueryFactory = default)
        where TAsyncQuery : notnull, IAsyncQuery<TResult>
        where TAsyncQueryHandler : class, IAsyncQueryHandler<TAsyncQuery, TResult>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime<IAsyncQueryHandler
            <TAsyncQuery, TResult>, TAsyncQueryHandler>(
            implementationAsyncQueryFactory);

        _ = services.AddScoped<AsyncQueryHandler<TAsyncQuery, TResult>>(
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
    /// using the <see cref="AsyncQueryHandler{TQuery, TResult}"/> delegate
    /// .</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// if not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    /// <remarks>The query wrapper is also registered.</remarks>
    public static IServiceCollection AddXAsyncQueryHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXAsyncQueryHandlerWrapper();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IAsyncQueryHandler<,>),
            AddAsyncQueryHandlerMethod,
            assemblies);
    }


    /// <summary>
    /// Registers and configures the <see cref="ICommandHandler{TCommand}"/>, 
    /// <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXCommandQueryHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXCommandHandlers(assemblies);
        _ = services.AddXQueryHandlers(assemblies);
        _ = services.AddXAsyncQueryHandlers(assemblies);

        return services;
    }

    /// <summary>
    /// Registers and configures the <see cref="ICommandHandler{TCommand}"/>, 
    /// <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <param name="configureOptions">A delegate to configure the 
    /// <see cref="CommandOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXCommandQueryHandlers(
        this IServiceCollection services,
        Action<CommandOptions> configureOptions,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXCommandQueryHandlers(assemblies);

        return services.AddXCommandOptions(configureOptions);
    }

    /// <summary>
    /// Configures the <see cref="ICommandHandler{TCommand}"/>
    /// <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> options behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureOptions">A delegate to configure the 
    /// <see cref="CommandOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXCommandOptions(
        this IServiceCollection services,
        Action<CommandOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        CommandOptions definedOptions = new();
        configureOptions.Invoke(definedOptions);

        if (definedOptions.IsPersistenceEnabled)
        {
            _ = services.AddXPersistenceCommandDecorator();
        }

        if (definedOptions.IsTransactionEnabled)
        {
            _ = services.AddXTransactionCommandDecorator();
        }

        if (definedOptions.IsValidatorEnabled)
        {
            _ = services.AddXValidatorDecorators();
        }

        if (definedOptions.IsVisitorEnabled)
        {
            _ = services.AddXVisitorDecorators();
        }

        if (definedOptions.IsOperationFinalizerEnabled)
        {
            _ = services.AddXOperationFinalizerDecorator();
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
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXPersistenceCommandDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(ICommandHandler<>),
                typeof(PersistenceCommandDecorator<>),
                typeof(IPersistenceDecorator));

        return services;
    }

    /// <summary>
    /// Registers transactional scope behavior to the serviceswith scope 
    /// life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXTransactionStoreCommand
        <TTransactionStoreCommand>(
        this IServiceCollection services)
        where TTransactionStoreCommand : class, ICommandTransactional
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<ICommandTransactional, TTransactionStoreCommand>();
    }

    /// <summary>
    /// Registers transaction scope behavior to commands that 
    /// are decorated with the <see cref="ITransactionDecorator"/>
    /// to the services
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXTransactionCommandDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(ICommandHandler<>),
                typeof(TransactionCommandDecorator<>),
                typeof(ITransactionDecorator));

        return services;
    }

    /// <summary>
    /// Registers the default persistence command handler delegate that does 
    /// nothing, to be used to apply persistence to commands decorated with 
    /// <see cref="IPersistenceDecorator"/>.
    /// The delegate is use by the 
    /// <see cref="PersistenceCommandDecorator{TCommand}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// .</exception>
    /// <remarks>To be used when persistence is managed differently.</remarks>
    public static IServiceCollection AddXPersistenceCommandHandler(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<PersistenceCommandHandler>(
            _ => ct => ValueTask.FromResult(OperationResults.Ok().Build()));

        return services;
    }

    /// <summary>
    /// Registers the persistence command handler delegate to be used to 
    /// apply persistence to commands decorated with 
    /// <see cref="IPersistenceDecorator"/>.
    /// The delegate is use by the 
    /// <see cref="PersistenceCommandDecorator{TCommand}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="persistenceHandlerBuilder">The persistence command handler 
    /// factory.</param>
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
    /// Registers validation behavior to commands and queries 
    /// that are decorated with the <see cref="IValidateDecorator"/> to 
    /// the services with transient life time.
    /// <see cref="ValidatorAsyncQueryDecorator{TQuery, TResult}"/>,
    /// <see cref="ValidatorCommandDecorator{TCommand}"/>,
    /// <see cref="ValidatorQueryDecorator{TQuery, TResult}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXValidatorDecorators(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(ICommandHandler<>),
                typeof(ValidatorCommandDecorator<>),
                typeof(IValidateDecorator));
        _ = services.XTryDecorate(
                typeof(IAsyncQueryHandler<,>),
                typeof(ValidatorAsyncQueryDecorator<,>),
                typeof(IValidateDecorator));
        _ = services.XTryDecorate(
                typeof(IQueryHandler<,>),
                typeof(ValidatorQueryDecorator<,>),
                typeof(IValidateDecorator));

        return services;
    }

    /// <summary>
    /// Adds visitor behavior to commands and queries that are decorated with 
    /// the <see cref="IVisitable"/> interface to the services
    /// with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXVisitorDecorators(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(ICommandHandler<>),
                typeof(VisitorCommandDecorator<>),
                typeof(IVisitorDecorator));
        _ = services.XTryDecorate(
                typeof(IAsyncQueryHandler<,>),
                typeof(VisitorAsyncQueryDecorator<,>),
                typeof(IVisitorDecorator));
        _ = services.XTryDecorate(
                typeof(IQueryHandler<,>),
                typeof(VisitorQueryDecorator<,>),
                typeof(IVisitorDecorator));
        return services;
    }

    /// <summary>
    /// Adds operation result correlation behavior to commands and queries 
    /// that are decorated with the <see cref="IOperationFinalizerDecorator"/> 
    /// to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXOperationFinalizerDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(ICommandHandler<>),
                typeof(OperationFinalizerCommandDecorator<>),
                typeof(IOperationFinalizerDecorator));
        _ = services.XTryDecorate(
                typeof(IAsyncQueryHandler<,>),
                typeof(OperationFinalizerAsyncQueryDecorator<,>),
                typeof(IOperationFinalizerDecorator));
        _ = services.XTryDecorate(
                typeof(IQueryHandler<,>),
                typeof(OperationFinalizerQueryDecorator<,>),
                typeof(IOperationFinalizerDecorator));

        return services;
    }

    /// <summary>
    /// Adds the implementation of <see cref="IOperationFinalizer"/> to 
    /// the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXOperationResultFinalizer(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IOperationFinalizer, OperationFinalizerInternal>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="ICommandHandler{TCommand, TAggregate}"/> 
    /// decorator that provide with the target aggregate instance using the
    /// Decider pattern for command that are decorated with the
    /// <see cref="ICommandAggregate"/> interface.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXAggregateCommandDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(ICommandHandler<,>),
                typeof(AggregateCommandDecorator<,>),
                typeof(ICommandAggregate));

        return services;
    }

    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping all original 
    /// registered handlers type for which the command/query implementing 
    /// <see cref="IInterceptorDecorator"/> found in the specified collection 
    /// of assemblies.
    /// </summary>
    /// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXInterceptorHandlers<TInterceptor>(
        this IServiceCollection services,
        params Assembly[] assemblies)
        where TInterceptor : class, IInterceptor
        => services.AddXInterceptorHandlers(typeof(TInterceptor), assemblies);

    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping all original 
    /// registered handlers type for which the command/query implementing 
    /// <see cref="IInterceptorDecorator"/> found in the specified collection 
    /// of assemblies.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="interceptorType">The interceptor type that will be used 
    /// to wrap the original service type
    /// and should implement <see cref="IInterceptor"/>.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    /// <exception cref="ArgumentException">The 
    /// <paramref name="interceptorType"/> 
    /// must implement <see cref="IInterceptor"/>.</exception>
    public static IServiceCollection AddXInterceptorHandlers(
        this IServiceCollection services,
        Type interceptorType,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(interceptorType);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (!typeof(IInterceptor).IsAssignableFrom(interceptorType))
        {
            throw new ArgumentException($"{nameof(interceptorType)} must " +
                $"implement {nameof(IInterceptor)}.");
        }

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        Type[] genericHandlerInterfaceTypes =
        [
            typeof(IQueryHandler<,>),
            typeof(ICommandHandler<>),
        ];

        var handlers = assemblies
            .SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                && !type.IsInterface
                && !type.IsGenericType
                && Array.Exists(
                    type.GetInterfaces(),
                    i => i.IsGenericType
                    && genericHandlerInterfaceTypes
                        .Contains(i.GetGenericTypeDefinition())
                    && Array.Exists(i.GetGenericArguments(),
                        a => typeof(IInterceptorDecorator)
                            .IsAssignableFrom(a))))
            .Select(type => new
            {
                Type = type,
                Interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType
                        && genericHandlerInterfaceTypes
                            .Contains(i.GetGenericTypeDefinition()))
            });

        _ = services.AddTransient(interceptorType);
        foreach (var hander in handlers)
        {
            foreach (Type typeInterface in hander.Interfaces)
            {
                _ = services.XTryDecorate(typeInterface, (instance, provider) =>
                {
                    IInterceptor interceptor = (IInterceptor)provider
                        .GetRequiredService(interceptorType);
                    return InterceptorFactory
                        .CreateProxy(typeInterface, interceptor, instance);
                });
            }
        }

        return services;
    }

}
