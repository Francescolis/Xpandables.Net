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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Events;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding services related to DataContextEvent.
/// </summary>
public static class ServiceCollectionRepositoryExtensions
{
    /// <summary>
    /// Adds the DataContextEvent to the service collection with the specified 
    /// options.
    /// </summary>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionAction">An action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The same service collection so that multiple calls can be 
    /// chained.</returns>
    public static IServiceCollection AddXDataContextEvent(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionAction) =>
        services.AddXDataContext<DataContextEvent>(optionAction);

    /// <summary>
    /// Adds a specified DataContext to the service collection with the specified options.
    /// </summary>
    /// <typeparam name="TDataContext">The type of the data context to add.</typeparam>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionsAction">An optional action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <param name="contextLifetime">The lifetime with which to register 
    /// the context service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register 
    /// the options service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddXDataContext<TDataContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDataContext : DataContext =>
        services.AddDbContext<TDataContext>(
            optionsAction, contextLifetime, optionsLifetime);

    /// <summary>
    /// Adds the UnitOfWork to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the UnitOfWork to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddXUnitOfWork(
        this IServiceCollection services)
        => services.AddXUnitOfWork<UnitOfWork>();

    /// <summary>
    /// Adds the UnitOfWork for DataContextEvent to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the UnitOfWork to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddXEventUnitOfWork(
        this IServiceCollection services)
        => services
            .AddXUnitOfWork<UnitOfWork<DataContextEvent>>();

    /// <summary>
    /// Adds the keyed UnitOfWork for DataContextEvent to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the UnitOfWork to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <remarks>The key is "Aggregate".</remarks>
    public static IServiceCollection AddXEventUnitOfWorkKeyed(
        this IServiceCollection services)
        => services
            .AddXUnitOfWorkKeyed<UnitOfWork<DataContextEvent>>("Aggregate");

    /// <summary>
    /// Adds the EventStore to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the EventStore to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddXEventStore(
        this IServiceCollection services) =>
        services.AddXEventStore<EventStore>();

}
