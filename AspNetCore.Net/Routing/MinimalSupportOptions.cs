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

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Represents configuration options for minimal support features, endpoint selection, 
/// and endpoint customization during route registration.
/// </summary>
public sealed record MinimalSupportOptions
{
    /// <summary>
    /// Gets or sets a predicate used to determine whether a given <see cref="RouteEndpoint"/> should have filters applied.
    /// </summary>
    /// <remarks>If set to <see langword="null"/>, all endpoints have filters applied. The predicate is
    /// evaluated at runtime for each request.</remarks>
    public Func<RouteEndpoint, bool>? EndpointPredicate { get; set; }

    /// <summary>
    /// Gets or sets a delegate that configures the endpoint convention builder during route registration.
    /// </summary>
    /// <remarks>The delegate receives the endpoint convention builder, allowing customization of endpoint 
    /// metadata, conventions, or behavior. This is called during endpoint registration, before the endpoint 
    /// is built.</remarks>
    public Action<IEndpointConventionBuilder>? ConfigureEndpoint { get; set; }
}
