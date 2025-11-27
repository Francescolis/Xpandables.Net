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

using AspNetCore.Net;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering Execution-Result header writers in an ASP.NET Core application's
/// service collection.
/// </summary>
/// <remarks>Use this class to enable automatic inclusion of execution result information in HTTP response headers
/// by configuring dependency injection for default or custom header writers. These methods should be called during
/// application startup to ensure the appropriate services are available for HTTP response processing.</remarks>
public static class MinimalResultHeaderExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the default endpoint validator for minimal result endpoints to the service collection.
        /// </summary>
        /// <remarks>Call this method during application startup to enable validation of minimal result
        /// endpoints. This method registers the <see cref="MinimalResultEndpointValidator"/> as the implementation for
        /// endpoint validation.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the minimal result endpoint validator registered.</returns>
        public IServiceCollection AddXMinialResultEndpointValidator()
            => services
                .AddXMinimalResultEndpointValidator<MinimalResultEndpointValidator>()
                .AddXValidatorProvider()
                .AddXValidatorFactory()
                .AddXExecutionResultHeaderWriter();

        /// <summary>
        /// Registers a scoped implementation of <see cref="IMinimalResultEndpointValidator"/> using the specified
        /// validator type.
        /// </summary>
        /// <remarks>Use this method to add a custom minimal result endpoint validator to the dependency
        /// injection container. The validator will be resolved with scoped lifetime for each request.</remarks>
        /// <typeparam name="TMinimalResultEndpointValidator">The type of the minimal result endpoint validator to register. Must be a class that implements <see
        /// cref="IMinimalResultEndpointValidator"/> and have a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the validator registration added. This enables chaining
        /// additional service registrations.</returns>
        public IServiceCollection AddXMinimalResultEndpointValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMinimalResultEndpointValidator>()
            where TMinimalResultEndpointValidator : class, IMinimalResultEndpointValidator
            => services.AddScoped<IMinimalResultEndpointValidator, TMinimalResultEndpointValidator>();

        /// <summary>
        /// Adds the default X-Execution-Result header writer to the service collection for use in HTTP responses.
        /// </summary>
        /// <remarks>This method registers <see cref="OperationResultHeaderWriter"/> as the implementation
        /// for writing X-Execution-Result headers. Call this method during application startup to enable automatic
        /// inclusion of execution result information in HTTP response headers.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the X-Execution-Result header writer registered.</returns>
        public IServiceCollection AddXExecutionResultHeaderWriter()
            => services.AddXExecutionResultHeaderWriter<OperationResultHeaderWriter>();

        /// <summary>
        /// Registers a scoped implementation of <see cref="IOperationResultHeaderWriter"/> using the specified type in
        /// the service collection.
        /// </summary>
        /// <remarks>Use this method to configure dependency injection for custom execution result header
        /// writers. Each request will receive a new instance of <typeparamref
        /// name="TExecutionResultHeaderWriter"/>.</remarks>
        /// <typeparam name="TExecutionResultHeaderWriter">The type that implements <see cref="IOperationResultHeaderWriter"/> and will be registered as a scoped
        /// service. Must have a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the registration applied.</returns>
        public IServiceCollection AddXExecutionResultHeaderWriter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExecutionResultHeaderWriter>()
            where TExecutionResultHeaderWriter : class, IOperationResultHeaderWriter
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IOperationResultHeaderWriter, TExecutionResultHeaderWriter>();
            return services;
        }
    }

}
