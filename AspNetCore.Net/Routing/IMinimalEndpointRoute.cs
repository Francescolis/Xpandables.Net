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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Defines a contract for configuring minimal application routes, registering services, and setting up middleware for
/// endpoint-based routing in a web application.
/// </summary>
/// <remarks>Implementations of this interface enable modular configuration of routing, dependency injection, and
/// middleware setup for ASP.NET Core applications. Methods provide extension points for adding custom routes,
/// registering services with the application's service collection, and configuring middleware in the application's
/// request pipeline. This interface is typically used to organize application startup logic and promote separation of
/// concerns in web projects.</remarks>
public interface IMinimalEndpointRoute
{
    /// <summary>
    /// Configures and adds application-specific routes to the provided endpoint route builder.
    /// </summary>
    /// <param name="app">The <see cref="MinimalRouteBuilder"/> used to define and build the application's routing endpoints. Cannot be
    /// null.</param>
    void AddRoutes(MinimalRouteBuilder app);

    /// <summary>
    /// Adds services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public void AddServices(IServiceCollection services) { }

    /// <summary>
    /// Adds services to the specified service collection using the provided configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to use for adding services.</param>
    public void AddServices(IServiceCollection services, IConfiguration configuration) =>
        AddServices(services);

    /// <summary>  
    /// Configures the middleware for the specified <see cref="WebApplication"/>.  
    /// </summary>  
    /// <param name="application">The <see cref="WebApplication"/> 
    /// to configure.</param>  
    public virtual void UseServices(WebApplication application) { }
}
