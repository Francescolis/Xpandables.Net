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

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestClientIntegrationTests
{
    [Fact]
    public async Task SendAsync_WithValidRequest_ComposesRequestAndParsesResponse()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        ServiceCollection services = new();
        services.AddXRestAttributeProvider();
        services.AddXRestRequestComposers();
        services.AddXRestResponseComposers();
        services.AddXRestRequestBuilder();
        services.AddXRestResponseBuilder();
        services.AddScoped<StubHandler>();
        services.AddXRestClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
            .ConfigurePrimaryHttpMessageHandler<StubHandler>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        IRestClient client = serviceProvider.GetRequiredService<IRestClient>();

        // Act
        RestResponse rawResponse = await client.SendAsync(new GetWidgetRequest("42"));
        var typedResponse = rawResponse.ToRestResponse<WidgetDto>();

        // Assert
        typedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        typedResponse.IsSuccess.Should().BeTrue();
        typedResponse.Result.Should().NotBeNull();
        typedResponse.Result!.Should().BeEquivalentTo(new WidgetDto("42", "Integration widget"));
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

    private sealed record GetWidgetRequest(string WidgetId)
        : IRestRequestResult<WidgetDto>, IRestPathString, IRestAttributeBuilder
    {
        public IDictionary<string, string> GetPathString() => new Dictionary<string, string>
        {
            ["widgetId"] = WidgetId
        };

        public RestAttribute Build(IServiceProvider serviceProvider) => new RestGetAttribute("/widgets/{widgetId}")
        {
            Location = RestSettings.Location.Path
        };
    }

    private sealed record WidgetDto(string Id, string Name);

    private sealed class StubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri.Should().NotBeNull();
            request.RequestUri!.AbsoluteUri.Should().Contain("/widgets/42");

            WidgetDto dto = new("42", "Integration widget");
            string json = JsonSerializer.Serialize(dto, RestSettings.SerializerOptions);

            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, RestSettings.ContentType.Json),
                Version = HttpVersion.Version20
            };

            return Task.FromResult(response);
        }
    }
}
