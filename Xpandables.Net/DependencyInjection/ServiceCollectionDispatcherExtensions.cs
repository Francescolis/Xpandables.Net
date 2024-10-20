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

using Xpandables.Net.Responsibilities;

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
}
