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

using System.Composition;

using Microsoft.AspNetCore.Routing;

namespace AspNetCore.Net;

/// <summary>
/// Defines a contract for adding routes to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
/// <remarks>This interface is typically implemented by components that configure endpoint routing in an
/// application. It provides a method to register routes with an <see cref="IEndpointRouteBuilder"/> instance, enabling
/// the setup of endpoint-based routing for HTTP requests.</remarks>
public interface IEndpointRoute : IAddService, IUseService
{
    /// <summary>
    /// Configures and adds application-specific routes to the provided endpoint route builder.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> used to define and build the application's routing endpoints. Cannot be
    /// null.</param>
    void AddRoutes(IEndpointRouteBuilder app);
}
