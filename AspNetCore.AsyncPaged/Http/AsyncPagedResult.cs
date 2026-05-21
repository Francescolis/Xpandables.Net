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
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.AspNetCore.Http;
/// <summary>
/// Represents an HTTP result that asynchronously serializes and streams a paged enumerable of results as JSON in
/// response to a request.
/// </summary>
/// <remarks>This result is intended for use in ASP.NET Core endpoints that return large or paged datasets,
/// enabling efficient streaming of JSON data to the client. The serialization can be customized using optional JSON
/// serializer options or type information. The response content type is set to "application/json; charset=utf-8" unless
/// otherwise specified by endpoint metadata. Thread safety is ensured for serializer options caching.</remarks>
/// <typeparam name="TResult">The type of elements contained in the paged enumerable to be serialized and streamed.</typeparam>
/// <remarks>
/// Initializes a new instance of the AsyncPagedEnumerableResult class to provide asynchronous, paged access to a
/// sequence of results with optional JSON serialization settings.
/// </remarks>
/// <param name="results">An asynchronous paged enumerable that supplies the sequence of result items. Cannot be null.</param>
/// <param name="serializerOptions">Optional JSON serialization options to customize how result items are serialized or deserialized. If null,
/// default serialization settings are used.</param>
/// <param name="jsonTypeInfo">Optional type metadata for JSON serialization of result items. If specified, this overrides type information
/// inferred from the result type.</param>
/// <exception cref="ArgumentNullException">Thrown if the results parameter is null.</exception>
public sealed class AsyncPagedResult<TResult>(
	IAsyncPagedEnumerable<TResult> results,
	JsonSerializerOptions? serializerOptions = null,
	JsonTypeInfo<TResult>? jsonTypeInfo = null) : IResult
{
	private readonly IAsyncPagedEnumerable<TResult> _results = results ?? throw new ArgumentNullException(nameof(results));
	private volatile JsonTypeInfo<TResult>? _jsonTypeInfo = jsonTypeInfo;
	private readonly JsonSerializerOptions? _serializerOptions = serializerOptions;

	/// <inheritdoc/>
	public async Task ExecuteAsync(HttpContext httpContext)
	{
		ArgumentNullException.ThrowIfNull(httpContext);

		httpContext.Response.ContentType ??= httpContext.GetContentType("application/json; charset=utf-8");
		CancellationToken cancellationToken = httpContext.RequestAborted;
		PipeWriter pipeWriter = httpContext.Response.BodyWriter;

		JsonSerializerOptions options = _serializerOptions ?? httpContext.GetJsonSerializerOptions();
		JsonTypeInfo<TResult>? typeInfo = _jsonTypeInfo ?? ResolveJsonTypeInfo(options);

		typeInfo ??= ResolveJsonTypeInfo(options)
			?? throw new InvalidOperationException(
				$"No JsonTypeInfo<{typeof(TResult).Name}> was found. Register source-generated metadata or pass JsonTypeInfo explicitly.");

		await JsonSerializer
			.SerializeAsyncPaged(pipeWriter, _results, typeInfo, cancellationToken)
			.ConfigureAwait(false);
	}

	private JsonTypeInfo<TResult>? ResolveJsonTypeInfo(JsonSerializerOptions options)
	{
		var resolved = (JsonTypeInfo<TResult>?)options.GetTypeInfo(typeof(TResult));
		_jsonTypeInfo = resolved;
		return resolved;
	}
}
