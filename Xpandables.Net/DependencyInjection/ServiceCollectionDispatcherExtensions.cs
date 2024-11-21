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

using Xpandables.Net.Commands;
using Xpandables.Net.Commands.Wrappers;
using Xpandables.Net.Operations;
using Xpandables.Net.Pipelines;

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

    internal readonly record struct HandlerType(
        Type Type,
        IEnumerable<Type> Interfaces);

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
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                    i.GetGenericTypeDefinition() == typeof(IQueryAsyncHandler<,>)))))
            .Select(type => new HandlerType(
                Type: type,
                Interfaces: type.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IQueryAsyncHandler<,>)))));

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
    /// Adds a decider dependency provider of type <typeparamref name="TService"/> to 
    /// the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the decider dependency 
    /// provider to add.</typeparam>
    /// <param name="services">The service collection to add the decider 
    /// dependency provider to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXCommandDeciderDependencyProvider<TService>(
        this IServiceCollection services)
        where TService : class, ICommandDeciderDependencyProvider =>
        services.AddScoped<ICommandDeciderDependencyProvider, TService>();

    /// <summary>
    /// Adds an aggregate pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineAggregateDeciderDecorator(
        this IServiceCollection services) =>
        services.AddScoped(
            typeof(IPipelineDecorator<,>),
            typeof(PipelineAggregateDeciderDecorator<,>));

    /// <summary>
    /// Adds a command pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineDeciderDecorator(
        this IServiceCollection services) =>
        services.AddScoped(
            typeof(IPipelineDecorator<,>),
            typeof(PipelineDeciderDecorator<,>));

    /// <summary>
    /// Adds a unit of work pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineUnitOfWorkDecorator(
        this IServiceCollection services) =>
        services.AddScoped(
            typeof(IPipelineDecorator<,>),
            typeof(PipelineUnitOfWorkDecorator<,>));

    /// <summary>
    /// Adds a validation pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineValidationDecorator(
        this IServiceCollection services) =>
        services.AddScoped(
            typeof(IPipelineDecorator<,>),
            typeof(PipelineValidationDecorator<,>));

    /// <summary>
    /// Adds an exception pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineExceptionDecorator(
        this IServiceCollection services) =>
        services.AddScoped(
            typeof(IPipelineDecorator<,>),
            typeof(PipelineExceptionDecorator<,>));

    /// <summary>
    /// Adds an async exception pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXAsyncPipelineExceptionDecorator(
        this IServiceCollection services) =>
        services.AddScoped(
            typeof(IPipelineAsyncDecorator<,>),
            typeof(PipelineExceptionAsyncDecorator<,>));

    /// <summary>
    /// Adds a finalizer pipeline decorator to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXPipelineFinalizerDecorator(
        this IServiceCollection services) =>
        services
            .AddScoped(
                typeof(IPipelineDecorator<,>),
                typeof(PipelineFinalizerDecorator<,>))
            .AddScoped<IExecutionResultFinalizer, ExecutionResultFinalizer>();
}
