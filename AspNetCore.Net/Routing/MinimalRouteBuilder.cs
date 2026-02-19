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

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides a minimal implementation of <see cref="IEndpointRouteBuilder"/> for building endpoints in minimal APIs.
/// </summary>
/// <remarks>This class is intended for internal use within the minimal APIs infrastructure and is not designed
/// for direct use in application code. It wraps an existing <see cref="IEndpointRouteBuilder"/> and applies minimal
/// API-specific conventions and configuration. All endpoints are grouped under a root route group with no prefix. For
/// most scenarios, use the standard routing APIs provided by ASP.NET Core.</remarks>
public sealed class MinimalRouteBuilder : IEndpointRouteBuilder
{
    internal const string MapEndpointUnreferencedCodeWarning =
        "This API may perform reflection on the supplied delegate and its parameters. " +
        "These types may be trimmed if not directly referenced.";
    internal const string MapEndpointDynamicCodeWarning =
        "This API may perform reflection on the supplied delegate and its parameters. " +
        "These types may require generated code and aren't compatible with native AOT applications.";

    private readonly IEndpointRouteBuilder _inner;
    internal readonly RouteGroupBuilder _rootGroup;
    private readonly MinimalSupportOptions _options;

    internal MinimalRouteBuilder(IEndpointRouteBuilder inner, MinimalSupportOptions options)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        // Create a root group with empty prefix - all endpoints go through this
        _rootGroup = EndpointRouteBuilderExtensions.MapGroup(inner, string.Empty);
    }

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider => _inner.ServiceProvider;

    /// <inheritdoc/>
    public ICollection<EndpointDataSource> DataSources => _inner.DataSources;

    /// <inheritdoc/>
    public IApplicationBuilder CreateApplicationBuilder() => _inner.CreateApplicationBuilder();

    private bool RequiresPerEndpointConfiguration =>
        _options.EndpointPredicate is not null || _options.ConfigureEndpoint is not null;

    internal RouteHandlerBuilder ApplyFilters(RouteHandlerBuilder builder)
    {
        if (!RequiresPerEndpointConfiguration)
        {
            // Filters already applied at root group level
            return builder;
        }

        // When predicate exists, wrap filters to evaluate at runtime
        if (_options.EndpointPredicate is not null)
        {
            if (_options.ConfigureEndpoint is not null)
			{
				builder.AddEndpointFilter(CreateConditionalFilter(
                    _options.EndpointPredicate, _options.ConfigureEndpoint, builder));
			}
		}
        else
        {
            if (_options.ConfigureEndpoint is not null)
			{
				_options.ConfigureEndpoint(builder);
			}
		}

        return builder;
    }

    internal IEndpointConventionBuilder ApplyFilters(IEndpointConventionBuilder builder)
    {
        if (!RequiresPerEndpointConfiguration)
        {
            // Filters already applied at root group level
            return builder;
        }

        // For RequestDelegate handlers, we can only apply custom configuration
        // Endpoint filters are not supported on IEndpointConventionBuilder
        if (_options.ConfigureEndpoint is not null)
        {
            _options.ConfigureEndpoint(builder);
        }

        return builder;
    }

    private static Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>> CreateConditionalFilter(
        Func<RouteEndpoint, bool> predicate,
        Action<IEndpointConventionBuilder> configure,
        RouteHandlerBuilder builder)
    {
        return async (context, next) =>
        {
            // Evaluate predicate at runtime using the current endpoint
            if (context.HttpContext.GetEndpoint() is RouteEndpoint routeEndpoint
                && predicate(routeEndpoint))
            {
                // Apply configuration to the builder
                configure(builder);
                return await next(context).ConfigureAwait(false);
            }
            // Skip configuration if predicate doesn't match
            return await next(context).ConfigureAwait(false);
        };
    }
}