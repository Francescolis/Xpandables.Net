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
using Microsoft.AspNetCore.Routing;

namespace AspNetCore.Net;

/// <summary>
/// Represents configuration options for minimal support features, including validation and result filtering, endpoint
/// selection, and endpoint customization during route registration.
/// </summary>
/// <remarks>Use this type to control the behavior of minimal support features in the routing system. Options
/// include enabling or disabling request validation and result filtering, specifying custom logic for endpoint
/// inclusion, and applying additional configuration to endpoints as they are registered. All properties are optional
/// and can be tailored to meet specific application requirements.</remarks>
public sealed record MinimalSupportOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the validation filter is enabled for incoming requests.
    /// </summary>
    public bool EnableValidationFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether result filtering is enabled for the operation.
    /// </summary>
    public bool EnableResultFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a predicate used to determine whether a given <see cref="RouteEndpoint"/> should be included.
    /// </summary>
    /// <remarks>If set to <see langword="null"/>, all endpoints are considered eligible. The predicate is
    /// invoked for each endpoint to filter the set based on custom logic.</remarks>
    public Func<RouteEndpoint, bool>? EndpointPredicate { get; set; }

    /// <summary>
    /// Gets or sets a delegate that configures the endpoint during route registration.
    /// </summary>
    /// <remarks>The delegate receives the endpoint convention builder and the route endpoint, allowing
    /// customization of endpoint metadata, conventions, or behavior. This property is typically used to apply
    /// additional configuration, such as authorization policies or custom metadata, to endpoints as they are added to
    /// the routing system.</remarks>
    public Action<IEndpointConventionBuilder, RouteEndpoint>? ConfigureEndpoint { get; set; }
}
