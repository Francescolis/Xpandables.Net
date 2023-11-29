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
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.CQRS;
using Xpandables.Net.CQRS.Decorators;
using Xpandables.Net.Interception;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Validators;
using Xpandables.Net.Visitors;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register cqrs services.
/// </summary>
public static class ServiceCollectionCQRSExtensions
{
    /// <summary>
    /// Registers and configures the <see cref="ICommandHandler{TCommand}"/>, 
    /// <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="CQRSOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXCQRSHandlers(
        this IServiceCollection services,
        Action<CQRSOptions> configureOptions,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        services.AddXCQRSHandlers(assemblies);

        return services.AddXCQRSOptions(configureOptions, assemblies);
    }

    /// <summary>
    /// Configures the <see cref="ICommandHandler{TCommand}"/>
    /// <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and <see cref="IAsyncQueryHandler{TQuery, TResult}"/> options behaviors.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="CQRSOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXCQRSOptions(
        this IServiceCollection services,
        Action<CQRSOptions> configureOptions,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        var definedOptions = new CQRSOptions();
        configureOptions.Invoke(definedOptions);

        if (definedOptions.IsPersistenceEnabled)
        {
            services.AddXPersistenceCommandDecorator();
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

        if (definedOptions.IsOperationResultEnabled)
        {
            services.AddXOperationResultDecorator();
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

    /// <summary>
    /// Registers validation behavior to commands and queries 
    /// that are decorated with the <see cref="IValidateDecorator"/> to the services
    /// with transient life time.
    /// <see cref="ValidatorAsyncQueryDecorator{TQuery, TResult}"/>,
    /// <see cref="ValidatorCommandDecorator{TCommand}"/>,
    /// <see cref="ValidatorQueryDecorator{TQuery, TResult}"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXValidatorDecorators(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(ICommandHandler<>), typeof(ValidatorCommandDecorator<>));
        services.XTryDecorate(typeof(IAsyncQueryHandler<,>), typeof(ValidatorAsyncQueryDecorator<,>));
        services.XTryDecorate(typeof(IQueryHandler<,>), typeof(ValidatorQueryDecorator<,>));

        return services;
    }

    /// <summary>
    /// Adds visitor behavior to commands and queries that are decorated with 
    /// the <see cref="IVisitable"/> interface to the services
    /// with transient life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXVisitorDecorators(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(ICommandHandler<>), typeof(VisitorCommandDecorator<>));
        services.XTryDecorate(typeof(IAsyncQueryHandler<,>), typeof(VisitorAsyncQueryDecorator<,>));
        services.XTryDecorate(typeof(IQueryHandler<,>), typeof(VisitorQueryDecorator<,>));
        return services;
    }

    /// <summary>
    /// Adds operation result correlation behavior to commands and queries 
    /// that are decorated with the <see cref="IOperationResultDecorator"/> to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultDecorator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(ICommandHandler<>), typeof(OperationResultCommandDecorator<>));
        services.XTryDecorate(typeof(IAsyncQueryHandler<,>), typeof(OperationResultAsyncQueryDecorator<,>));
        services.XTryDecorate(typeof(IQueryHandler<,>), typeof(OperationResultQueryDecorator<,>));

        return services;
    }

    /// <summary>
    /// Adds the implementation of <see cref="IOperationResultFinalizer"/> to 
    /// the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultFinalizer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IOperationResultFinalizer, OperationResultFinalizerInternal>();
        return services;
    }

    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping all original registered handlers
    /// type for which the command/query implementing <see cref="IInterceptorDecorator"/> 
    /// found in the specified collection of assemblies.
    /// </summary>
    /// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXInterceptorHandlers<TInterceptor>(
        this IServiceCollection services,
        params Assembly[] assemblies)
        where TInterceptor : class, IInterceptor
        => services.AddXInterceptorHandlers(typeof(TInterceptor), assemblies);

    /// <summary>
    /// Ensures that the supplied interceptor is returned, wrapping all original registered handlers
    /// type for which the command/query implementing <see cref="IInterceptorDecorator"/> 
    /// found in the specified collection of assemblies.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="interceptorType">The interceptor type that will be used 
    /// to wrap the original service type
    /// and should implement <see cref="IInterceptor"/>.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="interceptorType"/> 
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
            throw new ArgumentException($"{nameof(interceptorType)} must implement {nameof(IInterceptor)}.");

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        var genericHandlerInterfaceTypes = new[]
        {
            typeof(IQueryHandler<,>),
            typeof(ICommandHandler<>),
        };

        var handlers = assemblies
            .SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                && !type.IsInterface
                && !type.IsGenericType
                && Array.Exists(
                    type.GetInterfaces(),
                    i => i.IsGenericType && genericHandlerInterfaceTypes.Contains(i.GetGenericTypeDefinition())
                    && Array.Exists(i.GetGenericArguments(), a => typeof(IInterceptorDecorator).IsAssignableFrom(a))))
            .Select(type => new
            {
                Type = type,
                Interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType
                        && genericHandlerInterfaceTypes.Contains(i.GetGenericTypeDefinition()))
            });

        services.AddTransient(interceptorType);
        foreach (var hander in handlers)
            foreach (var typeInterface in hander.Interfaces)
            {
                services.XTryDecorate(typeInterface, (instance, provider) =>
                {
                    var interceptor = (IInterceptor)provider.GetRequiredService(interceptorType);
                    return InterceptorFactory.CreateProxy(typeInterface, interceptor, instance);
                });
            }

        return services;
    }

}
