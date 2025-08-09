/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Collections;
/// <summary>
/// JSON converter for materialized paged data that serializes pagination info to headers.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
internal sealed class MaterializedPagedDataHeaderJsonConverter<T>(IServiceProvider serviceProvider) : JsonConverter<MaterializedPagedData<T>>
{
    /// <inheritdoc />
    public override MaterializedPagedData<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        throw new NotSupportedException("Deserialization of MaterializedPagedData is not supported.");

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        MaterializedPagedData<T> value,
        JsonSerializerOptions options)
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is not available.");

        httpContext.Response.Headers["X-Total-Count"] = $"{value.Pagination.TotalCount}";
        httpContext.Response.Headers["X-Page-Number"] = $"{value.Pagination.PageNumber}";
        httpContext.Response.Headers["X-Page-Size"] = $"{value.Pagination.PageSize}";
        httpContext.Response.Headers["X-Total-Pages"] = $"{value.Pagination.TotalPages}";
        httpContext.Response.Headers["X-Has-Previous-Page"] = $"{value.Pagination.HasPreviousPage}";
        httpContext.Response.Headers["X-Has-Next-Page"] = $"{value.Pagination.HasNextPage}";
        httpContext.Response.Headers["X-Is-Paginated"] = $"{value.Pagination.IsPaginated}";
        httpContext.Response.Headers["X-Skip"] = $"{value.Pagination.Skip}";
        httpContext.Response.Headers["X-Take"] = $"{value.Pagination.Take}";

        JsonSerializer.Serialize(writer, value.Data, options);
    }
}