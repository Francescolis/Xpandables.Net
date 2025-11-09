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
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.ExecutionResults.Controllers;
using Xpandables.Net.ExecutionResults.DataAnnotations;
using Xpandables.Net.ExecutionResults.Minimals;
using Xpandables.Net.ExecutionResults.ResponseWriters;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering ExecutionResult response writers and related configuration with an
/// IServiceCollection.
/// </summary>
/// <remarks>These extension methods simplify the setup of ExecutionResult support in ASP.NET Core applications,
/// including registration of response writers, controller MVC options, and minimal JSON serialization options. Call
/// these methods during application startup to enable XExecutionResult features as needed.</remarks>
public static class IExecutionResultExtensions
{
    /// <summary>
    /// Adds the ExecutionResultMinimalMiddleware to the application's request pipeline.
    /// </summary>
    /// <param name="builder">The IApplicationBuilder instance to configure the middleware for. Cannot be null.</param>
    /// <returns>The original IApplicationBuilder instance with the ExecutionResultMinimalMiddleware added to the request
    /// pipeline.</returns>
    public static IApplicationBuilder UseXExecutionResultMinimalMiddleware(
        this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.UseMiddleware<ExecutionResultMinimalMiddleware>();
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the required services for Minimal API support to the current <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>Call this method during application startup to register all necessary components for
        /// Minimal API functionality. This method is intended to be used with dependency injection in ASP.NET Core
        /// applications.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Minimal API services registered. This enables further
        /// configuration via method chaining.</returns>
        public IServiceCollection AddXMinimalApi() =>
            services
                .AddXExecutionResultMinimalJsonOptions()
                .AddXValidatorProvider()
                .AddXExecutionResultEndpointValidator()
                .AddXExecutionResultMinimalMiddleware()
                .AddXValidator()
                .AddXExecutionResultResponseWriters();

        /// <summary>
        /// Adds a scoped implementation of IExecutionResultResponseWriter using the specified writer type to the
        /// service collection.
        /// </summary>
        /// <typeparam name="TWriter">The type that implements IExecutionResultResponseWriter to be registered. Must have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance for chaining additional service configuration.</returns>
        public IServiceCollection AddXExecutionResultResponseWriter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWriter>()
            where TWriter : class, IExecutionResultResponseWriter
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IExecutionResultResponseWriter, TWriter>();
            return services;
        }

        /// <summary>
        /// Registers the default set of execution result response writers with the service collection.
        /// </summary>
        /// <remarks>This method adds implementations of <see cref="IExecutionResultResponseWriter"/> to
        /// the service collection for handling various execution result scenarios. Call this method during application
        /// startup to enable response writing for execution results.</remarks>
        /// <returns>The current <see cref="IServiceCollection"/> instance with the response writers registered.</returns>
        public IServiceCollection AddXExecutionResultResponseWriters()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IExecutionResultResponseWriter, FileExecutionResultResponseWriter>();
            services.AddScoped<IExecutionResultResponseWriter, SuccessExecutionResultResponseWriter>();
            services.AddScoped<IExecutionResultResponseWriter, FailureExecutionResultResponseWriter>();
            services.AddScoped<IExecutionResultResponseWriter, StreamExecutionResultResponseWriter>();
            return services;
        }

        /// <summary>
        /// Adds the required MVC options for XExecutionResult controllers to the service collection.
        /// </summary>
        /// <remarks>Call this method to enable support for XExecutionResult controllers in your ASP.NET
        /// Core application. This method registers the necessary configuration for MVC options to support
        /// XExecutionResult handling.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the XExecutionResult controller MVC options configured.</returns>
        public IServiceCollection AddXExecutionResultControllerMvcOptions()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>, ExecutionResultControllerMvcOptions>();
            return services;
        }

        /// <summary>
        /// Adds configuration for minimal JSON serialization of XExecutionResult to the service collection.
        /// </summary>
        /// <remarks>Call this method to register JSON serialization options for XExecutionResult when
        /// using minimal APIs. This enables consistent formatting of execution results in JSON responses.</remarks>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
        public IServiceCollection AddXExecutionResultMinimalJsonOptions()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IConfigureOptions<JsonOptions>, ExecutionResultMinimalJsonOptions>();
            return services;
        }

        /// <summary>
        /// Adds the ExecutionResultMinimalMiddleware to the service collection for dependency injection.
        /// </summary>
        /// <returns>The IServiceCollection instance with the middleware registered. This enables chaining of further service
        /// configuration calls.</returns>
        public IServiceCollection AddXExecutionResultMinimalMiddleware()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddScoped<ExecutionResultMinimalMiddleware>();
        }

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

    extension<TBuilder>(TBuilder builder)
         where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        /// Configures the builder with the minimal API setup, including default filtering and execution result
        /// validation.
        /// </summary>
        /// <returns>The builder instance configured for minimal API usage.</returns>
        public TBuilder WithXMinimalApi() =>
            builder
                .WithXMinimalFilter()
                .WithXExecutionResultValidation();

        /// <summary>
        /// Adds a minimal execution result filter to the endpoint pipeline.
        /// </summary>
        /// <remarks>Use this method to ensure that only essential execution result data is included in
        /// endpoint responses. This can help reduce payload size and improve performance for scenarios where detailed
        /// result information is not required.</remarks>
        /// <returns>The current builder instance with the minimal filter applied.</returns>
        public TBuilder WithXMinimalFilter() =>
            builder.AddEndpointFilter<TBuilder, ExecutionResultMinimalFilter>();

        /// <summary>
        /// Adds an endpoint filter factory that enables execution result validation for endpoints built by this
        /// builder.
        /// </summary>
        /// <remarks>Use this method to ensure that endpoints created with this builder will have
        /// execution result validation applied. This can help enforce consistent validation logic across multiple
        /// endpoints.</remarks>
        /// <returns>The current builder instance with execution result validation enabled.</returns>
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
}