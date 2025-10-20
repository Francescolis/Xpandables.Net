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

using Xpandables.Net.Tasks.Pipelines;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
/// instance.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of <see
/// cref="IServiceCollection"/> to simplify service registration and setup in dependency injection scenarios. All
/// methods are intended to be used as extension methods and should be called on an existing <see
/// cref="IServiceCollection"/> object.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IPipelineExtensions
{
    /// <summary>
    /// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The service collection to extend. Cannot be null.</param>"
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the PipelineUnitOfWorkDecorator to the pipeline, enabling unit of work behavior for pipeline handlers
        /// registered in the service collection.
        /// </summary>
        /// <remarks>Use this method to ensure that each pipeline handler executes within a unit of work
        /// scope, typically for transactional consistency. This method should be called during service registration,
        /// before building the service provider.</remarks>
        /// <returns>The current IServiceCollection instance with the PipelineUnitOfWorkDecorator registered. This enables
        /// further configuration via method chaining.</returns>
        public IServiceCollection AddXPipelineUnitOfWorkDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineUnitOfWorkDecorator<>));

        /// <summary>
        /// Adds a pipeline validation decorator to the service collection for use with Pipeline requests.
        /// </summary>
        /// <remarks>This method registers the <c>PipelineValidationDecorator&lt;TRequest&gt;</c> in the
        /// service collection, enabling automatic validation of requests processed through the XPipeline. Call this
        /// method during application startup to ensure validation is applied to all pipeline requests.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the pipeline validation decorator registered.</returns>
        public IServiceCollection AddXPipelineValidationDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineValidationDecorator<>));
    }
}
