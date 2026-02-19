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
using System.Diagnostics;
using System.Events;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore;

/// <summary>
/// Middleware that manages the event context for each HTTP request, setting correlation and causation identifiers for
/// distributed tracing and event tracking.
/// </summary>
/// <remarks>EventContextMiddleware reads correlation and causation IDs from incoming HTTP headers or the current
/// activity, and makes them available via IEventContextAccessor for the duration of the request. It also ensures the
/// correlation ID is included in the response headers if present. This middleware is typically used to enable
/// end-to-end tracing and event correlation across distributed systems. Thread safety is ensured for per-request
/// context. Register this middleware early in the pipeline to ensure downstream components have access to the event
/// context.</remarks>
public sealed class EventContextMiddleware : Disposable, IMiddleware
{
    private readonly IDisposable? _disposable;
    private readonly IEventContextAccessor _contextAccessor;
    private EventContextOptions _options;

    /// <summary>
    /// Initializes a new instance of the EventContextMiddleware class with the specified event context accessor and
    /// options.
    /// </summary>
    /// <remarks>The middleware subscribes to changes in the provided options and updates its configuration
    /// automatically when options are changed at runtime.</remarks>
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

		string? correlationValue =
            ReadHeaderValue(context, _options.CorrelationIdHeaderName) ??
            GetTraceParentFromActivity(Activity.Current);

		string? causationValue = ReadHeaderValue(context, _options.CausationIdHeaderName);

        var eventContext = new EventContext
        {
            CorrelationId = correlationValue,
            CausationId = causationValue
        };

        using IDisposable _ = _contextAccessor.BeginScope(eventContext);

        if (!string.IsNullOrWhiteSpace(correlationValue))
        {
            context.Response.Headers[_options.CorrelationIdHeaderName] = correlationValue;
        }

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

    private static string? ReadHeaderValue(HttpContext httpContext, string headerName)
    {
        if (!httpContext.Request.Headers.TryGetValue(headerName, out StringValues values))
        {
            return null;
        }

		string? value = values.Count > 0 ? values[0] : null;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? GetTraceParentFromActivity(Activity? activity)
    {
        if (activity is null)
        {
            return null;
        }

		// Keep aligned with W3C: activity.Id is the W3C "traceparent" when IdFormat is W3C.
		string? id = activity.Id;
        return string.IsNullOrWhiteSpace(id) ? null : id;
    }
}