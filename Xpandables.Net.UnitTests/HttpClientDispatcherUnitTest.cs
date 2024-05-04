
/*******************************************************************************
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
********************************************************************************/
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Builders;

using static Xpandables.Net.Http.HttpClientParameters.Patch;

namespace Xpandables.Net.UnitTests;
public sealed class HttpClientDispatcherUnitTest
{
    private readonly IHttpMonkeyDispatcher _dispatcher;
    private readonly IHttpClientDispatcherFactory _httpClientDispatcherFactory;
    public HttpClientDispatcherUnitTest()
    {
        ServiceCollection services = new();

        services
            .Configure<HttpClientOptions>(HttpClientOptions.Default)
            .AddXHttpClientDispatcherBuilders()
            //.AddXHttpClientOptions()
            .AddXHttpClientDispatcherFactory()
            .AddXHttpClientDispatcher<IHttpMonkeyDispatcher, HttpMonkeyDispatcher>(
            (_, httpClient) =>
            {
                httpClient.BaseAddress = new Uri("https://www.montemagno.com/");
                httpClient.Timeout = new TimeSpan(0, 5, 0);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new System.Net.Http.Headers
                    .MediaTypeWithQualityHeaderValue(
                        HttpClientParameters.ContentType.Json));
            });

        ServiceProvider provider = services.BuildServiceProvider();

        _dispatcher = provider.GetRequiredService<IHttpMonkeyDispatcher>();
        _httpClientDispatcherFactory = provider
            .GetRequiredService<IHttpClientDispatcherFactory>();
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
            PatchOperationsBuilder = req =>
            [
                Add(nameof(req.Name), req.Name),
                Add(nameof(req.BirthDate), req.BirthDate),
                Replace(nameof(req.Age), req.Age)
            ]
        };


        string expectedResult = JsonSerializer.Serialize(
            request.PatchOperations,
        _httpClientDispatcherFactory.Options.SerializerOptions); ;

        HttpRequestMessage message = await _httpClientDispatcherFactory
            .BuildRequestAsync(
            request, CancellationToken.None);

        Assert.NotNull(message.Content);

        string content = await message.Content.ReadAsStringAsync();

        content.Should().BeEquivalentTo(expectedResult);
    }

}

[HttpClient(Path = "/api/call",
    IsNullable = false, IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    ContentType = HttpClientParameters.ContentType.JsonPatch,
    Method = HttpClientParameters.Method.PATCH)]
sealed record Request(string Name, DateTime BirthDate, int Age) :
    HttpRequestPatch<Request>, IHttpClientRequest;
readonly record struct CountryRequest(string? Name = default);

readonly record struct Monkey(
    string Name, string Location, string Details, string Image,
    int Population, double Latitude, double Longitude);

[HttpClient(Path = "monkeys.json", IsSecured = true, IsNullable = true,
    Method = HttpClientParameters.Method.GET,
    Location = HttpClientParameters.Location.Body)]
sealed record Query : IHttpClientAsyncRequest<Monkey>;
interface IHttpMonkeyDispatcher : IHttpClientDispatcher
{
    IAsyncEnumerable<Monkey> GetMonkeyAsync();
}

sealed class HttpMonkeyDispatcher(
    HttpClient httpClient,
    IHttpClientDispatcherFactory dispatcherFactory)
    : HttpClientDispatcher(
        httpClient,
        dispatcherFactory), IHttpMonkeyDispatcher
{
    public async IAsyncEnumerable<Monkey> GetMonkeyAsync()
    {
        Query query = new();
        using HttpClientResponse<IAsyncEnumerable<Monkey>> response
            = await SendAsync(query).ConfigureAwait(false);

        await foreach (Monkey monkey in response.Result)
            yield return monkey;
    }
}
