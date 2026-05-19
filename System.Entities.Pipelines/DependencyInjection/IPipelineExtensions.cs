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
	/// Registers the PipelineEntityUnitOfWorkDecorator for all pipeline handlers in the service collection, enabling
	/// unit-of-work behavior within the pipeline execution.
	/// </summary>
	/// <remarks>Use this method to ensure that each pipeline handler is executed within a
	/// unit-of-work scope, which can help manage transactional consistency and resource cleanup. This method should
	/// be called during application startup as part of dependency injection configuration.</remarks>
	/// <returns>The IServiceCollection instance with the PipelineEntityUnitOfWorkDecorator registered. This enables further
	/// chaining of service registrations.</returns>
	public static IServiceCollection AddXPipelineEntityUnitOfWorkDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelineEntityUnitOfWorkDecorator<>));
}
