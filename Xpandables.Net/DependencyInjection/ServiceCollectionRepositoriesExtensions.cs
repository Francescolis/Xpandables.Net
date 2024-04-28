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
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides a set of static methods for <see cref="IServiceCollection"/> to
/// support repositories services.
/// </summary>
public static class ServiceCollectionRepositoriesExtensions
{
    /// <summary>
    /// Registers the <see cref="IUnitOfWork"/> using the 
    /// <typeparamref name="TUnitOfWork"/> as <see cref="IUnitOfWork"/> to 
    /// the services with scope life time using the key.
    /// </summary>
    /// <typeparam name="TUnitOfWork">The type of the unit of work.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="serviceKey">The key to use for the unit of work.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXUnitOfWorkKeyed<TUnitOfWork>(
        this IServiceCollection services,
        string serviceKey)
        where TUnitOfWork : class, IUnitOfWork
        => services
            .AddKeyedScoped<IUnitOfWork, TUnitOfWork>(serviceKey);
}
