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
namespace System;

/// <summary>
/// Provides ambient access to the current request-scoped <see cref="IServiceProvider"/>
/// across asynchronous execution boundaries.
/// </summary>
/// <remarks>
/// <para>This accessor uses <see cref="AsyncLocal{T}"/> to flow the current request scope's service provider
/// into child async contexts. It enables singleton services to resolve scoped services from the correct request scope
/// instead of creating an isolated child scope that would receive separate scoped service instances.</para>
/// <para>The value must be set by the pipeline entry point (e.g., the Mediator) before dispatching,
/// and restored after the pipeline completes. Use <see cref="BeginScope"/> to ensure proper cleanup:</para>
/// <code>
/// using var scope = RequestScopeAccessor.BeginScope(serviceProvider);
/// // pipeline executes with the ambient provider
/// </code>
/// <para>When no ambient provider is set (e.g., events published from a background service),
/// consumers should fall back to creating a new scope via <see cref="Microsoft.Extensions.DependencyInjection.IServiceScopeFactory"/>.</para>
/// </remarks>
public static class RequestScopeAccessor
{
	private static readonly AsyncLocal<IServiceProvider?> _provider = new();

	/// <summary>
	/// Gets the current request-scoped service provider for the logical execution flow,
	/// or <see langword="null"/> if no ambient scope has been set.
	/// </summary>
	public static IServiceProvider? Current => _provider.Value;

	/// <summary>
	/// Sets the ambient service provider and returns a disposable scope that restores
	/// the previous value when disposed.
	/// </summary>
	/// <param name="provider">The request-scoped service provider to make available to the current async flow.</param>
	/// <returns>An <see cref="IDisposable"/> that restores the previous ambient provider on dispose.</returns>
	public static IDisposable BeginScope(IServiceProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);

		IServiceProvider? prior = _provider.Value;
		_provider.Value = provider;
		return new RestoreScope(prior);
	}

	private sealed class RestoreScope(IServiceProvider? prior) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			_provider.Value = prior;
		}
	}
}
