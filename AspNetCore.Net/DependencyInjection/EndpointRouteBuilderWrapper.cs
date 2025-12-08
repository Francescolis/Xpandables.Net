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
using Microsoft.AspNetCore.Routing;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A wrapper for <see cref="IEndpointRouteBuilder"/> that uses a <see cref="RouteGroupBuilder"/>
/// to automatically apply filters to all endpoints registered through it.
/// </summary>
public sealed class EndpointRouteBuilderWrapper : IEndpointRouteBuilder
{
    private const string MapEndpointUnreferencedCodeWarning =
        "This API may perform reflection on the supplied delegate and its parameters. " +
        "These types may be trimmed if not directly referenced.";
    private const string MapEndpointDynamicCodeWarning =
        "This API may perform reflection on the supplied delegate and its parameters. " +
        "These types may require generated code and aren't compatible with native AOT applications.";

    private readonly IEndpointRouteBuilder _inner;
    private readonly RouteGroupBuilder _rootGroup;
    private readonly ResultSupportOptions? _options;

    internal EndpointRouteBuilderWrapper(
        IEndpointRouteBuilder inner,
        ResultSupportOptions? options)
    {
        _inner = inner;
        _options = options;

        // Create a root group with empty prefix - all endpoints go through this
        _rootGroup = EndpointRouteBuilderExtensions.MapGroup(inner, string.Empty);

        // Apply default filters to the root group if no custom configuration is provided
        if (options is not null && options.EndpointPredicate is null && options.ConfigureEndpoint is null)
        {
            if (options.EnableValidationFilter)
            {
                _rootGroup.AddEndpointFilter(new ResultEndpointValidationFilter().InvokeAsync);
            }

            if (options.EnableResultFilter)
            {
                _rootGroup.AddEndpointFilter<ResultEndpointFilter>();
            }
        }
    }

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider => _inner.ServiceProvider;

    /// <inheritdoc/>
    public ICollection<EndpointDataSource> DataSources => _inner.DataSources;

    /// <inheritdoc/>
    public IApplicationBuilder CreateApplicationBuilder() => _inner.CreateApplicationBuilder();

    private bool RequiresPerEndpointConfiguration =>
        _options?.EndpointPredicate is not null || _options?.ConfigureEndpoint is not null;

    private RouteHandlerBuilder ApplyFilters(RouteHandlerBuilder builder)
    {
        if (_options is null || !RequiresPerEndpointConfiguration)
        {
            return builder;
        }

        if (_options.EnableValidationFilter)
        {
            builder.WithXResultValidation();
        }

        if (_options.EnableResultFilter)
        {
            builder.WithXResultFilter();
        }

        return builder;
    }

    private IEndpointConventionBuilder ApplyFilters(IEndpointConventionBuilder builder)
    {
        if (_options is null || RequiresPerEndpointConfiguration)
        {
            return builder;
        }

        if (_options.EnableValidationFilter)
        {
            builder.AddEndpointFilter(new ResultEndpointValidationFilter().InvokeAsync);
        }

        if (_options.EnableResultFilter)
        {
            builder.AddEndpointFilter(new ResultEndpointFilter().InvokeAsync);
        }

        return builder;
    }

    #region MapGroup

    /// <summary>
    /// Creates a <see cref="RouteGroupBuilder"/> for defining endpoints with a common prefix.
    /// Filters are automatically inherited by all endpoints in the group.
    /// </summary>
    public RouteGroupBuilder MapGroup([StringSyntax("Route")] string prefix) =>
        _rootGroup.MapGroup(prefix);

    #endregion

    #region MapGet

    /// <summary>
    /// Maps a GET endpoint with a <see cref="Delegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public RouteHandlerBuilder MapGet([StringSyntax("Route")] string pattern, Delegate handler) =>
        ApplyFilters(_rootGroup.MapGet(pattern, handler));

    /// <summary>
    /// Maps a GET endpoint with a <see cref="RequestDelegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    public IEndpointConventionBuilder MapGet([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
        ApplyFilters(_rootGroup.MapGet(pattern, requestDelegate));

    #endregion

    #region MapPost

    /// <summary>
    /// Maps a POST endpoint with a <see cref="Delegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public RouteHandlerBuilder MapPost([StringSyntax("Route")] string pattern, Delegate handler) =>
        ApplyFilters(_rootGroup.MapPost(pattern, handler));

    /// <summary>
    /// Maps a POST endpoint with a <see cref="RequestDelegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    public IEndpointConventionBuilder MapPost([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
        ApplyFilters(_rootGroup.MapPost(pattern, requestDelegate));

    #endregion

    #region MapPut

    /// <summary>
    /// Maps a PUT endpoint with a <see cref="Delegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public RouteHandlerBuilder MapPut([StringSyntax("Route")] string pattern, Delegate handler) =>
        ApplyFilters(_rootGroup.MapPut(pattern, handler));

    /// <summary>
    /// Maps a PUT endpoint with a <see cref="RequestDelegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    public IEndpointConventionBuilder MapPut([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
        ApplyFilters(_rootGroup.MapPut(pattern, requestDelegate));

    #endregion

    #region MapDelete

    /// <summary>
    /// Maps a DELETE endpoint with a <see cref="Delegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public RouteHandlerBuilder MapDelete([StringSyntax("Route")] string pattern, Delegate handler) =>
        ApplyFilters(_rootGroup.MapDelete(pattern, handler));

    /// <summary>
    /// Maps a DELETE endpoint with a <see cref="RequestDelegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    public IEndpointConventionBuilder MapDelete([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
        ApplyFilters(_rootGroup.MapDelete(pattern, requestDelegate));

    #endregion

    #region MapPatch

    /// <summary>
    /// Maps a PATCH endpoint with a <see cref="Delegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public RouteHandlerBuilder MapPatch([StringSyntax("Route")] string pattern, Delegate handler) =>
        ApplyFilters(_rootGroup.MapPatch(pattern, handler));

    /// <summary>
    /// Maps a PATCH endpoint with a <see cref="RequestDelegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    public IEndpointConventionBuilder MapPatch([StringSyntax("Route")] string pattern, RequestDelegate requestDelegate) =>
        ApplyFilters(_rootGroup.MapPatch(pattern, requestDelegate));

    #endregion

    #region MapMethods

    /// <summary>
    /// Maps endpoints for the specified HTTP methods with a <see cref="Delegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public RouteHandlerBuilder MapMethods(
        [StringSyntax("Route")] string pattern,
        IEnumerable<string> httpMethods,
        Delegate handler) =>
        ApplyFilters(_rootGroup.MapMethods(pattern, httpMethods, handler));

    /// <summary>
    /// Maps endpoints for the specified HTTP methods with a <see cref="RequestDelegate"/> handler.
    /// Filters are automatically applied.
    /// </summary>
    public IEndpointConventionBuilder MapMethods(
        [StringSyntax("Route")] string pattern,
        IEnumerable<string> httpMethods,
        RequestDelegate requestDelegate) =>
        ApplyFilters(_rootGroup.MapMethods(pattern, httpMethods, requestDelegate));

    #endregion
}