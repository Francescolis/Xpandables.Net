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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Xpandables.Net.Http.Minimals;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring minimal API endpoint validation, result filtering, and execution result
/// header writing in ASP.NET Core applications.
/// </summary>
/// <remarks>Use the methods in this class to add middleware and service registrations that enable minimal API
/// features, such as automatic validation, consistent result filtering, and inclusion of execution result information
/// in HTTP response headers. These extensions are intended to be called during application startup when setting up the
/// application's request pipeline and dependency injection container.</remarks>
public static class MinimalResultExtensions
{
    extension(IApplicationBuilder builder)
    {
        /// <summary>
        /// Adds minimal API endpoint validation and result filters to the application's request pipeline.
        /// </summary>
        /// <remarks>This extension method configures the middleware pipeline to apply validation and
        /// result filters specifically to minimal API endpoints. It should be called during application startup before
        /// mapping minimal API routes. The method does not affect non-minimal API endpoints.</remarks>
        /// <returns>The <see cref="IApplicationBuilder"/> instance with minimal API support filters applied.</returns>
        public IApplicationBuilder UseXMinimalSupport()
        {
            ArgumentNullException.ThrowIfNull(builder);

            var validationFilter = new MinimalResultEndpointValidationFilter();
            var resultFilter = new MinimalResultEndpointFilter();

            return builder.Use(async (context, next) =>
            {
                Endpoint? endpoint = context.GetEndpoint();

                // Only apply filters to route endpoints (minimal APIs)
                if (endpoint is null)
                {
                    await next().ConfigureAwait(false);
                    return;
                }

                // Build the filter pipeline
                // The pipeline order is: validation -> result -> actual endpoint
                EndpointFilterDelegate pipeline = async (filterContext) =>
                {
                    await next().ConfigureAwait(false);
                    return null;
                };

                // Wrap with result filter
                EndpointFilterDelegate resultPipeline = pipeline;
                pipeline = (filterContext) => resultFilter.InvokeAsync(filterContext, resultPipeline);

                // Wrap with validation filter  
                EndpointFilterDelegate validationPipeline = pipeline;
                pipeline = (filterContext) => validationFilter.InvokeAsync(filterContext, validationPipeline);

                // Create filter invocation context
                var filterInvocationContext = new MinimalEndpointFilterInvocationContext(context);

                // Execute the filter pipeline
                await pipeline(filterInvocationContext).ConfigureAwait(false);
            });
        }
    }

    /// <summary>
    /// </summary>
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        /// Configures the builder to use the minimal API features, including filtering and validation.
        /// </summary>
        /// <returns>The builder instance configured with minimal API support. This enables both minimal filtering and
        /// validation.</returns>
        public TBuilder WithXMinimalApi() =>
            builder
                .WithXMinimalFilter()
                .WithXMinimalValidation();

        /// <summary>
        /// Adds minimal validation for endpoint results to the current builder.
        /// </summary>
        /// <remarks>This method configures the builder to use a minimal validation filter for endpoint
        /// results. Use this when only basic validation is required for endpoint responses. The returned builder can be
        /// further configured or used to build the endpoint.</remarks>
        /// <returns>The builder instance with minimal result validation applied.</returns>
        public TBuilder WithXMinimalValidation()
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.AddEndpointFilter(new MinimalResultEndpointValidationFilter().InvokeAsync);
        }

        /// <summary>
        /// Adds a minimal result endpoint filter to the current builder configuration.
        /// </summary>
        /// <remarks>Use this method to ensure that endpoints only return minimal results, which can help
        /// enforce consistent API responses. This method is typically used when configuring endpoints that should not
        /// include additional metadata or formatting.</remarks>
        /// <returns>The builder instance with the minimal result endpoint filter applied.</returns>
        public TBuilder WithXMinimalFilter() =>
            builder.AddEndpointFilter<TBuilder, MinimalResultEndpointFilter>();
    }

    /// <summary>
    /// Provides a minimal implementation of <see cref="EndpointFilterInvocationContext"/> for global filter execution.
    /// </summary>
    private sealed class MinimalEndpointFilterInvocationContext(HttpContext httpContext) : EndpointFilterInvocationContext
    {
        /// <inheritdoc/>
        public override HttpContext HttpContext { get; } = httpContext;

        /// <inheritdoc/>
        public override IList<object?> Arguments { get; } = [];

        /// <inheritdoc/>
        public override T GetArgument<T>(int index) =>
            Arguments.Count > index ? (T)Arguments[index]! : default!;
    }
}
