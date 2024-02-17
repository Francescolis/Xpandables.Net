
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Operations;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides method to register services.
/// </summary>
public static class ServiceCollectionValidatorExtensions
{
    /// <summary>
    /// Registers the operation result request validator response builder.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <typeparam name="TOperationResultRequestValidator">The type of the operation result request validator.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXOperationResultRequestValidator<TOperationResultRequestValidator>(
        this IServiceCollection services)
        where TOperationResultRequestValidator : class, IOperationResultRequestValidator
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<IOperationResultRequestValidator, TOperationResultRequestValidator>();
    }

    /// <summary>
    /// Registers the default operation result request validator response builder.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXOperationResultRequestValidator(
        this IServiceCollection services)
        => services
            .AddXOperationResultRequestValidator<OperationResultRequestValidator>();

    /// <summary>
    /// Applies the validation filter factory to the request of the target route(s).
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> to add the filter to.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/> instance.</returns>
    public static TBuilder WithXValidatorFilterFactory<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        _ = builder.AddEndpointFilterFactory(OperationResultValidatorFilterFactory.MinimalFilterFactory);

        return builder;
    }

    /// <summary>
    /// Applies the validation filter factory to the request of the target route(s) according to the predicate.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> to add the filter to.</param>
    /// <param name="predicate">The predicate that a request must match in order to be validated.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/> instance.</returns>
    public static TBuilder WithXValidatorFilterFactory<TBuilder>(this TBuilder builder, Predicate<Type> predicate)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(predicate);

        OperationResultValidatorFilter.ValidatorPredicate = predicate;

        return builder.WithXValidatorFilterFactory();
    }

    /// <summary>
    /// Applies the validation filter to the request of the target route(s).
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> to add the filter to.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/> instance.</returns>
    /// <remarks>To be applied on many routes, please use <see langword="MapGroup"/> with empty prefix (<see cref="string.Empty"/>).</remarks>
    public static TBuilder WithXValidatorFilter<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        _ = builder.AddEndpointFilter(new OperationResultValidatorFilter().InvokeAsync);

        return builder;
    }

    /// <summary>
    /// Applies the validation process to the request of the target route(s) according to the predicate.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> to add the filter to.</param>
    /// <param name="predicate">The predicate that a request must match in order to be validated.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/> instance.</returns>
    /// <remarks>To be applied on many routes, please use <see langword="MapGroup"/> with empty prefix (<see cref="string.Empty"/>).</remarks>
    public static TBuilder WithXValidatorFilter<TBuilder>(this TBuilder builder, Predicate<Type> predicate)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(predicate);

        OperationResultValidatorFilter.ValidatorPredicate = predicate;

        return builder.WithXValidatorFilter();
    }
}
