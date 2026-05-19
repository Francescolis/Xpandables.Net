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
using System.Diagnostics.CodeAnalysis;
using System.Results.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
/// instance.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of <see
/// cref="IServiceCollection"/> to simplify service registration and setup in dependency injection scenarios. All
/// methods are intended to be used as extension methods and should be called on an existing <see
/// cref="IServiceCollection"/> object.</remarks>
public static class ITaskExtensions
{
	/// <summary>
	/// Adds Mediator and related pipeline request handler services to the current service collection.
	/// </summary>
	/// <remarks>
	/// if you want to add pipeline decorators, register handler services in this order :
	/// <list type="bullet">
	/// <item>PipelinePreHanderDecorator</item>
	/// <item>PipelinePostHandlerDecorator</item>
	/// <item>PipelineUnitOfWorkDecorator</item>
	/// <item>PipelineValidationDecorator</item>
	/// <item>PipelineExceptionDecorator</item>
	/// <item>PipelineRequestHandler</item>
	/// </list>
	/// <para>In order to register the mediator to be used with Event sourcing, add registrations as follow:</para>
	/// <list type="bullet">
	/// <item>PipelineBeforeCommitDomainEventDecorator</item>
	/// <item>PipelineEnqueueIntegrationEventDecorator</item>
	/// <item>PipelinePreHandlerDecorator</item>
	/// <item>PipelinePostHandlerDecorator</item>
	/// <item>PipelineRequireUnitOfWorkDecorator</item>
	/// <item>PipelineAfterCommitDomainEventDecorator</item>
	/// <item>PipelineValidationDecorator</item>
	/// <item>PipelineExceptionDecorator</item>
	/// <item>PipelineRequestHandler</item>
	/// </list>
	/// In order to register custom pipeline decorators, use the <see langword="AddXPipelineDecorator(IServiceCollection, Type)"/> method.</remarks>
	/// <returns>The <see cref="IServiceCollection"/> instance with Mediator services registered. This enables further
	/// configuration of dependency injection.</returns>
	public static IServiceCollection AddXMediator(this IServiceCollection services) => services.AddXMediator<Mediator>();

	/// <summary>
	/// Registers the specified mediator implementation as a transient service for dependency injection.
	/// </summary>
	/// <remarks>
	/// if you want to add pipeline decorators, register handler services in this order :
	/// <list type="bullet">
	/// <item>PipelinePreHanderDecorator</item>
	/// <item>PipelinePostHandlerDecorator</item>
	/// <item>PipelineUnitOfWorkDecorator</item>
	/// <item>PipelineValidationDecorator</item>
	/// <item>PipelineExceptionDecorator</item>
	/// <item>PipelineRequestHandler</item>
	/// </list>
	/// <para>In order to register the mediator to be used with Event sourcing, add registrations as follow:</para>
	/// <list type="bullet">
	/// <item>PipelineBeforeCommitDomainEventDecorator</item>
	/// <item>PipelineEnqueueIntegrationEventDecorator</item>
	/// <item>PipelinePreHandlerDecorator</item>
	/// <item>PipelinePostHandlerDecorator</item>
	/// <item>PipelineRequireUnitOfWorkDecorator</item>
	/// <item>PipelineAfterCommitDomainEventDecorator</item>
	/// <item>PipelineValidationDecorator</item>
	/// <item>PipelineExceptionDecorator</item>
	/// <item>PipelineRequestHandler</item>
	/// </list>
	/// In order to register custom pipeline decorators, use the <see langword="AddXPipelineDecorator(IServiceCollection, Type)"/> method.</remarks>
	/// <returns>The <see cref="IServiceCollection"/> instance with Mediator services registered. This enables further
	/// configuration of dependency injection.</returns>
	public static IServiceCollection AddXMediator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMediator>(this IServiceCollection services)
		where TMediator : class, IMediator =>
		services.AddScoped<IMediator, TMediator>()
		.AddXPipelineRequestHandler();
}
