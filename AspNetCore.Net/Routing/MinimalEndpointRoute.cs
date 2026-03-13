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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides an abstract base class for defining minimal endpoint routes in a web application.
/// </summary>
/// <remarks>Derived classes must implement the AddRoutes method to specify routing behavior. This class also
/// provides virtual methods for adding services to the dependency injection container and configuring application
/// services, allowing for customization of service registration and usage.</remarks>
public abstract class MinimalEndpointRoute : IMinimalEndpointRoute
{
	/// <inheritdoc/>
	public abstract void AddRoutes(MinimalRouteBuilder app);
	/// <inheritdoc/>
	public virtual void AddServices(IServiceCollection services) { }
	/// <inheritdoc/>
	public virtual void AddServices(IServiceCollection services, IConfiguration configuration) => AddServices(services);
	/// <inheritdoc/>
	public virtual void UseServices(WebApplication application) { }
}
