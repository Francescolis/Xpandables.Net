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

using Xpandables.Net.Validators;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering Minimal API support services with an <see cref="IServiceCollection"/> in
/// ASP.NET Core applications.
/// </summary>
/// <remarks>Use this static class to add all required services for Minimal API functionality during application
/// startup. The extension methods are designed to integrate with the dependency injection system and enable further
/// configuration through method chaining.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IMiminalExtensions
{
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
    }
}