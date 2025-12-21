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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Rests;
using System.Rests.Abstractions;
using System.Rests.ResponseBuilders;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestResponseComposerTests
{
    public sealed class ResultComposerTests
    {
        [Fact]
        public async Task ComposeAsync_WithValidPayload_ReturnsTypedResult()
        {
            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                    "id": 42,
                    "name": "Widget"
                }
                """, Encoding.UTF8, RestSettings.ContentType.Json)
            };

            RestResponseContext context = CreateContext(new TypedRequest<WidgetDto>(), message);
            RestResponseResultComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Result.Should().BeOfType<WidgetDto>().Which.Should().BeEquivalentTo(new WidgetDto(42, "Widget"));
        }

        [Fact]
        public async Task ComposeAsync_WithInvalidPayload_ReturnsException()
        {
            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new StringContent("not json", Encoding.UTF8, RestSettings.ContentType.Json)
            };

            RestResponseContext context = CreateContext(new TypedRequest<WidgetDto>(), message);
            RestResponseResultComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Exception.Should().NotBeNull();
        }
    }

    public sealed class ContentComposerTests
    {
        [Fact]
        public async Task ComposeAsync_TextContent_ReturnsString()
        {
            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new StringContent("plain text", Encoding.UTF8, "text/plain")
            };

            RestResponseContext context = CreateContext(new BasicRequest(), message);
            RestResponseContentComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.Result.Should().BeOfType<string>().Which.Should().Be("plain text");
        }

        [Fact]
        public async Task ComposeAsync_BinaryContent_ReturnsStream()
        {
            byte[] payload = [1, 2, 3, 4];
            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            };
            message.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            RestResponseContext context = CreateContext(new BasicRequest(), message);
            RestResponseContentComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.Result.Should().BeAssignableTo<Stream>();
            using Stream stream = (Stream)response.Result!;
            byte[] result = await ReadAllBytesAsync(stream);
            result.Should().Equal(payload);
        }
    }

    public sealed class NoContentComposerTests
    {
        [Fact]
        public async Task ComposeAsync_NoContent_ReturnsEmptyResponse()
        {
            using HttpResponseMessage message = new(HttpStatusCode.NoContent);
            RestResponseContext context = CreateContext(new BasicRequest(), message);
            RestResponseNoContentComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Result.Should().BeNull();
        }

        [Fact]
        public void CanCompose_WithBody_ReturnsFalse()
        {
            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new StringContent("value", Encoding.UTF8, RestSettings.ContentType.Json)
            };

            RestResponseContext context = CreateContext(new BasicRequest(), message);
            RestResponseNoContentComposer composer = new();

            composer.CanCompose(context).Should().BeFalse();
        }
    }

    public sealed class FailureComposerTests
    {
        [Fact]
        public async Task ComposeAsync_FailureResponse_AttachesException()
        {
            using HttpResponseMessage message = new(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Bad request",
                Content = new StringContent("error", Encoding.UTF8, RestSettings.ContentType.Json)
            };

            RestResponseContext context = CreateContext(new BasicRequest(), message);
            RestResponseFailureComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Exception.Should().NotBeNull();
            response.Exception!.Message.Should().Contain("400");
        }
    }

    public sealed class StreamComposerTests
    {
        [Fact]
        public async Task ComposeAsync_StreamRequest_ReturnsAsyncEnumerable()
        {
            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                [
                    { "id": 1, "name": "One" },
                    { "id": 2, "name": "Two" }
                ]
                """, Encoding.UTF8, RestSettings.ContentType.Json)
            };

            RestResponseContext context = CreateContext(new StreamRequest<WidgetDto>(), message);
            RestResponseStreamComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.Result.Should().NotBeNull();
            response.Result.Should().BeAssignableTo<IAsyncEnumerable<WidgetDto>>();
            IAsyncEnumerable<WidgetDto> stream = (IAsyncEnumerable<WidgetDto>)response.Result!;
            List<WidgetDto> items = await ToListAsync(stream);
            items.Should().HaveCount(2);
            items.Should().ContainEquivalentOf(new WidgetDto(1, "One"));
            items.Should().ContainEquivalentOf(new WidgetDto(2, "Two"));
        }
    }

    public sealed class StreamPagedComposerTests
    {
        [Fact]
        public async Task ComposeAsync_PagedStream_ReturnsPagedEnumerable()
        {
            string json = """
{
    "items": [
        { "id": 1, "name": "One" },
        { "id": 2, "name": "Two" }
    ],
    "pagination": {
        "TotalCount": 2,
        "PageSize": 2,
        "CurrentPage": 1,
        "ContinuationToken": null
    }
}
""";

            using HttpResponseMessage message = new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, RestSettings.ContentType.Json)
            };

            RestResponseContext context = CreateContext(new StreamPagedRequest<WidgetDto>(), message);
            RestResponseStreamPagedComposer composer = new();

            RestResponse response = await composer.ComposeAsync(context);

            response.Result.Should().NotBeNull();
            response.Result.Should().BeAssignableTo<IAsyncPagedEnumerable<WidgetDto>>();
            IAsyncPagedEnumerable<WidgetDto> paged = (IAsyncPagedEnumerable<WidgetDto>)response.Result!;

            List<WidgetDto> items = await ToListAsync((IAsyncEnumerable<WidgetDto>)paged);
            items.Should().HaveCount(2);
            Pagination pagination = await paged.GetPaginationAsync();
            pagination.CurrentPage.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    private static RestResponseContext CreateContext(IRestRequest request, HttpResponseMessage message, JsonSerializerOptions? options = null) => new()
    {
        Request = request,
        Message = message,
        SerializerOptions = options ?? CreateSerializerOptions()
    };

    private static JsonSerializerOptions CreateSerializerOptions() => new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        List<T> results = [];
        await foreach (T item in source)
        {
            results.Add(item);
        }

        return results;
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        using MemoryStream buffer = new();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }

    private sealed record WidgetDto(int Id, string Name);

    private sealed record TypedRequest<T> : IRestRequestResult<T>
        where T : notnull
    {
        public Type ResultType => typeof(T);
    }

    private sealed record StreamRequest<T> : IRestRequestStream<T>
        where T : notnull
    {
        public Type ResultType => typeof(T);
    }

    private sealed record StreamPagedRequest<T> : IRestRequestStreamPaged<T>
        where T : notnull
    {
        public Type ResultType => typeof(T);
    }

    private sealed record BasicRequest : IRestRequest;
}
