
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

using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register repositories to the services collection.
/// </summary>
public static class ServiceCollectionRepositoryExtensions
{
    internal readonly static MethodInfo AddRepositoryMethod = typeof(ServiceCollectionRepositoryExtensions).GetMethod(nameof(AddXRepository))!;
    internal readonly static MethodInfo AddRepositoryForMethod = typeof(ServiceCollectionRepositoryExtensions).GetMethod(nameof(AddXRepositoryFor))!;

    /// <summary>
    /// Registers the <typeparamref name="TRepository"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationFactory">The factory that creates the repository.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXRepository<TEntity, TRepository>(
        this IServiceCollection services,
        Func<IServiceProvider, TRepository>? implementationFactory = default)
        where TRepository : class, IRepository<TEntity>
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.DoRegisterTypeServiceLifeTime<IRepository<TEntity>, TRepository>(
            implementationFactory);
    }

    /// <summary>
    /// Tries to add the <see cref="IRepository{TEntity}"/> for the specified type
    /// to the services with the scope life time.
    /// </summary>
    /// <typeparam name="TEntity">The type of the target entity.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXRepositoryFor<TEntity>(
        this IServiceCollection services)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .Add(new ServiceDescriptor(
                    typeof(IRepository<TEntity>),
                    service => service.GetRequiredService<IRepository<TEntity>>(),
                    ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers all <see cref="IRepository{TEntity}"/> implementations found 
    /// in the assemblies to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXRepositories(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IRepository<>),
            AddRepositoryMethod,
            assemblies);
    }

    /// <summary>
    /// Tries to register the <see cref="IRepository{TEntity}"/>
    /// for all entities found that implements <see cref="IEntity"/> to the services with the scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXRepositoryFors(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        var entityTypes = assemblies.SelectMany(ass => ass.GetExportedTypes())
            .Where(type => !type.IsAbstract
                           && !type.IsInterface
                           && !type.IsGenericType
                           && type.GetInterfaces().Exists(inter => !inter.IsGenericType && inter == typeof(IEntity)))
            .Select(type => type);

        foreach (var entityType in entityTypes)
        {
            MethodInfo repositoryRegister = AddRepositoryForMethod.MakeGenericMethod(entityType);

            repositoryRegister.Invoke(null, [services]);
        }

        return services;
    }
}
