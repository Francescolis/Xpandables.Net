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
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding repository services to 
/// the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionRepositoryExtensions
{
    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TRepository"/> to the <see
    /// cref="IServiceCollection"/> with an <see cref="IRepository"/> service type.
    /// </summary>
    /// <remarks>This method registers the repository as a scoped service, meaning a new instance will be
    /// created for each request within the scope.</remarks>
    /// <typeparam name="TRepository">The type of the repository to add. 
    /// This type must implement <see cref="IRepository"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository will be added.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddXRepository<TRepository>(
        this IServiceCollection services)
        where TRepository : class, IRepository =>
        services.AddScoped<IRepository, TRepository>();

    /// <summary>
    /// Adds a keyed scoped service of type <typeparamref name="TRepository"/> to the specified <see
    /// cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the repository as a scoped service, allowing it to be resolved with the
    /// specified key.</remarks>
    /// <typeparam name="TRepository">The type of the repository to add. 
    /// Must implement <see cref="IRepository"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <param name="key">The key associated with the repository service.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXRepositoryKeyed<TRepository>(
        this IServiceCollection services, string key)
        where TRepository : class, IRepository =>
        services.AddKeyedScoped<IRepository, TRepository>(key);

    /// <summary>
    /// Registers a repository service with a scoped lifetime in the dependency injection container.
    /// </summary>
    /// <remarks>This method registers the specified repository implementation with a scoped lifetime, meaning
    /// a new instance is created for each request within the same scope. Ensure that <typeparamref name="TRepository"/>
    /// implements <typeparamref name="TInterface"/> to avoid runtime errors.</remarks>
    /// <typeparam name="TInterface">The interface type of the repository to register.</typeparam>
    /// <typeparam name="TRepository">The concrete implementation type of the repository.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the repository service registered.</returns>
    public static IServiceCollection AddXRepository<TInterface, TRepository>(
        this IServiceCollection services)
        where TInterface : class, IRepository
        where TRepository : class, TInterface =>
        services.AddScoped<TInterface, TRepository>();

    /// <summary>
    /// Adds a keyed scoped <see cref="IUnitOfWork"/> service to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TUnitOfWork">The type of the unit of work.</typeparam>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXUnitOfWorkKeyed<TUnitOfWork>(
        this IServiceCollection services, string key)
        where TUnitOfWork : class, IUnitOfWork =>
        services.AddKeyedScoped<IUnitOfWork, TUnitOfWork>(key);

    /// <summary>
    /// Adds a scoped <see cref="IUnitOfWork"/> service to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TUnitOfWork">The type of the unit of work.</typeparam>
    /// <param name="services">The service collection to add the service to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXUnitOfWork<TUnitOfWork>(
        this IServiceCollection services)
        where TUnitOfWork : class, IUnitOfWork =>
        services.AddScoped<IUnitOfWork, TUnitOfWork>();

    /// <summary>
    /// Adds a scoped <see cref="IUnitOfWorkEvent"/> service to the 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TUnitOfWorkEvent">The type of the unit of work event.</typeparam>
    /// <param name="services">The service collection to add the service to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddXUnitOfWorkEvent<TUnitOfWorkEvent>(
        this IServiceCollection services)
        where TUnitOfWorkEvent : class, IUnitOfWorkEvent =>
        services.AddScoped<IUnitOfWorkEvent, TUnitOfWorkEvent>();
}
