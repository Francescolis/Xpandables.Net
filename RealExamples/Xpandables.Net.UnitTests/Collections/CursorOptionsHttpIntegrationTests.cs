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
using System.Net;
using System.Rests.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Collections;

/// <summary>
/// Integration tests that validate <see cref="CursorOptions{TSource}"/> usage
/// alongside <see cref="IRestClient"/> with a fake <see cref="HttpMessageHandler"/>.
/// These tests simulate cursor-based pagination over HTTP responses.
/// </summary>
public sealed class CursorOptionsHttpIntegrationTests : IDisposable
{
	private readonly IDisposable _serializerScope;

	public CursorOptionsHttpIntegrationTests()
	{
		_serializerScope = UseDefaultSerializerOptions();
	}

	public void Dispose() => _serializerScope.Dispose();

	[Fact]
	public async Task SendAsync_WithCursorInQueryString_ReturnsPaginatedResponse()
	{
		// Arrange
		var cursorOptions = CursorOptions.Create<ProductDto, int>(x => x.Id);

		var handler = new FakeProductHandler(pageSize: 3, totalProducts: 9);
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		var request = new GetProductsRequest(
			PageSize: 3,
			Cursor: cursorOptions.FormatToken(null));

		// Act
		var response = await client.SendAsync(request);
		var typed = response.ToRestResponse<PagedResult<ProductDto>>();

		// Assert
		typed.StatusCode.Should().Be(HttpStatusCode.OK);
		typed.IsSuccess.Should().BeTrue();
		typed.Result.Should().NotBeNull();
		typed.Result!.Items.Should().HaveCount(3);
		typed.Result.NextCursor.Should().NotBeNull();
	}

	[Fact]
	public async Task SendAsync_MultiplePages_CursorAdvancesCorrectly()
	{
		// Arrange
		var cursorOptions = CursorOptions.Create<ProductDto, int>(x => x.Id);
		var handler = new FakeProductHandler(pageSize: 3, totalProducts: 7);
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		var allItems = new List<ProductDto>();
		string? cursor = null;

		// Act — paginate through all pages
		do
		{
			var request = new GetProductsRequest(PageSize: 3, Cursor: cursor);
			var response = await client.SendAsync(request);
			var typed = response.ToRestResponse<PagedResult<ProductDto>>();

			typed.IsSuccess.Should().BeTrue();
			typed.Result.Should().NotBeNull();

			allItems.AddRange(typed.Result!.Items);
			cursor = typed.Result.NextCursor;
		}
		while (cursor is not null);

		// Assert
		allItems.Should().HaveCount(7);
		allItems.Select(p => p.Id).Should().BeInAscendingOrder();
		allItems.Select(p => p.Id).Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public async Task SendAsync_CursorRoundtrip_ParsedTokenMatchesFormattedToken()
	{
		// Arrange
		var cursorOptions = CursorOptions.Create<ProductDto, int>(x => x.Id);
		var handler = new FakeProductHandler(pageSize: 2, totalProducts: 5);
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		var firstRequest = new GetProductsRequest(PageSize: 2, Cursor: null);
		var firstResponse = await client.SendAsync(firstRequest);
		var firstTyped = firstResponse.ToRestResponse<PagedResult<ProductDto>>();

		firstTyped.IsSuccess.Should().BeTrue();
		var nextCursorToken = firstTyped.Result!.NextCursor;

		// Act — parse cursor from response and format it back
		var parsedCursor = cursorOptions.ParseToken(nextCursorToken);
		var reformatted = cursorOptions.FormatToken(parsedCursor);

		// Assert — roundtrip produces the same token
		reformatted.Should().Be(nextCursorToken);
		parsedCursor.Should().Be(2); // last Id on page 1 is 2
	}

	[Fact]
	public async Task SendAsync_WithGuidCursor_PaginatesThroughAllPages()
	{
		// Arrange
		var cursorOptions = CursorOptions.Create<OrderDto, Guid>(
			x => x.OrderId,
			formatter: g => g.ToString(),
			parser: s => s is null ? Guid.Empty : Guid.Parse(s));

		var orders = Enumerable.Range(1, 6)
			.Select(i => new OrderDto(Guid.NewGuid(), $"Order-{i}"))
			.OrderBy(o => o.OrderId)
			.ToList();

		var handler = new FakeOrderHandler(orders, pageSize: 2);
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		var allItems = new List<OrderDto>();
		string? cursor = null;

		// Act
		do
		{
			var request = new GetOrdersRequest(PageSize: 2, Cursor: cursor);
			var response = await client.SendAsync(request);
			var typed = response.ToRestResponse<PagedResult<OrderDto>>();

			typed.IsSuccess.Should().BeTrue();
			typed.Result.Should().NotBeNull();

			allItems.AddRange(typed.Result!.Items);

			if (typed.Result.NextCursor is not null)
			{
				var parsedToken = cursorOptions.ParseToken(typed.Result.NextCursor);
				var reformatted = cursorOptions.FormatToken(parsedToken);
				reformatted.Should().Be(typed.Result.NextCursor);
			}

			cursor = typed.Result.NextCursor;
		}
		while (cursor is not null);

		// Assert
		allItems.Should().HaveCount(6);
		allItems.Select(o => o.OrderId).Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public async Task SendAsync_WithCustomFormatterAndParser_CursorWorksAcrossPages()
	{
		// Arrange — base64-encoded cursor
		var cursorOptions = CursorOptions.Create<ProductDto, int>(
			x => x.Id,
			formatter: v => Convert.ToBase64String(Encoding.UTF8.GetBytes(v.ToString()!)),
			parser: s => int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(s!))));

		var handler = new FakeProductHandler(pageSize: 3, totalProducts: 6, useBase64Cursor: true);
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		// Act — first page
		var request = new GetProductsRequest(PageSize: 3, Cursor: null);
		var response = await client.SendAsync(request);
		var typed = response.ToRestResponse<PagedResult<ProductDto>>();

		typed.IsSuccess.Should().BeTrue();
		typed.Result.Should().NotBeNull();
		typed.Result!.Items.Should().HaveCount(3);

		var nextCursor = typed.Result.NextCursor!;
		var parsedId = cursorOptions.ParseToken(nextCursor);
		parsedId.Should().Be(3);

		// Act — second page
		var request2 = new GetProductsRequest(PageSize: 3, Cursor: nextCursor);
		var response2 = await client.SendAsync(request2);
		var typed2 = response2.ToRestResponse<PagedResult<ProductDto>>();

		typed2.IsSuccess.Should().BeTrue();
		typed2.Result!.Items.Should().HaveCount(3);
		typed2.Result.NextCursor.Should().BeNull();
	}

	[Fact]
	public async Task SendAsync_WithBackwardCursor_OptionsConfiguredCorrectly()
	{
		// Arrange
		var cursorOptions = CursorOptions.Create<ProductDto, int>(
			x => x.Id,
			CursorDirection.Backward,
			isInclusive: true);

		cursorOptions.Direction.Should().Be(CursorDirection.Backward);
		cursorOptions.IsInclusive.Should().BeTrue();

		var handler = new FakeProductHandler(pageSize: 5, totalProducts: 5);
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		// Act
		var request = new GetProductsRequest(PageSize: 5, Cursor: null);
		var response = await client.SendAsync(request);
		var typed = response.ToRestResponse<PagedResult<ProductDto>>();

		// Assert
		typed.IsSuccess.Should().BeTrue();
		typed.Result!.Items.Should().HaveCount(5);
		typed.Result.NextCursor.Should().BeNull();
	}

	[Fact]
	public async Task SendAsync_WhenApiFails_CursorOptionsUnaffected()
	{
		// Arrange
		var cursorOptions = CursorOptions.Create<ProductDto, int>(x => x.Id);
		var handler = new FakeErrorHandler();
		var (provider, client) = BuildRestClient(handler);
		using var scope = provider as IDisposable;

		// Act
		var request = new GetProductsRequest(PageSize: 10, Cursor: null);
		var response = await client.SendAsync(request);

		// Assert — API failure doesn't break cursor options
		response.IsFailure.Should().BeTrue();
		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		cursorOptions.FormatToken(42).Should().Be("42");
		cursorOptions.ParseToken("42").Should().Be(42);
	}

	// ──────────────────────────────────────────────────
	//  Infrastructure: DI, serializer scope, builders
	// ──────────────────────────────────────────────────

	private static (IServiceProvider Provider, IRestClient Client) BuildRestClient(
		HttpMessageHandler handler)
	{
		ServiceCollection services = new();
		services.AddXRestAttributeProvider();
		services.AddXRestRequestComposers();
		services.AddXRestResponseComposers();
		services.AddXRestRequestBuilder();
		services.AddXRestResponseBuilder();
		services.AddSingleton(handler);
		services.AddXRestClient((_, client) =>
		{
			client.BaseAddress = new Uri("https://api.test.local");
			client.Timeout = TimeSpan.FromSeconds(5);
		})
			.ConfigurePrimaryHttpMessageHandler(sp =>
				sp.GetRequiredService<HttpMessageHandler>());

		var provider = services.BuildServiceProvider();
		var client = provider.GetRequiredService<IRestClient>();
		return (provider, client);
	}

	private static IDisposable UseDefaultSerializerOptions()
	{
		var previous = RestSettings.SerializerOptions;
		RestSettings.SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
		{
			TypeInfoResolver = new DefaultJsonTypeInfoResolver()
		};
		return new SerializerScope(previous);
	}

	private sealed class SerializerScope(JsonSerializerOptions previous) : IDisposable
	{
		public void Dispose() => RestSettings.SerializerOptions = previous;
	}

	// ──────────────────────────────────────────────────
	//  DTOs
	// ──────────────────────────────────────────────────

	private sealed record ProductDto(int Id, string Name);

	private sealed record OrderDto(Guid OrderId, string Description);

	private sealed record PagedResult<T>(List<T> Items, string? NextCursor);

	// ──────────────────────────────────────────────────
	//  Request types
	// ──────────────────────────────────────────────────

	private sealed record GetProductsRequest(int PageSize, string? Cursor)
		: IRestRequestResult<PagedResult<ProductDto>>, IRestQueryString, IRestAttributeBuilder
	{
		public IDictionary<string, string?>? GetQueryString() => new Dictionary<string, string?>
		{
			["pageSize"] = PageSize.ToString(),
			["cursor"] = Cursor
		};

		public RestAttribute Build(IServiceProvider serviceProvider) => new RestGetAttribute("/products")
		{
			Location = RestSettings.Location.Query
		};
	}

	private sealed record GetOrdersRequest(int PageSize, string? Cursor)
		: IRestRequestResult<PagedResult<OrderDto>>, IRestQueryString, IRestAttributeBuilder
	{
		public IDictionary<string, string?>? GetQueryString() => new Dictionary<string, string?>
		{
			["pageSize"] = PageSize.ToString(),
			["cursor"] = Cursor
		};

		public RestAttribute Build(IServiceProvider serviceProvider) => new RestGetAttribute("/orders")
		{
			Location = RestSettings.Location.Query
		};
	}

	// ──────────────────────────────────────────────────
	//  Fake handlers
	// ──────────────────────────────────────────────────

	private sealed class FakeProductHandler(int pageSize, int totalProducts, bool useBase64Cursor = false)
		: HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var uri = request.RequestUri!;
			var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
			var cursorStr = query["cursor"];

			int startAfter = 0;
			if (!string.IsNullOrEmpty(cursorStr))
			{
				startAfter = useBase64Cursor
					? int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(cursorStr)))
					: int.Parse(cursorStr);
			}

			var items = Enumerable.Range(1, totalProducts)
				.Where(id => id > startAfter)
				.Take(pageSize)
				.Select(id => new ProductDto(id, $"Product-{id}"))
				.ToList();

			var lastId = items.Count > 0 ? items[^1].Id : (int?)null;
			var hasMore = lastId.HasValue && lastId.Value < totalProducts;

			string? nextCursor = null;
			if (hasMore)
			{
				nextCursor = useBase64Cursor
					? Convert.ToBase64String(Encoding.UTF8.GetBytes(lastId!.Value.ToString()))
					: lastId!.Value.ToString();
			}

			var result = new PagedResult<ProductDto>(items, nextCursor);
			var json = JsonSerializer.Serialize(result, RestSettings.SerializerOptions);

			var response = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(json, Encoding.UTF8, RestSettings.ContentType.Json),
				Version = HttpVersion.Version20
			};

			return Task.FromResult(response);
		}
	}

	private sealed class FakeOrderHandler(List<OrderDto> orders, int pageSize) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var uri = request.RequestUri!;
			var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
			var cursorStr = query["cursor"];

			IEnumerable<OrderDto> filtered = orders;
			if (!string.IsNullOrEmpty(cursorStr))
			{
				var cursorGuid = Guid.Parse(cursorStr);
				filtered = orders.Where(o => o.OrderId.CompareTo(cursorGuid) > 0);
			}

			var items = filtered.Take(pageSize).ToList();
			var lastId = items.Count > 0 ? items[^1].OrderId : (Guid?)null;
			var hasMore = lastId.HasValue && orders.Any(o => o.OrderId.CompareTo(lastId.Value) > 0);

			var nextCursor = hasMore ? lastId!.Value.ToString() : null;
			var result = new PagedResult<OrderDto>(items, nextCursor);
			var json = JsonSerializer.Serialize(result, RestSettings.SerializerOptions);

			var response = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(json, Encoding.UTF8, RestSettings.ContentType.Json),
				Version = HttpVersion.Version20
			};

			return Task.FromResult(response);
		}
	}

	private sealed class FakeErrorHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
			{
				Content = new StringContent("""{"error":"Something went wrong"}""",
					Encoding.UTF8, RestSettings.ContentType.Json),
				Version = HttpVersion.Version20
			};

			return Task.FromResult(response);
		}
	}
}
