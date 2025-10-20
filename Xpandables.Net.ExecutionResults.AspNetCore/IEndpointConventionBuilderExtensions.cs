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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Xpandables.Net.ExecutionResults;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring endpoint convention builders with additional filters and behaviors.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of types implementing <see
/// cref="IEndpointConventionBuilder"/>. Use these extensions to customize endpoint pipelines, such as adding filters to
/// control response data or execution behavior.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IEndpointConventionBuilderExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        /// Adds a minimal execution result filter to the endpoint pipeline.
        /// </summary>
        /// <remarks>Use this method to ensure that only essential execution result data is included in
        /// endpoint responses. This can help reduce payload size and improve performance for scenarios where detailed
        /// result information is not required.</remarks>
        /// <returns>The current builder instance with the minimal filter applied.</returns>
        public TBuilder WithXMinimalFilter() =>
            builder.AddEndpointFilter<TBuilder, ExecutionResultMinimalFilter>();
    }
}