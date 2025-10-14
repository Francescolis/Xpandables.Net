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
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Validators;

/// <summary>
/// Provides extension methods for configuring execution result validation on endpoint convention builders and
/// registering related services in the dependency injection container.
/// </summary>
/// <remarks>Use the methods in this class to enable execution result validation for endpoints and to register the
/// necessary validation services during application startup. These extensions help ensure that endpoint responses are
/// validated consistently across the application.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IEndpointConventionBuilderExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        /// Adds an endpoint filter factory that enables execution result validation for endpoints built by this
        /// builder.
        /// </summary>
        /// <remarks>Use this method to ensure that endpoints created with this builder will have
        /// execution result validation applied. This can help enforce consistent validation logic across multiple
        /// endpoints.</remarks>
        /// <returns>The current builder instance with execution result validation enabled.</returns>
        [RequiresUnreferencedCode("This method may be trimmed.")]
        [RequiresDynamicCode("This method may be trimmed.")]
        public TBuilder WithXExecutionResultValidationFactory()
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddEndpointFilterFactory(ExecutionResultEndpointValidationFilterFactory.FilterFactory);
            return builder;
        }

        /// <summary>
        /// Adds execution result validation to the endpoint builder pipeline.
        /// </summary>
        /// <remarks>This method configures the builder to validate the results of endpoint executions
        /// using a predefined validation filter. Use this method to ensure that endpoint responses meet expected
        /// validation criteria before being returned to the client.</remarks>
        /// <returns>The current builder instance with execution result validation enabled.</returns>
        public TBuilder WithXExecutionResultValidation()
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddEndpointFilter(new ExecutionResultEndpointValidationFilter().InvokeAsync);
            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the necessary services for execution result validation to the service collection.
        /// </summary>
        /// <remarks>This method registers the required services and dependencies needed for
        /// execution result validation in the application's dependency injection container. Use this method
        /// during application startup to ensure that all necessary components are available for validation
        /// operations.</remarks>
        /// <returns>The updated service collection with execution result validation services added.</returns>
        public IServiceCollection AddXExecutionResultEndpointValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExecutionResultEndpointValidator>()
            where TExecutionResultEndpointValidator : class, IExecutionResultEndpointValidator
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddScoped<IExecutionResultEndpointValidator, TExecutionResultEndpointValidator>();
        }

        /// <summary>
        /// Adds the default implementation of the X execution result endpoint validator to the service collection.
        /// </summary>
        /// <remarks>Call this method to register the standard execution result endpoint validator for use
        /// in dependency injection. This method is typically used during application startup when configuring
        /// services.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the endpoint validator service registered. This enables
        /// further configuration of services.</returns>
        public IServiceCollection AddXExecutionResultEndpointValidator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXExecutionResultEndpointValidator<ExecutionResultEndpointValidator>();
        }
    }
}
