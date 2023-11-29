
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <typeparamref name="TDataContext"/> type class reference 
    /// implementation derives from <see cref="DataContext"/> to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TDataContext">The type of the data context that derives from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="optionsAction">An optional action to configure the Microsoft.EntityFrameworkCore.DbContextOptions for the context.</param>
    /// <param name="contextLifetime">The lifetime with which to register the context service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDataContext<TDataContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDataContext : DataContext
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        services.AddDbContext<TDataContext>(optionsAction, contextLifetime, optionsLifetime);
        return services;
    }

    /// <summary>
    /// Registers the <see cref="UnitOfWork{TDataContext}"/> using the <typeparamref name="TDataContext"/> 
    /// as <see cref="IUnitOfWork"/> to the services with scope life time.
    /// </summary>
    /// <typeparam name="TDataContext">The type of the context for unit of work.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXUnitOfWork<TDataContext>(this IServiceCollection services)
        where TDataContext : DataContext
        => services.AddScoped<IUnitOfWork, UnitOfWork<TDataContext>>();

    /// <summary>
    /// Registers the default generic <see cref="IUnitOfWork{TDataContext}"/> implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXUnitOfWorks(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        return services;
    }
}
