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
namespace Microsoft.AspNetCore.Http;

using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text.Json;

/// <summary>
/// A non-generic <see cref="IResult"/> implementation that serializes an
/// <see cref="IAsyncPagedEnumerable"/> using the non-generic JSON serialization paths.
/// </summary>
/// <remarks>
/// This type avoids <c>MakeGenericType</c> / <c>Activator.CreateInstance</c> at runtime,
/// making it safe for AOT / trimming scenarios. It is used internally by
/// <see cref="AsyncPagedEnpointFilter"/> when the concrete generic type is not
/// known at compile time.
/// </remarks>
internal sealed class AsyncPagedNonGenericResult(IAsyncPagedEnumerable results) : IResult
{
    private readonly IAsyncPagedEnumerable _results = results ?? throw new ArgumentNullException(nameof(results));

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Non-generic serialization path delegates to annotated methods.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Non-generic serialization path delegates to annotated methods.")]
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType ??= httpContext.GetContentType("application/json; charset=utf-8");
		CancellationToken cancellationToken = httpContext.RequestAborted;
		PipeWriter pipeWriter = httpContext.Response.BodyWriter;

		JsonSerializerOptions options = httpContext.GetJsonSerializerOptions();

        await JsonSerializer
            .SerializeAsyncPaged(pipeWriter, _results, options, cancellationToken)
            .ConfigureAwait(false);
    }
}
