
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;

namespace Xpandables.Net.UnitTests;
public sealed class HttpClientDispatcherUnitTest
{
    private readonly IHttpMonkeyDispatcher _dispatcher;
    private readonly IHttpClientRequestBuilder _httpClientRequestBuilder;
    public HttpClientDispatcherUnitTest()
    {
        ServiceCollection services = new();
        services
            .AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web))
            .AddXHttpClientRequestBuilder()
            .AddXHttpClientResponseBuilder()
            .AddXHttpClientBuildProvider()
            .AddXHttpClientDispatcher<IHttpMonkeyDispatcher, HttpMonkeyDispatcher>(_ => "token", (_, httpClient) =>
            {
                httpClient.BaseAddress = new Uri("https://www.montemagno.com/");
                httpClient.Timeout = new TimeSpan(0, 5, 0);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(HttpClientParameters.ContentType.Json));
            });

        ServiceProvider provider = services.BuildServiceProvider();

        _dispatcher = provider.GetRequiredService<IHttpMonkeyDispatcher>();
        _httpClientRequestBuilder = provider.GetRequiredService<IHttpClientRequestBuilder>();
    }

    [Fact]
    public async Task BuildCustomHttpClientDispatcherAndReturnsAsyncEnumerable()
    {
        List<Monkey> monkeys = [];

        await foreach (Monkey monkey in _dispatcher.GetMonkeyAsync())
            monkeys.Add(monkey);

        monkeys.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AssertPatchWorks()
    {
        Request request = new("MyName", DateTime.UtcNow, 32)
        {
            PatchOperationsBuilder = req => new List<IPatchOperation>
            {
                HttpClientParameters.Patch.Add(nameof(req.Name), req.Name),
                HttpClientParameters.Patch.Add(nameof(req.BirthDate), req.BirthDate),
                HttpClientParameters.Patch.Replace(nameof(req.Age), req.Age)
            }
        };


        string expectedResult = JsonSerializer.Serialize(request.PatchOperations);

        HttpRequestMessage message = await _httpClientRequestBuilder
            .BuildHttpRequestAsync(request, _dispatcher.HttpClient, new(JsonSerializerDefaults.Web));

        Assert.NotNull(message.Content);

        string content = await message.Content.ReadAsStringAsync();

        content.Should().BeEquivalentTo(expectedResult);
    }

}

[HttpClient(Path = "/api/call",
    IsNullable = false, IsSecured = false,
    Location = HttpClientParameters.Location.Body, ContentType = HttpClientParameters.ContentType.JsonPatch,
    Method = HttpClientParameters.Method.PATCH)]
sealed record Request(string Name, DateTime BirthDate, int Age) : HttpRequestPatch<Request>, IHttpClientRequest;
readonly record struct CountryRequest(string? Name = default);

readonly record struct Monkey(string Name, string Location, string Details, string Image, int Population, double Latitude, double Longitude);

[HttpClient(Path = "monkeys.json", IsSecured = false, IsNullable = true,
    Method = HttpClientParameters.Method.GET, Location = HttpClientParameters.Location.Body)]
sealed record Query : IHttpClientAsyncRequest<Monkey>;
interface IHttpMonkeyDispatcher : IHttpClientDispatcher
{
    IAsyncEnumerable<Monkey> GetMonkeyAsync();
}

sealed class HttpMonkeyDispatcher(
    IHttpClientBuildProvider httpClientBuildProvider,
    HttpClient httpClient,
    JsonSerializerOptions? jsonSerializerOptions)
    : HttpClientDispatcher(
        httpClientBuildProvider,
        httpClient,
        jsonSerializerOptions), IHttpMonkeyDispatcher
{
    public async IAsyncEnumerable<Monkey> GetMonkeyAsync()
    {
        Query query = new();
        using HttpClientResponse<IAsyncEnumerable<Monkey>> response = await SendAsync(query).ConfigureAwait(false);

        await foreach (Monkey monkey in response.Result)
            yield return monkey;
    }
}
