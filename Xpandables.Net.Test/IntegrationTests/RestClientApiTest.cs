using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;
using Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;
using Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;

namespace Xpandables.Net.Test.IntegrationTests;

[TestCaseOrderer("Xpandables.Net.Test.PriorityOrderer", "Xpandables.Net.Test")]
public sealed class RestClientApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IRestClient _restClient;
    private readonly Guid _keyId = Guid.NewGuid();
    public RestClientApiTest(WebApplicationFactory<Program> factory)
    {
        var services = new ServiceCollection();

        services.Configure<RestOptions>(RestOptions.Default);
        services.AddXRestOptions();
        services.AddXRestRequestHandler();
        services.AddXRestResponseHandler();
        services.AddSingleton(factory.CreateClient());
        services.AddScoped<IRestClient>(provider =>
        {
            HttpClient httpClient = provider.GetRequiredService<HttpClient>();
            return new RestClient(provider, httpClient);
        });

        var serviceProvider = services.BuildServiceProvider();
        _restClient = serviceProvider.GetRequiredService<IRestClient>();
    }

    [Fact, TestPriority(0)]
    public async Task CreateAccount_Should_Return_Valid_Result()
    {
        // Create       
        var create = new CreateAccountRequest { KeyId = _keyId };

        var createResponse = await _restClient.SendAsync(create);

        createResponse.IsSuccess.Should().BeTrue();

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Deposit
        var deposit = new DepositAccountRequest { KeyId = _keyId, Amount = 100 };

        var depositResponse = await _restClient.SendAsync(deposit);

        depositResponse.IsSuccess.Should().BeTrue();

        depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Balance
        var balance = new GetBalanceAccountRequest { KeyId = _keyId };

        var balanceResponse = await _restClient.SendAsync(balance);

        balanceResponse.IsSuccess.Should().BeTrue();

        balanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        balanceResponse.Result.Should().Be(100);
    }
}
