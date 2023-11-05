
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Text;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides with methods to register services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the specified <typeparamref name="TTokenProcessor"/> type as <see cref="ITokenProcessor"/>
    /// to the services collection with scoped life time..
    /// </summary>
    /// <typeparam name="TTokenProcessor">The token processor type.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTokenProcessor<TTokenProcessor>(this IServiceCollection services)
        where TTokenProcessor : class, ITokenProcessor
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddXTokenProcessor(typeof(TTokenProcessor));
        return services;
    }

    /// <summary>
    /// Adds the specified <paramref name="tokenProcessor"/> type as <see cref="ITokenProcessor"/> 
    /// to the services collection with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="tokenProcessor">The type that implements <see cref="ITokenProcessor"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="tokenProcessor"/> is null.</exception>
    public static IServiceCollection AddXTokenProcessor(this IServiceCollection services, Type tokenProcessor)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(tokenProcessor);

        services.TryAddScoped(typeof(ITokenProcessor), tokenProcessor);
        return services;
    }

    /// <summary>
    /// Adds the specified <typeparamref name="TTokenRefreshProcessor"/> type as <see cref="ITokenRefreshProcessor"/>
    /// to the services collection with scoped life time..
    /// </summary>
    /// <typeparam name="TTokenRefreshProcessor">The token processor type.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXTokenRefreshProcessor<TTokenRefreshProcessor>(this IServiceCollection services)
        where TTokenRefreshProcessor : class, ITokenRefreshProcessor
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddXTokenRefreshProcessor(typeof(TTokenRefreshProcessor));
        return services;
    }

    /// <summary>
    /// Adds the specified <paramref name="tokenRefreshProcessor"/> type as <see cref="ITokenRefreshProcessor"/> 
    /// to the services collection with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="tokenRefreshProcessor">The type that implements <see cref="ITokenProcessor"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="tokenRefreshProcessor"/> is null.</exception>
    public static IServiceCollection AddXTokenRefreshProcessor(this IServiceCollection services, Type tokenRefreshProcessor)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(tokenRefreshProcessor);

        services.TryAddScoped(typeof(ITokenRefreshProcessor), tokenRefreshProcessor);
        return services;
    }
}
