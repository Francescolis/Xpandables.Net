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
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides extension methods for mapping HTTP endpoints and route groups with automatic filter application in minimal
/// APIs.
/// </summary>
/// <remarks>These extensions simplify the registration of GET, POST, PUT, DELETE, PATCH, and custom HTTP method
/// endpoints, as well as route groups, by ensuring that filters configured on the builder are consistently applied to
/// all mapped endpoints. Use these methods to define endpoints with handlers and to organize related endpoints under a
/// common route prefix.</remarks>
public static class MinimalRouteBuilderExtensions
{
    extension(MinimalRouteBuilder builder)
    {
        #region MapGroup

        /// <summary>
        /// Creates a <see cref="RouteGroupBuilder"/> for defining endpoints with a common prefix.
        /// Filters are automatically inherited by all endpoints in the group.
        /// </summary>
        public RouteGroupBuilder MapGroup([StringSyntax("Route")] string prefix) =>
            builder._rootGroup.MapGroup(prefix);

        #endregion

        #region MapGet

        /// <summary>
        /// Maps a GET endpoint with a <see cref="Delegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        [RequiresUnreferencedCode(MinimalRouteBuilder.MapEndpointUnreferencedCodeWarning)]
        [RequiresDynamicCode(MinimalRouteBuilder.MapEndpointDynamicCodeWarning)]
        public RouteHandlerBuilder MapGet([StringSyntax("Route")] string pattern, Delegate handler) =>
            builder.ApplyFilters(builder._rootGroup.MapGet(pattern, handler));

        /// <summary>
        /// Maps a GET endpoint with a <see cref="RequestDelegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        public IEndpointConventionBuilder MapGet([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
            builder.ApplyFilters(builder._rootGroup.MapGet(pattern, requestDelegate));

        #endregion

        #region MapPost

        /// <summary>
        /// Maps a POST endpoint with a <see cref="Delegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        [RequiresUnreferencedCode(MinimalRouteBuilder.MapEndpointUnreferencedCodeWarning)]
        [RequiresDynamicCode(MinimalRouteBuilder.MapEndpointDynamicCodeWarning)]
        public RouteHandlerBuilder MapPost([StringSyntax("Route")] string pattern, Delegate handler) =>
            builder.ApplyFilters(builder._rootGroup.MapPost(pattern, handler));

        /// <summary>
        /// Maps a POST endpoint with a <see cref="RequestDelegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        public IEndpointConventionBuilder MapPost([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
            builder.ApplyFilters(builder._rootGroup.MapPost(pattern, requestDelegate));

        #endregion

        #region MapPut

        /// <summary>
        /// Maps a PUT endpoint with a <see cref="Delegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        [RequiresUnreferencedCode(MinimalRouteBuilder.MapEndpointUnreferencedCodeWarning)]
        [RequiresDynamicCode(MinimalRouteBuilder.MapEndpointDynamicCodeWarning)]
        public RouteHandlerBuilder MapPut([StringSyntax("Route")] string pattern, Delegate handler) =>
            builder.ApplyFilters(builder._rootGroup.MapPut(pattern, handler));

        /// <summary>
        /// Maps a PUT endpoint with a <see cref="RequestDelegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        public IEndpointConventionBuilder MapPut([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
            builder.ApplyFilters(builder._rootGroup.MapPut(pattern, requestDelegate));

        #endregion

        #region MapDelete

        /// <summary>
        /// Maps a DELETE endpoint with a <see cref="Delegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        [RequiresUnreferencedCode(MinimalRouteBuilder.MapEndpointUnreferencedCodeWarning)]
        [RequiresDynamicCode(MinimalRouteBuilder.MapEndpointDynamicCodeWarning)]
        public RouteHandlerBuilder MapDelete([StringSyntax("Route")] string pattern, Delegate handler) =>
            builder.ApplyFilters(builder._rootGroup.MapDelete(pattern, handler));

        /// <summary>
        /// Maps a DELETE endpoint with a <see cref="RequestDelegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        public IEndpointConventionBuilder MapDelete([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
            builder.ApplyFilters(builder._rootGroup.MapDelete(pattern, requestDelegate));

        #endregion

        #region MapPatch

        /// <summary>
        /// Maps a PATCH endpoint with a <see cref="Delegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        [RequiresUnreferencedCode(MinimalRouteBuilder.MapEndpointUnreferencedCodeWarning)]
        [RequiresDynamicCode(MinimalRouteBuilder.MapEndpointDynamicCodeWarning)]
        public RouteHandlerBuilder MapPatch([StringSyntax("Route")] string pattern, Delegate handler) =>
            builder.ApplyFilters(builder._rootGroup.MapPatch(pattern, handler));

        /// <summary>
        /// Maps a PATCH endpoint with a <see cref="RequestDelegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        public IEndpointConventionBuilder MapPatch([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
            builder.ApplyFilters(builder._rootGroup.MapPatch(pattern, requestDelegate));

        #endregion

        #region MapMethods

        /// <summary>
        /// Maps endpoints for the specified HTTP methods with a <see cref="Delegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        [RequiresUnreferencedCode(MinimalRouteBuilder.MapEndpointUnreferencedCodeWarning)]
        [RequiresDynamicCode(MinimalRouteBuilder.MapEndpointDynamicCodeWarning)]
        public RouteHandlerBuilder MapMethods(
            [StringSyntax("Route")] string pattern,
            IEnumerable<string> httpMethods,
            Delegate handler) =>
            builder.ApplyFilters(builder._rootGroup.MapMethods(pattern, httpMethods, handler));

        /// <summary>
        /// Maps endpoints for the specified HTTP methods with a <see cref="RequestDelegate"/> handler.
        /// Filters are automatically applied.
        /// </summary>
        public IEndpointConventionBuilder MapMethods(
            [StringSyntax("Route")] string pattern,
            IEnumerable<string> httpMethods,
            RequestDelegate requestDelegate) =>
            builder.ApplyFilters(builder._rootGroup.MapMethods(pattern, httpMethods, requestDelegate));

        #endregion
    }
}
