
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
using Xpandables.Net.Decorators;
using Xpandables.Net.Decorators.Internals;
using Xpandables.Net.Events;
using Xpandables.Net.Interceptions;
using Xpandables.Net.Internals;
using Xpandables.Net.Operations;
using Xpandables.Net.Transactions;
using Xpandables.Net.Visitors;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register request services.
/// </summary>
public static class ServiceCollectionDispatcherExtensions
{
    internal static readonly MethodInfo AddRequestHandlerMethod =
        typeof(ServiceCollectionDispatcherExtensions)
        .GetMethod(nameof(AddXRequestHandler))!;

    internal static readonly MethodInfo AddRequestAggregateHandlerMethod =
        typeof(ServiceCollectionDispatcherExtensions)
        .GetMethod(nameof(AddXRequestAggregateHandler))!;

    internal static readonly MethodInfo AddRequestResponseHandlerMethod =
        typeof(ServiceCollectionDispatcherExtensions)
        .GetMethod(nameof(AddXRequestResponseHandler))!;

    internal static readonly MethodInfo AddAsyncRequestResponseHandlerMethod =
        typeof(ServiceCollectionDispatcherExtensions)
        .GetMethod(nameof(AddXAsyncRequestResponseHandler))!;

    /// <summary>
    /// Registers the <typeparamref name="TDistributor"/> type 
    /// as <see cref="IDispatcher"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TDistributor">The type that implements 
    /// <see cref="IDispatcher"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDispatcher<TDistributor>(
        this IServiceCollection services)
        where TDistributor : class, IDispatcher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IDispatcher, TDistributor>();
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
    /// Registers the <typeparamref name="TRequestHandler"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the request handler using the 
    /// <see cref="RequestAggregateHandler{TRequest, TAggregate}"/> delegate.</remarks>
    /// <typeparam name="TAggregate">The type of aggregate.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TRequestHandler">The type of the request 
    /// handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that 
    /// creates the request handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXRequestAggregateHandler
        <TRequest, TAggregate, TRequestHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TRequestHandler>? implementationHandlerFactory
        = default)
        where TAggregate : class, IAggregate
        where TRequestHandler : class, IRequestAggregateHandler<TRequest, TAggregate>
        where TRequest : class, IRequestAggregate<TAggregate>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <IRequestAggregateHandler<TRequest, TAggregate>, TRequestHandler>(
            implementationHandlerFactory);

        _ = services.AddScoped<RequestAggregateHandler<TRequest, TAggregate>>(
            provider => provider
                .GetRequiredService<IRequestAggregateHandler<TRequest, TAggregate>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers all <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/> 
    /// implementations found in 
    /// the assemblies to the services with scope life time.
    /// </summary>
    /// <remarks>You can refer to the request handler using the 
    /// <see cref="RequestAggregateHandler{TRequest, TAggregate}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXRequestAggregateHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXRequestHandlerWrappers();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IRequestAggregateHandler<,>),
            AddRequestAggregateHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the request handler wrappers necessary to resolve 
    /// handlers using type inference.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXRequestHandlerWrappers(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(RequestAggregateHandlerWrapper<,>));
        services.TryAddTransient(typeof(RequestResponseHandlerWrapper<,>));
        services.TryAddTransient(typeof(AsyncRequestResponseHandlerWrapper<,>));
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TRequestHandler"/> to the services with 
    /// scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the request handler using the 
    /// <see cref="RequestHandler{TRequest}"/> delegate.</remarks>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TRequestHandler">The type of the request 
    /// handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that 
    /// creates the request handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXRequestHandler<TRequest, TRequestHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TRequestHandler>? implementationHandlerFactory
        = default)
        where TRequestHandler : class, IRequestHandler<TRequest>
        where TRequest : notnull, IRequest
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <IRequestHandler<TRequest>, TRequestHandler>(
            implementationHandlerFactory);

        _ = services.AddScoped<RequestHandler<TRequest>>(
            provider => provider
                .GetRequiredService<IRequestHandler<TRequest>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers all <see cref="IRequestHandler{TRequest}"/> 
    /// implementations found in 
    /// the assemblies to the services with scope life time.
    /// </summary>
    /// <remarks>You can refer to the request handler using the 
    /// <see cref="RequestHandler{TRequest}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXRequestHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IRequestHandler<>),
            AddRequestHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the <typeparamref name="TRequestHandler"/> to the 
    /// services with scope 
    /// life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the request handler using the 
    /// <see cref="RequestHandler{TRequest, TResponse}"/> delegate.</remarks>
    /// <typeparam name="TRequest">Type of the request that will 
    /// be used as argument.</typeparam>
    /// <typeparam name="TResponse">Type of the response of the request.</typeparam>
    /// <typeparam name="TRequestHandler">The type of the request handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationQueryFactory">The factory that 
    /// creates the request handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXRequestResponseHandler<TRequest, TResponse, TRequestHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TRequestHandler>? implementationQueryFactory
        = default)
        where TRequest : notnull, IRequest<TResponse>
        where TRequestHandler : class, IRequestHandler<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime
            <IRequestHandler<TRequest, TResponse>, TRequestHandler>(
            implementationQueryFactory);

        _ = services.AddScoped<RequestHandler<TRequest, TResponse>>(
            provider => provider
                .GetRequiredService<IRequestHandler<TRequest, TResponse>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IRequestHandler{TRequest, TResponse}"/> implementations to 
    /// the services with scoped life time.
    /// </summary>
    /// <remarks>You can refer to the request handler 
    /// using the <see cref="RequestHandler{TRequest, TResponse}"/> delegate.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// if not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    /// <remarks>The request wrapper is also registered.</remarks>
    public static IServiceCollection AddXRequestResponseHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies =
                [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXRequestHandlerWrappers();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IRequestHandler<,>),
            AddRequestResponseHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the <typeparamref name="TAsyncRequestHandler"/> to the 
    /// services with  scope life time using the factory if specified.
    /// </summary>
    /// <remarks>You can refer to the request handler 
    /// using the <see cref="AsyncRequestHandler{TRequest, TResponse}"/> delegate
    /// .</remarks>
    /// <typeparam name="TAsyncRequest">Type of the request that will be used as 
    /// argument.</typeparam>
    /// <typeparam name="TResponse">Type of the response of the request.</typeparam>
    /// <typeparam name="TAsyncRequestHandler">The type of the request handler
    /// .</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationAsyncQueryFactory">The factory that creates 
    /// the request handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXAsyncRequestResponseHandler
        <TAsyncRequest, TResponse, TAsyncRequestHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TAsyncRequestHandler>?
        implementationAsyncQueryFactory = default)
        where TAsyncRequest : notnull, IAsyncRequest<TResponse>
        where TAsyncRequestHandler : class, IAsyncRequestHandler<TAsyncRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.DoRegisterTypeServiceLifeTime<IAsyncRequestHandler
            <TAsyncRequest, TResponse>, TAsyncRequestHandler>(
            implementationAsyncQueryFactory);

        _ = services.AddScoped<AsyncRequestHandler<TAsyncRequest, TResponse>>(
            provider => provider
                .GetRequiredService<IAsyncRequestHandler<TAsyncRequest, TResponse>>()
                .HandleAsync);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> 
    /// implementations to the services with scoped life time.
    /// </summary>
    /// <remarks>You can refer to the request handler 
    /// using the <see cref="AsyncRequestHandler{TRequest, TResponse}"/> delegate
    /// .</remarks>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// if not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    /// <remarks>The request wrapper is also registered.</remarks>
    public static IServiceCollection AddXAsyncRequestResponseHandlers(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXRequestHandlerWrappers();

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IAsyncRequestHandler<,>),
            AddAsyncRequestResponseHandlerMethod,
            assemblies);
    }


    /// <summary>
    /// Registers and configures the <see cref="IRequestHandler{TRequest}"/>, 
    /// <see cref="IRequestHandler{TRequest, TResponse}"/>,
    /// <see cref="IEventHandler{TEvent}"/>
    /// and <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXRequestAggregateHandlers(assemblies);
        _ = services.AddXRequestHandlers(assemblies);
        _ = services.AddXRequestResponseHandlers(assemblies);
        _ = services.AddXAsyncRequestResponseHandlers(assemblies);
        _ = services.AddXEventHandlers(assemblies);

        return services;
    }

    /// <summary>
    /// Registers and configures the <see cref="IRequestHandler{TRequest}"/>, 
    /// <see cref="IRequestHandler{TRequest, TResponse}"/>,
    /// <see cref="IEventHandler{TEvent}"/>,
    /// <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> and
    /// <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <param name="configureOptions">A delegate to configure the 
    /// <see cref="RequestOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXHandlers(
        this IServiceCollection services,
        Action<RequestOptions> configureOptions,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        _ = services.AddXHandlers(assemblies);

        return services.AddXRequestOptions(configureOptions);
    }

    /// <summary>
    /// Configures the <see cref="IRequestHandler{TRequest}"/>
    /// <see cref="IRequestHandler{TRequest, TResponse}"/>
    /// and <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> options behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureOptions">A delegate to configure the 
    /// <see cref="RequestOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXRequestOptions(
        this IServiceCollection services,
        Action<RequestOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        RequestOptions definedOptions = new();
        configureOptions.Invoke(definedOptions);

        if (definedOptions.IsDuplicateEventEnabled)
        {
            _ = services.AddXEventDuplicateDecorator();
        }

        if (definedOptions.IsAggregateEnabled)
        {
            _ = services.AddXAggregateAccessor();
            _ = services.AddXRequestAggregateHandlerDecorator();
        }

        if (definedOptions.IsPersistenceEnabled)
        {
            _ = services.AddXRequestPersistenceHandlerDecorator();
        }

        if (definedOptions.IsTransactionEnabled)
        {
            _ = services.AddXRequestTransactionalHandlerDecorator();
        }

        if (definedOptions.IsValidatorEnabled)
        {
            _ = services.AddXValidators();
            _ = services.AddXValidatorGenerics();
            _ = services.AddXRequestValidatorDecorators();
        }

        if (definedOptions.IsVisitorEnabled)
        {
            _ = services.AddXVisitors();
            _ = services.AddXVisitorComposite();
            _ = services.AddXRequestVisitorDecorators();
        }

        if (definedOptions.IsOperationFinalizerEnabled)
        {
            _ = services.AddXOperationResultFinalizer();
            _ = services.AddXRequestFinalizerDecorator();
        }

        return services;
    }

    /// <summary>
    /// Registers persistence behavior to requests that are decorated with 
    /// the <see cref="IPersistenceDecorator"/> to the services
    /// with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXRequestPersistenceHandlerDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(IRequestHandler<>),
                typeof(RequestPersistenceHandlerDecorator<>),
                typeof(IPersistenceDecorator));

        return services;
    }

    /// <summary>
    /// Registers transactional request behavior to the services with scope 
    /// life time.
    /// </summary>
    /// <typeparam name="TTransactional">The type of the transactional
    /// instance.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXRequestTransactionalHandlerBehavior
        <TTransactional>(
        this IServiceCollection services)
        where TTransactional : class, ITransactional
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<ITransactional, TTransactional>();
    }

    /// <summary>
    /// Registers transaction scope behavior to requests that 
    /// are decorated with the <see cref="ITransactionDecorator"/>
    /// to the services
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXRequestTransactionalHandlerDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(IRequestHandler<>),
                typeof(RequestTransactionHandlerDecorator<>),
                typeof(ITransactionDecorator));

        return services;
    }

    /// <summary>
    /// Registers the default persistence request handler delegate that does 
    /// nothing, to be used to apply persistence to requests decorated with 
    /// <see cref="IPersistenceDecorator"/>.
    /// The delegate is use by the 
    /// <see cref="RequestPersistenceHandlerDecorator{TRequest}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// .</exception>
    /// <remarks>To be used when persistence is managed differently.</remarks>
    public static IServiceCollection AddXRequestPersistenceHandlerDelegate(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<PersistenceRequestDelegate>(
            _ => async ct =>
            {
                await Task.Yield();
                return OperationResults.Ok().Build();
            });

        return services;
    }

    /// <summary>
    /// Registers the persistence request handler delegate to be used to 
    /// apply persistence to requests decorated with 
    /// <see cref="IPersistenceDecorator"/>.
    /// The delegate is use by the 
    /// <see cref="RequestPersistenceHandlerDecorator{TRequest}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="persistenceDelegateBuilder">The persistence request handler 
    /// factory.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// or <paramref name="persistenceDelegateBuilder"/> is null.</exception>
    public static IServiceCollection AddXRequestPersistenceHandlerDelegate(
        this IServiceCollection services,
        Func<IServiceProvider, PersistenceRequestDelegate> persistenceDelegateBuilder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(persistenceDelegateBuilder);

        services.TryAddScoped(persistenceDelegateBuilder);

        return services;
    }

    /// <summary>
    /// Registers validation behavior to requests 
    /// that are decorated with the <see cref="IValidateDecorator"/> to 
    /// the services with transient life time.
    /// <see cref="AsyncRequestValidatorHandlerDecorator{TRequest, TResponse}"/>,
    /// <see cref="RequestValidatorHandlerDecorator{TRequest}"/>,
    /// <see cref="RequestValidatorHandlerDecorator{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXRequestValidatorDecorators(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(IRequestHandler<>),
                typeof(RequestValidatorHandlerDecorator<>),
                typeof(IValidateDecorator));
        _ = services.XTryDecorate(
                typeof(IAsyncRequestHandler<,>),
                typeof(AsyncRequestValidatorHandlerDecorator<,>),
                typeof(IValidateDecorator));
        _ = services.XTryDecorate(
                typeof(IRequestHandler<,>),
                typeof(RequestValidatorHandlerDecorator<,>),
                typeof(IValidateDecorator));

        return services;
    }

    /// <summary>
    /// Adds visitor behavior to requests that are decorated with 
    /// the <see cref="IVisitable"/> interface to the services
    /// with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXRequestVisitorDecorators(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(IRequestHandler<>),
                typeof(RequestVisitorHandlerDecorator<>),
                typeof(IVisitorDecorator));
        _ = services.XTryDecorate(
                typeof(IAsyncRequestHandler<,>),
                typeof(AsyncRequestVisitorHandlerDecorator<,>),
                typeof(IVisitorDecorator));
        _ = services.XTryDecorate(
                typeof(IRequestHandler<,>),
                typeof(RequestVisitorHandlerDecorator<,>),
                typeof(IVisitorDecorator));
        return services;
    }

    /// <summary>
    /// Adds operation response correlation behavior to requests 
    /// that are decorated with the <see cref="IOperationFinalizerDecorator"/> 
    /// to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXRequestFinalizerDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(IRequestHandler<>),
                typeof(RequestFinalizerHandlerDecorator<>),
                typeof(IOperationFinalizerDecorator));
        _ = services.XTryDecorate(
                typeof(IRequestAggregateHandler<,>),
                typeof(RequestAggregateFinalizerHandlerDecorator<,>),
                typeof(IOperationFinalizerDecorator));
        _ = services.XTryDecorate(
                typeof(IAsyncRequestHandler<,>),
                typeof(AsyncRequestFinalizerHandlerDecorator<,>),
                typeof(IOperationFinalizerDecorator));
        _ = services.XTryDecorate(
                typeof(IRequestHandler<,>),
                typeof(RequestFinalizerHandlerDecorator<,>),
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
    /// Registers the <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/> 
    /// decorator that provide with the target aggregate instance using the
    /// Decider pattern for request that are decorated with the
    /// <see cref="IAggregateDecorator"/> interface.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXRequestAggregateHandlerDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.XTryDecorate(
                typeof(IRequestAggregateHandler<,>),
                typeof(RequestAggregateHandlerDecorator<,>),
                typeof(IAggregateDecorator));

        return services;
    }

    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping all original 
    /// registered handlers type for which the request/request implementing 
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
    /// registered handlers type for which the request/request implementing 
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
            typeof(IRequestHandler<,>),
            typeof(IRequestHandler<>),
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
