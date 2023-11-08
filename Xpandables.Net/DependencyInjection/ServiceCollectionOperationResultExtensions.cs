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

using Xpandables.Net.Decorators;
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Messaging;

namespace Xpandables.Net.DependencyInjection;
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the implementation of <see cref="IOperationResultContextFinalizer"/> to 
    /// the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IOperationResultContextFinalizer, OperationResultContextFinalizerInternal>();
        return services;
    }

    /// <summary>
    /// Adds operation result correlation behavior to commands and queries 
    /// that are decorated with the <see cref="IOperationResultContextDecorator"/> to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXOperationResultContextDecorator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.XTryDecorate(typeof(ICommandHandler<>), typeof(OperationResultContextCommandDecorator<>));
        services.XTryDecorate(typeof(IAsyncQueryHandler<,>), typeof(OperationResultContextAsyncQueryDecorator<,>));
        services.XTryDecorate(typeof(IQueryHandler<,>), typeof(OperationResultContextQueryDecorator<,>));

        return services;
    }
}
