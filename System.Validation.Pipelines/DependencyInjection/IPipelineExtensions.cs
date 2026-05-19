/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.Pipelines;

namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering pipeline decorators with an <see cref="IServiceCollection"/>.
/// </summary>
public static class IPipelineExtensions
{
	/// <summary>
	/// Adds a pipeline validation decorator to the service collection for use with Pipeline requests.
	/// </summary>
	/// <remarks>This method registers the <c>PipelineValidationDecorator&lt;TRequest&gt;</c> in the
	/// service collection, enabling automatic validation of requests processed through the XPipeline. Call this
	/// method during application startup to ensure validation is applied to all pipeline requests.</remarks>
	/// <returns>The updated <see cref="IServiceCollection"/> instance with the pipeline validation decorator registered.</returns>
	public static IServiceCollection AddXPipelineValidationDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelineValidationDecorator<>));

}
