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

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides an endpoint filter that transforms asynchronous paged enumerable results into paged response objects when
/// applicable.
/// </summary>
/// <remarks>Use this filter to automatically wrap endpoint results of type <see cref="IAsyncPagedEnumerable"/> in
/// a <see cref="AsyncPagedResult{T}"/> for consistent paged response semantics. If the endpoint does not produce an
/// <see cref="IAsyncPagedEnumerable"/>, the result is returned unchanged. This filter is typically used in scenarios
/// where endpoints may return large datasets that benefit from paging.</remarks>
public sealed class AsyncPagedEnpointFilter : IEndpointFilter
{
    /// <summary>
    /// Invokes the next endpoint filter delegate asynchronously and transforms the result into a paged response if
    /// applicable.
    /// </summary>
    /// <remarks>If the result of the endpoint is an <see cref="IAsyncPagedEnumerable"/>, it is wrapped in a
    /// <see cref="AsyncPagedResult{T}"/> to provide paged response semantics. Otherwise, the original result is
    /// returned unchanged.</remarks>
    /// <param name="context">The invocation context containing information about the current endpoint execution. Cannot be null.</param>
    /// <param name="next">The delegate representing the next filter or endpoint in the pipeline. Cannot be null.</param>
    /// <returns>A <see cref="ValueTask{Object}"/> that represents the asynchronous operation. Returns a paged result if the
    /// endpoint produces an <see cref="IAsyncPagedEnumerable"/>; otherwise, returns the original result.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:'GetArgumentType' uses reflection to discover implemented interfaces", Justification = "Used for dynamic dispatch in ASP.NET Core filter")]
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        object? result = await next(context).ConfigureAwait(false);

        if (result is null)
            return result;

        IAsyncPagedEnumerable? pagedEnumerable = result switch
        {
            IAsyncPagedEnumerable paged => paged,
            ObjectResult { Value: IAsyncPagedEnumerable paged } => paged,
            _ => null
        };

        if (pagedEnumerable is null)
        {
            return result;
        }

        Type itemType = pagedEnumerable.GetArgumentType();
        Type resultAsyncPagedType = typeof(AsyncPagedResult<>).MakeGenericType(itemType);

        object resultAsyncPaged = Activator.CreateInstance(resultAsyncPagedType, pagedEnumerable, null, null)!;

        return resultAsyncPaged;
    }
}
