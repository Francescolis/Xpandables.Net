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

using System.Results.Requests;

using Microsoft.Extensions.Caching.Memory;

namespace System.Results.Pipelines;

/// <summary>
/// Marker interface for requests whose results can be cached.
/// </summary>
/// <remarks>
/// Implement this interface on idempotent query-type requests.
/// The <see cref="CacheKey"/> value is used as the cache key and
/// <see cref="AbsoluteExpirationRelativeToNow"/> controls the cache lifetime.
/// </remarks>
public interface ICacheableRequest : IRequest
{
	/// <summary>
	/// Gets the cache key that uniquely identifies this request's result.
	/// </summary>
	string CacheKey { get; }

	/// <summary>
	/// Gets the absolute expiration relative to now for the cached result.
	/// Defaults to 5 minutes when not overridden.
	/// </summary>
	TimeSpan AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
}

/// <summary>
/// A pipeline decorator that caches successful <see cref="Result"/> responses for requests
/// implementing <see cref="ICacheableRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request. Must implement <see cref="ICacheableRequest"/>.</typeparam>
/// <param name="cache">The memory cache used to store and retrieve cached results.</param>
/// <remarks>
/// <para>Only successful results are cached.
/// Failure results are never cached so that retries can succeed on transient errors.</para>
/// <para>Register this decorator in the pipeline after the validation decorator and before
/// the handler to avoid redundant processing of repeat queries.</para>
/// </remarks>
public sealed class PipelineCachingDecorator<TRequest>(IMemoryCache cache) :
	IPipelineDecorator<TRequest>
	where TRequest : class, ICacheableRequest
{
	/// <inheritdoc/>
	public async Task<Result> HandleAsync(
		RequestContext<TRequest> context,
		RequestHandler nextHandler,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(nextHandler);

		string cacheKey = context.Request.CacheKey;

		if (cache.TryGetValue(cacheKey, out Result? cached) && cached is { } cachedResult)
		{
			return cachedResult;
		}

		Result result = await nextHandler(cancellationToken).ConfigureAwait(false);

		if (result.IsSuccess)
		{
			MemoryCacheEntryOptions options = new()
			{
				AbsoluteExpirationRelativeToNow = context.Request.AbsoluteExpirationRelativeToNow
			};

			cache.Set(cacheKey, result, options);
		}

		return result;
	}
}
