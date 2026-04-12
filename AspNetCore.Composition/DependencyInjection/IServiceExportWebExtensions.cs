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
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.AspNetCore.Builder;


#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering services with an <see cref="IServiceCollection"/> using
/// configuration and assembly scanning.
/// </summary>
/// <remarks>These extension methods enable modular service registration by discovering and adding services that
/// implement specific export interfaces. Some methods perform assembly scanning and may be affected by trimming when
/// publishing applications; ensure that all required types are preserved if trimming is enabled.</remarks>
public static class IServiceExportWebExtensions
{
	/// <summary>
	/// Adds XService MEF exports to the application's request pipeline.
	/// </summary>
	/// <remarks>This method enables MEF-based composition for XService exports. It relies on reflection and runtime
	/// code generation, which may have implications for trimming and AOT scenarios.</remarks>
	/// <param name="application">The web application to configure. Cannot be null.</param>
	/// <returns>The configured <see cref="WebApplication"/> instance.</returns>
	[RequiresAssemblyFiles]
	[RequiresUnreferencedCode("UseXServiceExports uses MEF composition which relies on reflection.")]
	[RequiresDynamicCode("UseXServiceExports uses MEF composition which relies on runtime code generation.")]
	public static WebApplication UseXServiceExports(this WebApplication application)
	{
		ArgumentNullException.ThrowIfNull(application);
		return application.UseXServiceExports(_ => { });
	}

	/// <summary>
	/// Configures the application to use XService MEF exports asynchronously.
	/// </summary>
	/// <remarks>This method enables MEF-based composition for XService exports in the application. It requires
	/// support for reflection, dynamic code generation, and access to referenced assemblies at runtime. Use this method at
	/// application startup to ensure all MEF exports are available before handling requests.</remarks>
	/// <param name="application">The WebApplication instance to configure with XService MEF exports.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation. The default value is <see
	/// cref="CancellationToken.None"/>.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the configured WebApplication instance.</returns>
	[RequiresUnreferencedCode("UseXServiceExportsAsync uses MEF composition which relies on reflection.")]
	[RequiresAssemblyFiles()]
	[RequiresDynamicCode("UseXServiceExportsAsync uses MEF composition which relies on runtime code generation.")]
	public static Task<WebApplication> UseXServiceExportsAsync(
		this WebApplication application,
		CancellationToken cancellationToken = default) =>
		application.UseXServiceExportsAsync(_ => { }, cancellationToken);

	/// <summary>
	/// Configures and applies MEF-based service exports to the specified web application using the provided export
	/// options.
	/// </summary>
	/// <remarks>This method uses Managed Extensibility Framework (MEF) composition, which relies on reflection and
	/// runtime code generation. It is not compatible with trimming or ahead-of-time compilation scenarios. Use this method
	/// during application startup to register and compose exported services.</remarks>
	/// <param name="application">The web application to which the service exports will be applied. Cannot be null.</param>
	/// <param name="configureOptions">A delegate that configures the export options used for MEF composition. Cannot be null.</param>
	/// <returns>The same instance of <see cref="WebApplication"/> with the configured service exports applied.</returns>
	[RequiresAssemblyFiles()]
	[RequiresUnreferencedCode("UseXServiceExports uses MEF composition which relies on reflection.")]
	[RequiresDynamicCode("UseXServiceExports uses MEF composition which relies on runtime code generation.")]
	public static WebApplication UseXServiceExports(this WebApplication application, Action<ExportOptions> configureOptions)
	{
		ArgumentNullException.ThrowIfNull(application);
		ArgumentNullException.ThrowIfNull(configureOptions);

		ExportOptions options = new();
		configureOptions(options);

		IServiceExportExtensions.ApplyServiceExports<IUseServiceExport>(
			options, exports =>
			{
				foreach (IUseServiceExport export in exports)
				{
					export.UseServices(application);
				}
			});

		return application;
	}

	/// <summary>
	/// Configures and applies XService MEF exports to the specified web application asynchronously.
	/// </summary>
	/// <remarks>This method uses Managed Extensibility Framework (MEF) composition, which relies on reflection and
	/// runtime code generation. It is not compatible with trimming or ahead-of-time compilation scenarios. All registered
	/// service exports implementing IUseServiceExport will be invoked to configure the application.</remarks>
	/// <param name="application">The web application to which XService exports will be applied. Cannot be null.</param>
	/// <param name="configureOptions">A delegate to configure export options before applying service exports. Cannot be null.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation. Optional.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the configured web application
	/// instance.</returns>
	[RequiresUnreferencedCode("UseXServiceExportsAsync uses MEF composition which relies on reflection.")]
	[RequiresAssemblyFiles()]
	[RequiresDynamicCode("UseXServiceExportsAsync uses MEF composition which relies on runtime code generation.")]
	public static async Task<WebApplication> UseXServiceExportsAsync(
		this WebApplication application,
		Action<ExportOptions> configureOptions,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(application);
		ArgumentNullException.ThrowIfNull(configureOptions);
		ExportOptions options = new();
		configureOptions(options);

		await IServiceExportExtensions.ApplyServiceExportsAsync<IUseServiceExport>(
			options, async exports =>
			{
				foreach (IUseServiceExport export in exports)
				{
					await export.UseServicesAsync(application, cancellationToken).ConfigureAwait(false);
				}
			}, cancellationToken).ConfigureAwait(false);

		return application;
	}

	/// <summary>
	/// Uses the specified assemblies to apply services to the web application.
	/// </summary>
	/// <param name="application">The <see cref="WebApplication"/> to configure.</param>
	/// <param name="assemblies">The assemblies to scan for services.</param>
	/// <returns>The web application with applied services.</returns>
	[RequiresUnreferencedCode("UseXServices scans assemblies via reflection which is not compatible with trimming.")]
	[RequiresDynamicCode("UseXServices uses Activator.CreateInstance which requires runtime code generation.")]
	public static WebApplication UseXServices(this WebApplication application, params IEnumerable<Assembly> assemblies)
	{
		ArgumentNullException.ThrowIfNull(application);
		ArgumentNullException.ThrowIfNull(assemblies);

		Assembly[] assembliesArray = assemblies as Assembly[] ?? [.. assemblies];
		assembliesArray = assembliesArray is { Length: > 0 } ? assembliesArray : [Assembly.GetCallingAssembly()];

		List<Type> types = [.. assembliesArray
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type =>
				type is
				{
					IsAbstract: false,
					IsInterface: false,
					IsGenericType: false
				}
				&& Array.Exists(type.GetInterfaces(),
					t => !t.IsGenericType
					&& t == typeof(IUseService)))];

		foreach (Type type in types)
		{
			if (Activator.CreateInstance(type) is IUseService useService)
			{
				useService.UseServices(application);
			}
		}

		return application;
	}
}
