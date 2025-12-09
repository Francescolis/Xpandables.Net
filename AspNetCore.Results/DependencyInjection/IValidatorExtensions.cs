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

using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering minimal result endpoint validators and related services in an <see
/// cref="IServiceCollection"/>.
/// </summary>
/// <remarks>Use the methods in this class to enable and customize endpoint validation for minimal result
/// endpoints in ASP.NET Core applications. These extensions facilitate the registration of default or custom validators
/// and supporting services during application startup.</remarks>
public static class IValidatorExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the default endpoint validator for minimal result endpoints to the service collection.
        /// </summary>
        /// <remarks>Call this method during application startup to enable validation of minimal result
        /// endpoints. This method registers the <see cref="ResultEndpointValidator"/> as the implementation for
        /// endpoint validation.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the minimal result endpoint validator registered.</returns>
        public IServiceCollection AddXResultEndpointValidator()
            => services
                .AddXResultEndpointValidator<ResultEndpointValidator>()
                .AddXValidatorProvider()
                .AddXValidatorFactory()
                .AddXResultHeaderWriter();

        /// <summary>
        /// Registers a scoped implementation of <see cref="IResultEndpointValidator"/> using the specified
        /// validator type.
        /// </summary>
        /// <remarks>Use this method to add a custom minimal result endpoint validator to the dependency
        /// injection container. The validator will be resolved with scoped lifetime for each request.</remarks>
        /// <typeparam name="TResultEndpointValidator">The type of the minimal result endpoint validator to register. Must be a class that implements <see
        /// cref="IResultEndpointValidator"/> and have a public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the validator registration added. This enables chaining
        /// additional service registrations.</returns>
        public IServiceCollection AddXResultEndpointValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResultEndpointValidator>()
            where TResultEndpointValidator : class, IResultEndpointValidator
            => services.AddScoped<IResultEndpointValidator, TResultEndpointValidator>();
    }
}
