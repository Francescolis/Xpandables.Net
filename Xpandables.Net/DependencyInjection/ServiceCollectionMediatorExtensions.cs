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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Xpandables.Net.Executions.Deciders;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Pipelines;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.States;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding mediator services to the 
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionMediatorExtensions
{
    /// <summary>
    /// Adds a mediator of type <typeparamref name="TMediator"/> to 
    /// the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TMediator">The type of the mediator to add.</typeparam>
    /// <param name="services">The service collection to add the mediator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXMediator<TMediator>(
        this IServiceCollection services)
        where TMediator : class, IMediator =>
        services
            .AddScoped<IMediator, TMediator>()
            .AddXPipelineRequestHandler();

    /// <summary>
    /// Adds a defaults mediator and pipeline request handler to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the mediator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXMediator(
        this IServiceCollection services) =>
        services.AddXMediator<Mediator>();

    /// <summary>
    /// Registers a pipeline request handler of the specified type to the
    /// service collection.
    /// <para>The pipeline request handler is used to handle requests with a pipeline.</para>
    /// </summary>
    /// <param name="type">The type of the pipeline request handler to register.</param>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified type does not
    /// match the <see cref="IPipelineRequestHandler{TRequest}"/> interface.</exception>
    public static IServiceCollection AddXPipelineRequestHandler(
        this IServiceCollection services, Type type)
    {
        if (!type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IPipelineRequestHandler<>)))
        {
            throw new InvalidOperationException(
                $"{type.Name} does not implement IPipelineRequestHandler<,> interface.");
        }

        return services.AddTransient(typeof(IPipelineRequestHandler<>), type);
    }

    /// <summary>
    /// Registers the default pipeline request handler to the service collection.
    /// <para>The pipeline request handler is used to handle requests with a pipeline.</para>
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineRequestHandler(
        this IServiceCollection services)
        => services.AddXPipelineRequestHandler(typeof(PipelineRequestHandler<>));

    internal readonly record struct HandlerType(
        Type Type,
        IEnumerable<Type> Interfaces);

    /// <summary>
    /// Adds handlers to the <see cref="IServiceCollection"/> with scoped lifetime.
    /// </summary>
    /// <param name="services">The service collection to add the mediator 
    /// handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        IEnumerable<HandlerType> handlerTypes = assemblies.SelectMany(assembly =>
            assembly.GetTypes()
                .Where(type =>
                type is
                {
                    IsClass: true,
                    IsAbstract: false,
                    IsSealed: true,
                    IsGenericType: false
                }
                && type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))))
            .Select(type => new HandlerType(
                Type: type,
                Interfaces: type.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))));

        foreach (HandlerType handlerType in handlerTypes)
        {
            foreach (Type interfaceType in handlerType.Interfaces)
            {
                _ = services.AddTransient(interfaceType, handlerType.Type);
            }
        }

        return services;
    }

    /// <summary>
    /// Adds a decider dependency manager to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decider 
    /// dependency provider to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDependencyManager(
        this IServiceCollection services) =>
        services.AddScoped<IDependencyManager, DependencyManager>();

    /// <summary>
    /// Adds a decider dependency provider of type <typeparamref name="TService"/> to 
    /// the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the decider dependency 
    /// provider to add.</typeparam>
    /// <param name="services">The service collection to add the decider 
    /// dependency provider to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDependencyProvider<TService>(
        this IServiceCollection services)
        where TService : class, IDependencyProvider =>
        services.AddScoped<IDependencyProvider, TService>();

    /// <summary>
    /// Adds an pipeline decorator to the <see cref="IServiceCollection"/> that append the ambient aggregate root.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineAppenderDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineAppenderDecorator<,>));

    /// <summary>
    /// Adds an pipeline decorator to the <see cref="IServiceCollection"/> that resolve
    /// the ambient aggregate root before request is processed.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineResolverDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineResolverDecorator<,>));

    /// <summary>
    /// Adds the dependency pipeline decorator to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineDependencyDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineDependencyDecorator<,>));

    /// <summary>
    /// Adds a unit of work pipeline decorator to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineUnitOfWorkDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineUnitOfWorkDecorator<,>));

    /// <summary>
    /// Adds a validation pipeline decorator to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineValidationDecorator(
        this IServiceCollection services) =>
        services
            .AddXPipelineDecorator(typeof(PipelineValidationDecorator<,>));

    /// <summary>
    /// Adds an exception pipeline decorator to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineExceptionDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineExceptionDecorator<,>));

    /// <summary>
    /// Registers a pipeline decorator of the specified type to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <remarks>The pipeline decorator must implement the 
    /// <see cref="IPipelineDecorator{TRequest, TResponse}"/> interface.</remarks>
    /// <param name="pipelineType">The type of the pipeline decorator to register.</param>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified type does not
    /// match the <see cref="IPipelineDecorator{TRequest, TResponse}"/> interface.</exception>
    public static IServiceCollection AddXPipelineDecorator(
        this IServiceCollection services, Type pipelineType)
    {
        if (!pipelineType.GetInterfaces().Any(i =>
            i.IsGenericType
            && i.GetGenericTypeDefinition() == typeof(IPipelineDecorator<,>)))
        {
            throw new InvalidOperationException(
                $"{pipelineType.Name} does not implement IPipelineDecorator<,> interface.");
        }

        return services.AddTransient(typeof(IPipelineDecorator<,>), pipelineType);
    }

    /// <summary>
    /// Adds the aggregate snapshot store to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the aggregate 
    /// snapshot store to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXSnapshotAggregateStore(
        this IServiceCollection services) =>
        services.XTryDecorate(
            typeof(IAggregateStore<>),
            typeof(SnapShotAggregateStore<>),
            typeof(IOriginator));

    /// <summary>
    /// Adds the specified aggregate store implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// aggregate store to.</param>
    /// <param name="aggregateStoreType">The type of the aggregate store 
    /// implementation.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services,
        Type aggregateStoreType)
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IAggregateStore<>),
                aggregateStoreType,
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the default aggregate store implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// aggregate store to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAggregateStore(
        this IServiceCollection services) =>
        services.AddXAggregateStore(typeof(AggregateStore<>));

    /// <summary>
    /// Adds the specified event store implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEventStore">The type of the event store 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event store to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventStore<TEventStore>(
        this IServiceCollection services)
        where TEventStore : class, IEventStore
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventStore),
                typeof(TEventStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the specified publisher implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TPublisher">The type of the publisher 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// publisher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPublisher<TPublisher>(
        this IServiceCollection services)
        where TPublisher : class, IPublisher
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IPublisher),
                typeof(TPublisher),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the default publisher implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// publisher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPublisher(
        this IServiceCollection services) =>
        services.AddXPublisher<PublisherSubscriber>();

    /// <summary>
    /// Adds the specified subscriber implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the subscriber 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// subscriber to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXSubscriber<TSubscriber>(
        this IServiceCollection services)
        where TSubscriber : class, ISubscriber
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(ISubscriber),
                typeof(TSubscriber),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Adds the default subscriber implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// subscriber to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXSubscriber(
        this IServiceCollection services) =>
        services.AddXSubscriber<PublisherSubscriber>();

    /// <summary>
    /// Adds the specified scheduler implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TScheduler">The type of the event scheduler 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXScheduler<TScheduler>(
        this IServiceCollection services)
        where TScheduler : class, IScheduler
    {
        services.TryAdd(
            new ServiceDescriptor(
                typeof(IScheduler),
                typeof(TScheduler),
                ServiceLifetime.Singleton));

        return services;
    }

    /// <summary>
    /// Adds the default scheduler implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the
    /// scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXScheduler(
        this IServiceCollection services) =>
        services.AddXScheduler<Scheduler>();

    /// <summary>
    /// Adds the specified scheduler implementation that also implements
    /// <see cref="IHostedService"/> to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TScheduler">The type of the scheduler 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXSchedulerHosted<TScheduler>(
        this IServiceCollection services)
        where TScheduler : class, IScheduler, IHostedService =>
        services
            .AddXScheduler<TScheduler>()
            .AddHostedService(provider =>
                provider.GetRequiredService<IScheduler>());

    /// <summary>
    /// Adds the default hosted scheduler implementation that also implements
    /// <see cref="IHostedService"/> to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the hosted
    /// scheduler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXSchedulerHosted(
        this IServiceCollection services) =>
        services.AddXSchedulerHosted<Scheduler>();

    /// <summary>
    /// Adds the specified event handler implementation to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <typeparam name="TEventHandler">The type of the event handler 
    /// implementation.</typeparam>
    /// <param name="services">The service collection to add the
    /// event handler to.</param>
    /// <param name="factory">The factory method to create the event handler 
    /// instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TEventHandler>? factory)
        where TEvent : notnull, IEvent
        where TEventHandler : class, IEventHandler<TEvent>
    {
        if (factory is not null)
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IEventHandler<TEvent>),
                    factory,
                    ServiceLifetime.Scoped));
        }
        else
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IEventHandler<TEvent>),
                    typeof(TEventHandler),
                    ServiceLifetime.Scoped));
        }

        return services;
    }

    internal static readonly MethodInfo AddEventHandlerMethod =
        typeof(ServiceCollectionMediatorExtensions)
        .GetMethod(nameof(AddXEventHandler))!;

    /// <summary>
    /// Adds event handlers from the specified assemblies 
    /// to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the event 
    /// handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for event handlers.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var eventHandlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsSealed: true }
                && type.GetInterfaces().Any(@interface =>
                    @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .Select(type => new
            {
                InterfaceTypes = type.GetInterfaces()
                    .Where(@interface =>
                        @interface.IsGenericType
                        && @interface.GetGenericTypeDefinition() == typeof(IEventHandler<>)),
                EventHandlerType = type
            });

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            foreach (Type interfaceType in eventHandlerType.InterfaceTypes)
            {
                MethodInfo addEventHandlerMethod = AddEventHandlerMethod
                    .MakeGenericMethod(
                        interfaceType.GetGenericArguments()[0],
                        eventHandlerType.EventHandlerType);

                _ = addEventHandlerMethod.Invoke(null, [services, null]);
            }
        }

        return services;
    }
}
