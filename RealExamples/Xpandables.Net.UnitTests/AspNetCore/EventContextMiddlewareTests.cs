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
using System.Events;

using FluentAssertions;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Xpandables.Net.UnitTests.AspNetCore;

public sealed class EventContextMiddlewareTests
{
	private readonly AsyncLocalEventContextAccessor _accessor = new();

	private EventContextMiddleware CreateMiddleware(EventContextOptions? options = null)
	{
		options ??= new EventContextOptions();
		var monitor = new TestOptionsMonitor(options);
		return new EventContextMiddleware(_accessor, monitor);
	}

	[Fact]
	public async Task InvokeAsync_SetsCorrelationIdFromTraceparentHeader()
	{
		// Arrange
		var middleware = CreateMiddleware();
		var context = new DefaultHttpContext();
		context.Request.Headers["traceparent"] = "00-abc123-def456-01";

		EventContext? captured = null;

		// Act
		await middleware.InvokeAsync(context, _ =>
		{
			captured = _accessor.Current;
			return Task.CompletedTask;
		});

		// Assert
		captured.Should().NotBeNull();
		captured!.Value.CorrelationId.Should().Be("00-abc123-def456-01");
	}

	[Fact]
	public async Task InvokeAsync_SetsCausationIdFromHeader()
	{
		// Arrange
		var middleware = CreateMiddleware();
		var context = new DefaultHttpContext();
		context.Request.Headers["X-Causation-Id"] = "cause-789";

		EventContext? captured = null;

		// Act
		await middleware.InvokeAsync(context, _ =>
		{
			captured = _accessor.Current;
			return Task.CompletedTask;
		});

		// Assert
		captured.Should().NotBeNull();
		captured!.Value.CausationId.Should().Be("cause-789");
	}

	[Fact]
	public async Task InvokeAsync_WithoutHeaders_ContextHasNullIds()
	{
		// Arrange
		var middleware = CreateMiddleware();
		var context = new DefaultHttpContext();

		EventContext? captured = null;

		// Act
		await middleware.InvokeAsync(context, _ =>
		{
			captured = _accessor.Current;
			return Task.CompletedTask;
		});

		// Assert
		captured.Should().NotBeNull();
		// Without Activity or headers, both may be null
		captured!.Value.CausationId.Should().BeNull();
	}

	[Fact]
	public async Task InvokeAsync_EchoesCorrelationIdInResponseHeaders()
	{
		// Arrange
		var middleware = CreateMiddleware();
		var context = new DefaultHttpContext();
		context.Request.Headers["traceparent"] = "00-trace-span-01";

		// Act
		await middleware.InvokeAsync(context, _ => Task.CompletedTask);

		// Assert
		context.Response.Headers.TryGetValue("traceparent", out var values);
		values.ToString().Should().Be("00-trace-span-01");
	}

	[Fact]
	public async Task InvokeAsync_CustomHeaderNames_AreHonored()
	{
		// Arrange
		var options = new EventContextOptions
		{
			CorrelationIdHeaderName = "X-Custom-Corr",
			CausationIdHeaderName = "X-Custom-Cause"
		};
		var middleware = CreateMiddleware(options);
		var context = new DefaultHttpContext();
		context.Request.Headers["X-Custom-Corr"] = "corr-custom";
		context.Request.Headers["X-Custom-Cause"] = "cause-custom";

		EventContext? captured = null;

		// Act
		await middleware.InvokeAsync(context, _ =>
		{
			captured = _accessor.Current;
			return Task.CompletedTask;
		});

		// Assert
		captured!.Value.CorrelationId.Should().Be("corr-custom");
		captured!.Value.CausationId.Should().Be("cause-custom");
		context.Response.Headers.TryGetValue("X-Custom-Corr", out var echoValues);
		echoValues.ToString().Should().Be("corr-custom");
	}

	[Fact]
	public async Task InvokeAsync_ContextScope_IsDisposedAfterNext()
	{
		// Arrange
		var middleware = CreateMiddleware();
		var context = new DefaultHttpContext();
		context.Request.Headers["traceparent"] = "scope-test";

		// Act
		await middleware.InvokeAsync(context, _ => Task.CompletedTask);

		// Assert — after the middleware completes, the scope should be disposed
		// and the accessor should return default (empty) context
		EventContext afterScope = _accessor.Current;
		afterScope.CorrelationId.Should().BeNull();
	}

	/// <summary>
	/// Minimal <see cref="IOptionsMonitor{TOptions}"/> for testing.
	/// </summary>
	private sealed class TestOptionsMonitor(EventContextOptions options) : IOptionsMonitor<EventContextOptions>
	{
		public EventContextOptions CurrentValue => options;
		public EventContextOptions Get(string? name) => options;
		public IDisposable? OnChange(Action<EventContextOptions, string?> listener) => null;
	}
}
