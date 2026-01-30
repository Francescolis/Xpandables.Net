/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides traditional extension methods for Entity Framework service registrations.
/// </summary>
public static class EntityFrameworkServiceCollectionExtensions
{
    /// <summary>
    /// Adds the specified data context type to the service collection with configurable options and lifetimes.
    /// </summary>
    public static IServiceCollection AddXDataContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TDataContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDataContext : System.Entities.EntityFramework.DataContext
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddDbContext<TDataContext>(optionsAction, contextLifetime, optionsLifetime);
    }

    /// <summary>
    /// Adds a factory for creating instances of the specified data context type to the service collection.
    /// </summary>
    public static IServiceCollection AddXDataContextFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TDataContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
        ServiceLifetime factoryLifetime = ServiceLifetime.Singleton)
        where TDataContext : System.Entities.EntityFramework.DataContext
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddDbContextFactory<TDataContext>(optionsAction, factoryLifetime);
    }
}
