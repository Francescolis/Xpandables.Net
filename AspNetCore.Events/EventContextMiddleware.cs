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
using System.Events;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore;

/// <summary>
/// Middleware that establishes an event context for each HTTP request, providing correlation and causation identifiers
/// for distributed tracing and event tracking.
/// </summary>
/// <remarks>This middleware reads the correlation and causation identifiers from the incoming request headers, or
/// generates a new correlation identifier if one is not provided. The identifiers are made available via the event
/// context accessor for the duration of the request, and the correlation identifier is added to the response headers.
/// Use this middleware to enable end-to-end tracing and event correlation across distributed systems.</remarks>
public sealed class EventContextMiddleware : Disposable, IMiddleware
{
    private readonly IDisposable? _disposable;
    private readonly IEventContextAccessor _contextAccessor;
    private EventContextOptions _options;

    /// <summary>
    /// Initializes a new instance of the EventContextMiddleware class with the specified event context accessor and
    /// options.
    /// </summary>
    /// <remarks>The middleware subscribes to changes in the provided options monitor and updates its
    /// configuration dynamically when options change.</remarks>
    /// <param name="contextAccessor">The accessor used to retrieve and manage the current event context.</param>
    /// <param name="options">The options monitor that provides configuration settings for event context behavior. The current value is used
    /// at construction, and updates are applied automatically when options change.</param>
    /// <exception cref="ArgumentNullException">Thrown if contextAccessor or options is null.</exception>
    public EventContextMiddleware(
        IEventContextAccessor contextAccessor,
        IOptionsMonitor<EventContextOptions> options)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
        _disposable = options.OnChange(updatedOptions => _options = updatedOptions);
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var correlationId = ReadGuidHeader(context, _options.CorrelationIdHeaderName) ?? Guid.NewGuid();
        var causationId = ReadGuidHeader(context, _options.CausationIdHeaderName);

        var eventContext = new EventContext
        {
            CorrelationId = correlationId,
            CausationId = causationId
        };

        using var _ = _contextAccessor.BeginScope(eventContext);

        context.Response.Headers[_options.CorrelationIdHeaderName] = correlationId.ToString("D");

        await next(context).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposable?.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Guid? ReadGuidHeader(HttpContext httpContext, string headerName)
    {
        if (!httpContext.Request.Headers.TryGetValue(headerName, out var values))
        {
            return null;
        }

        var value = values.Count > 0 ? values[0] : null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}