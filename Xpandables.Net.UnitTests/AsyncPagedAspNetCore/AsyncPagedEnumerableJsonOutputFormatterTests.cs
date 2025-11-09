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
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Controllers;

namespace Xpandables.Net.UnitTests.AsyncPagedAspNetCore;

public class AsyncPagedEnumerableJsonOutputFormatterTests
{
    private sealed class Item { public int Id { get; set; } public string? Name { get; set; } }

    private static IAsyncPagedEnumerable<Item> CreatePaged(params Item[] items) =>
        new AsyncPagedEnumerable<Item>(items.ToAsyncEnumerable(), _ => new(Pagination.Create(items.Length, 1, totalCount: items.Length)));

    private static JsonSerializerOptions CreateOptions(bool camelCase = false)
    {
        var opts = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null
        };
        // Ensure resolver present before MakeReadOnly in formatter
        opts.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();
        return opts;
    }

    [Fact]
    public void CanWriteType_ReturnsTrueForIAsyncPagedEnumerable()
    {
        var formatter = new AsyncPagedEnumerableJsonOutputFormatter(CreateOptions());
        var http = new DefaultHttpContext();
        var paged = CreatePaged([]);
        var context = new OutputFormatterWriteContext(
            http,
            (s, e) => new StreamWriter(s, e),
            typeof(IAsyncPagedEnumerable<Item>),
            paged)
        {
            ContentType = "application/json"
        };

        formatter.CanWriteResult(context).Should().BeTrue();
    }

    [Fact]
    public async Task WriteResponseBodyAsync_WritesJson()
    {
        var formatter = new AsyncPagedEnumerableJsonOutputFormatter(CreateOptions(camelCase: true));
        var items = Enumerable.Range(1, 3).Select(i => new Item { Id = i, Name = $"N{i}" }).ToArray();
        var paged = CreatePaged(items);

        var httpContext = new DefaultHttpContext();
        var ms = new MemoryStream();
        httpContext.Response.Body = ms;

        var context = new OutputFormatterWriteContext(
            httpContext,
            (s, e) => new StreamWriter(s, e),
            paged.GetType(),
            paged)
        {
            ContentType = "application/json"
        };

        await formatter.WriteAsync(context);
        await httpContext.Response.BodyWriter.FlushAsync();

        ms.Position = 0;
        using var doc = await JsonDocument.ParseAsync(ms);
        var root = doc.RootElement;
        root.TryGetProperty("pagination", out var pagination).Should().BeTrue();
        root.GetProperty("items").GetArrayLength().Should().Be(3);
    }
}
