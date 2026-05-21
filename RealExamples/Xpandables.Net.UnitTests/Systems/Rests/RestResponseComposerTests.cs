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

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Rests.Abstractions;
using System.Rests.ResponseBuilders;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Rests;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class RestResponseComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public sealed class ResultComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_WithValidPayload_ReturnsTypedResult()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public sealed class ContentComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_TextContent_ReturnsString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_BinaryContent_ReturnsStream()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
			using var stream = (Stream)response.Result!;
			byte[] result = await ReadAllBytesAsync(stream);
			result.Should().Equal(payload);
		}
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public sealed class NoContentComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_NoContent_ReturnsEmptyResponse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
		{
			using HttpResponseMessage message = new(HttpStatusCode.NoContent);
			RestResponseContext context = CreateContext(new BasicRequest(), message);
			RestResponseNoContentComposer composer = new();

			RestResponse response = await composer.ComposeAsync(context);

			response.StatusCode.Should().Be(HttpStatusCode.NoContent);
			response.Result.Should().BeNull();
		}

		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public void CanCompose_WithBody_ReturnsFalse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public sealed class FailureComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_FailureResponse_AttachesException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public sealed class StreamComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_StreamRequest_ReturnsAsyncEnumerable()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
			var stream = (IAsyncEnumerable<WidgetDto>)response.Result!;
			List<WidgetDto> items = await ToListAsync(stream);
			items.Should().HaveCount(2);
			items.Should().ContainEquivalentOf(new WidgetDto(1, "One"));
			items.Should().ContainEquivalentOf(new WidgetDto(2, "Two"));
		}
	}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public sealed class StreamPagedComposerTests
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	{
		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_PagedStream_ReturnsPagedEnumerable()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
			var paged = (IAsyncPagedEnumerable<WidgetDto>)response.Result!;

			List<WidgetDto> items = await ToListAsync((IAsyncEnumerable<WidgetDto>)paged);
			items.Should().HaveCount(2);
			Pagination pagination = await paged.GetPaginationAsync();
			pagination.CurrentPage.Should().BeGreaterThanOrEqualTo(0);
		}

		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public async Task ComposeAsync_WithDeserializer_UsesDeserializerPath()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
		{
			string json = """
{
    "items": [
        { "id": 10, "name": "Alpha" },
        { "id": 20, "name": "Beta" }
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

			var request = new StreamPagedDeserializerRequest<WidgetDto>();
			RestResponseContext context = CreateContext(request, message);
			RestResponseStreamPagedComposer composer = new();

			RestResponse response = await composer.ComposeAsync(context);

			response.Result.Should().NotBeNull();
			response.Result.Should().BeAssignableTo<IAsyncPagedEnumerable<WidgetDto>>();
			request.DeserializerWasInvoked.Should().BeTrue();
		}

		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public void CanCompose_WithNonPagedStreamRequest_ReturnsFalse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
		{
			using HttpResponseMessage message = new(HttpStatusCode.OK)
			{
				Content = new StringContent("[]", Encoding.UTF8, RestSettings.ContentType.Json)
			};

			RestResponseContext context = CreateContext(new StreamRequest<WidgetDto>(), message);
			RestResponseStreamPagedComposer composer = new();

			composer.CanCompose(context).Should().BeFalse();
		}

		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public void CanCompose_WithFailedResponse_ReturnsFalse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
		{
			using HttpResponseMessage message = new(HttpStatusCode.BadRequest)
			{
				Content = new StringContent("{}", Encoding.UTF8, RestSettings.ContentType.Json)
			};

			RestResponseContext context = CreateContext(new StreamPagedRequest<WidgetDto>(), message);
			RestResponseStreamPagedComposer composer = new();

			composer.CanCompose(context).Should().BeFalse();
		}

		[Fact]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public void CanCompose_WithPagedStreamSuccessRequest_ReturnsTrue()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
		{
			using HttpResponseMessage message = new(HttpStatusCode.OK)
			{
				Content = new StringContent("{}", Encoding.UTF8, RestSettings.ContentType.Json)
			};

			RestResponseContext context = CreateContext(new StreamPagedRequest<WidgetDto>(), message);
			RestResponseStreamPagedComposer composer = new();

			composer.CanCompose(context).Should().BeTrue();
		}
	}

	private static RestResponseContext CreateContext(IRestRequest request, HttpResponseMessage message, JsonSerializerOptions? options = null) => new()
	{
		Request = request,
		Message = message,
		SerializerOptions = options ?? CreateSerializerOptions(),
		IsAborted = false
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

	private sealed record StreamPagedDeserializerRequest<T> : IRestRequestStreamPaged<T>, IRestStreamPagedDeserializer
		where T : notnull
	{
		public Type ResultType => typeof(T);
		public bool DeserializerWasInvoked { get; private set; }

#pragma warning disable IL2026
#pragma warning disable IL3050
		public object DeserializeAsAsyncPagedEnumerable(
			HttpContent content,
			JsonSerializerOptions options,
			CancellationToken cancellationToken)
		{
			DeserializerWasInvoked = true;
			JsonTypeInfo<T> typeInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
			return content.ReadFromJsonAsAsyncPagedEnumerable<T>(
				typeInfo, PaginationStrategy.None, cancellationToken);
		}
#pragma warning restore IL3050
#pragma warning restore IL2026
	}

	private sealed record BasicRequest : IRestRequest;
}
