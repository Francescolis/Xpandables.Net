/*******************************************************************************
* Copyright (C) 2025 Francis-Black EWANE
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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Rests;

namespace Xpandables.Net.UnitTests.Rests;

public partial class RestClientIntegrationTests
{
    [Fact]
    public async Task SendAsync_WithTypedResult_ShouldDeserializeAndReturnSuccess()
    {
        // Arrange - fake HTTP handler
        var handler = new FakeMessageHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new TestModel(7, "ok"), RestJsonContext.Default.TestModel);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, RestSettings.ContentType.Json)
            };
            return response;
        });

        // DI container with request/response builders and composers
        var services = new ServiceCollection();
        services.AddXRestAttributeProvider();
        services.AddXRestRequestBuilder();
        services.AddXRestResponseBuilder();
        services.AddXRestRequestComposers();
        services.AddXRestResponseComposers();
        services.AddXRestClient((sp, client) =>
        {
            client.BaseAddress = new Uri("https://example.test");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => handler);

        var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IRestClient>();

        var request = new GetItemRequest(7);
        RestSettings.SerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = RestJsonContext.Default
        };

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccess.Should().BeTrue();
        var typed = (RestResponse<TestModel>)response;
        typed.Result.Should().NotBeNull();
        typed.Result!.Id.Should().Be(7);
        typed.Result.Name.Should().Be("ok");
    }

    private sealed class FakeMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    [RestGet("/items")]
    private sealed class GetItemRequest(int id) : IRestRequest<TestModel>, IRestQueryString
    {
        public IDictionary<string, string?>? GetQueryString() => new Dictionary<string, string?>
        {
            ["id"] = id.ToString()
        };
    }

    private sealed record TestModel(int Id, string Name);

    [JsonSerializable(typeof(TestModel))]
    private sealed partial class RestJsonContext : JsonSerializerContext { }
}
