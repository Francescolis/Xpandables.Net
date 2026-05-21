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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering and configuring endpoint routes in an ASP.NET Core application.
/// </summary>
/// <remarks>These extension methods enable the discovery and registration of endpoint route implementations from
/// specified assemblies, as well as the configuration of the application's request pipeline to use those routes. Use
/// these methods to simplify the integration of modular endpoint routing patterns in your application.</remarks>
public static class IAsyncPagedRouteExtensions
{
	/// <summary>
	/// Adds a typed asynchronous paged endpoint filter to the builder configuration.
	/// </summary>
	/// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
	/// <typeparam name="TResult">The item type produced by the paged enumerable returned by the endpoint.</typeparam>
	/// <param name="builder">The endpoint convention builder to apply the filter to.</param>
	/// <returns>The builder instance with the typed asynchronous paged filter applied.</returns>
	public static TBuilder WithXAsyncPagedFilterSupport<TBuilder, TResult>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.AddEndpointFilter<TBuilder, AsyncPagedEndpointFilter<TResult>>();
}
