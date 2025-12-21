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
using System.Net.Http.Headers;
using System.Rests;
using System.Rests.Abstractions;
using System.Rests.RequestBuilders;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestRequestBuilderTests
{
    [Fact]
    public async Task BuildRequestAsync_SecuredStringRequest_ComposesHttpMessage()
    {
        using var serializerScope = UseDefaultSerializerOptions();

        // Arrange
        IServiceProvider services = new ServiceCollection()
            .AddSingleton<IRestRequestComposer<PlaceOrderRequest>, RestPathStringComposer<PlaceOrderRequest>>()
            .AddSingleton<IRestRequestComposer<PlaceOrderRequest>, RestStringComposer<PlaceOrderRequest>>()
            .BuildServiceProvider();

        RestAttributeProvider attributeProvider = new(services);
        RestRequestBuilder builder = new(attributeProvider, services);

        PlaceOrderRequest request = new("A123", "Widget", 2);

        // Act
        RestRequest restRequest = await builder.BuildRequestAsync(request);

        // Assert
        using (restRequest)
        {
            HttpRequestMessage message = restRequest.HttpRequestMessage;

            message.Method.Should().Be(HttpMethod.Post);
            message.RequestUri.Should().NotBeNull();
            message.RequestUri!.ToString().Should().Be("/orders/A123");
            message.Headers.Authorization.Should().NotBeNull();
            message.Headers.Authorization!.Scheme.Should().Be("Bearer");
            message.Headers.Accept.Should().ContainSingle(h => h.MediaType == RestSettings.ContentType.Json);

            MediaTypeHeaderValue? contentType = message.Content?.Headers.ContentType;
            contentType.Should().NotBeNull();
            contentType!.MediaType.Should().Be(RestSettings.ContentType.Json);

            string payload = await message.Content!.ReadAsStringAsync();
            payload.Should().Contain("\"product\":\"Widget\"");
            payload.Should().Contain("\"quantity\":2");
        }
    }

    private static IDisposable UseDefaultSerializerOptions()
    {
        JsonSerializerOptions previous = RestSettings.SerializerOptions;
        RestSettings.SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        return new DelegateDisposable(() => RestSettings.SerializerOptions = previous);
    }

    private sealed class DelegateDisposable(Action dispose) : IDisposable
    {
        private readonly Action _dispose = dispose;
        public void Dispose() => _dispose();
    }

    private sealed record PlaceOrderRequest(string OrderId, string Product, int Quantity)
        : IRestRequest, IRestString, IRestPathString, IRestAttributeBuilder
    {
        public IDictionary<string, string> GetPathString() => new Dictionary<string, string>
        {
            ["orderId"] = OrderId
        };

        public RestAttribute Build(IServiceProvider serviceProvider) => new RestPostAttribute("/orders/{orderId}")
        {
            Location = RestSettings.Location.Body | RestSettings.Location.Path,
            BodyFormat = RestSettings.BodyFormat.String,
            ContentType = RestSettings.ContentType.Json,
            Accept = RestSettings.ContentType.Json
        };
    }
}
