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
	/// Registers the PipelineDataUnitOfWorkDecorator for all pipeline handlers, enabling ADO.NET
	/// transaction management within the pipeline execution.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this method to ensure that each pipeline handler is executed within an ADO.NET transaction scope.
	/// The transaction will be committed on success or rolled back on failure.
	/// </para>
	/// </remarks>
	/// <returns>The IServiceCollection instance with the PipelineDataUnitOfWorkDecorator registered.</returns>
	public static IServiceCollection AddXPipelineRequireDataUnitOfWorkDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelineDataUnitOfWorkDecorator<>));
}
