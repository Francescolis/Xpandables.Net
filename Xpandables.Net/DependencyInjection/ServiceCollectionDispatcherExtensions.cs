﻿/*******************************************************************************
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

using Xpandables.Net.Executions.Deciders;
using Xpandables.Net.Executions.Pipelines;
using Xpandables.Net.Executions.Tasks;

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
    /// Registers a pipeline request handler of the specified type to the
    /// service collection.
    /// <para>The pipeline request handler is used to handle requests with a pipeline.</para>
    /// </summary>
    /// <param name="type">The type of the pipeline request handler to register.</param>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified type does not
    /// match the <see cref="IPipelineRequestHandler{TRequest, TResponse}"/> interface.</exception>
    public static IServiceCollection AddXPipelineRequestHandler(
        this IServiceCollection services, Type type)
    {
        if (!type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IPipelineRequestHandler<,>)))
        {
            throw new InvalidOperationException(
                $"{type.Name} does not implement IPipelineRequestHandler<,> interface.");
        }

        return services.AddTransient(typeof(IPipelineRequestHandler<,>), type);
    }

    /// <summary>
    /// Registers the default pipeline request handler to the service collection.
    /// <para>The pipeline request handler is used to handle requests with a pipeline.</para>
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineRequestHandler(
        this IServiceCollection services)
        => services.AddXPipelineRequestHandler(typeof(PipelineRequestHandler<,>));

    /// <summary>
    /// Registers a pipeline stream request handler of the specified type to the
    /// service collection.
    /// <para>The pipeline stream request handler is used to handle stream requests with a pipeline.</para>
    /// </summary>
    /// <param name="type">The type of the pipeline stream request handler to register.</param>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified type does not
    /// match the <see cref="IPipelineStreamRequestHandler{TRequest, TResponse}"/> interface.</exception>
    public static IServiceCollection AddXPipelineStreamRequestHandler(
        this IServiceCollection services, Type type)
    {
        if (!type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IPipelineStreamRequestHandler<,>)))
        {
            throw new InvalidOperationException(
                $"{type.Name} does not implement IPipelineStreamRequestHandler<,> interface.");
        }

        return services.AddTransient(typeof(IPipelineStreamRequestHandler<,>), type);
    }

    /// <summary>
    /// Registers the default pipeline stream request handler to the service collection.
    /// <para>The pipeline stream request handler is used to handle stream requests with a pipeline.</para>
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineStreamRequestHandler(
        this IServiceCollection services) =>
        services.AddXPipelineStreamRequestHandler(typeof(PipelineStreamRequestHandler<,>));

    /// <summary>
    /// Adds a defaults dispatcher and pipeline request handler to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the dispatcher to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDispatcher(
        this IServiceCollection services) =>
        services
            .AddXDispatcher<Dispatcher>()
            .AddXPipelineRequestHandler()
            .AddXPipelineStreamRequestHandler();

    internal readonly record struct HandlerType(
        Type Type,
        IEnumerable<Type> Interfaces);

    /// <summary>
    /// Adds handlers to the <see cref="IServiceCollection"/> with scoped lifetime.
    /// </summary>
    /// <param name="services">The service collection to add the dispatcher 
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
                    (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)))))
            .Select(type => new HandlerType(
                Type: type,
                Interfaces: type.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IHandler<,>)))));

        foreach (HandlerType handlerType in handlerTypes)
        {
            foreach (Type interfaceType in handlerType.Interfaces)
            {
                _ = services.AddScoped(interfaceType, handlerType.Type);
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
    public static IServiceCollection AddXDeciderDependencyManager(
        this IServiceCollection services) =>
        services.AddScoped<IDeciderDependencyManager, DeciderDependencyManager>();

    /// <summary>
    /// Adds a decider dependency provider of type <typeparamref name="TService"/> to 
    /// the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the decider dependency 
    /// provider to add.</typeparam>
    /// <param name="services">The service collection to add the decider 
    /// dependency provider to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXDeciderDependencyProvider<TService>(
        this IServiceCollection services)
        where TService : class, IDeciderDependencyProvider =>
        services.AddScoped<IDeciderDependencyProvider, TService>();

    /// <summary>
    /// Adds the aggregate decider dependency provider to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decider 
    /// dependency provider to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAggregateDependencyProvider(
        this IServiceCollection services) =>
        services.AddXDeciderDependencyProvider<AggregateDeciderDependencyProvider>();

    /// <summary>
    /// Adds an aggregate pipeline decorator to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineAggregateDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineAggregateDecorator<,>));

    /// <summary>
    /// Adds a command pipeline decorator to the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineDeciderDecorator(
        this IServiceCollection services) =>
        services.AddXPipelineDecorator(typeof(PipelineDeciderDecorator<,>));

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
            .AddXPipelineDecorator(typeof(PipelineValidationDecorator<,>))
            .AddXPipelineStreamDecorator(typeof(PipelineStreamValidationDecorator<,>));

    /// <summary>
    /// Adds an exception pipeline decorator to the <see cref="IServiceCollection"/>.
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

        return services.AddScoped(typeof(IPipelineDecorator<,>), pipelineType);
    }

    /// <summary>
    /// Registers a pipeline stream decorator of the specified type to 
    /// the <see cref="IServiceCollection"/>.
    /// <para>The pipeline decorator is applied in the order of registration.</para>
    /// </summary>
    /// <param name="pipelineType">The type of the pipeline decorator to register.</param>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified type does not
    /// match the <see cref="IPipelineStreamDecorator{TRequest, TResponse}"/> interface.</exception>
    public static IServiceCollection AddXPipelineStreamDecorator(
        this IServiceCollection services, Type pipelineType)
    {
        if (!pipelineType.GetInterfaces().Any(i =>
            i.IsGenericType
            && i.GetGenericTypeDefinition() == typeof(IPipelineStreamDecorator<,>)))
        {
            throw new InvalidOperationException(
                $"{pipelineType.Name} does not implement IPipelineStreamDecorator<,> interface.");
        }
        return services.AddScoped(typeof(IPipelineStreamDecorator<,>), pipelineType);
    }
}
