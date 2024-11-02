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

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding services to the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionDependencyInjectionExtensions
{
    /// <summary>
    /// Adds a transient service of the type <see cref="Lazy{T}"/> with an 
    /// implementation type of <see cref="LazyResolved{T}"/> to the specified 
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the 
    /// service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls 
    /// can be chained.</returns>
    public static IServiceCollection AddXLazyResolved(
        this IServiceCollection services) =>
        services.AddTransient(typeof(Lazy<>), typeof(LazyResolved<>));
}