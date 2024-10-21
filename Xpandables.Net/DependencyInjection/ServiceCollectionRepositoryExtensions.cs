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
}
