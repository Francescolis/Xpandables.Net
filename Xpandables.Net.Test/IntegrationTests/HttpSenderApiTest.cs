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
public sealed class HttpSenderApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IHttpSender _sender;
    private readonly Guid _keyId = Guid.NewGuid();
    public HttpSenderApiTest(WebApplicationFactory<Program> factory)
    {
        var services = new ServiceCollection();

        services.Configure<MapHttpOptions>(MapHttpOptions.Default);
        services.AddXHttpRequestOptions();
        services.AddXHttpRequestFactory();
        services.AddXHttpResponseFactory();
        services.AddSingleton(factory.CreateClient());
        services.AddXHttpSender();

        var serviceProvider = services.BuildServiceProvider();
        _sender = serviceProvider.GetRequiredService<IHttpSender>();
    }

    [Fact, TestPriority(0)]
    public async Task CreateAccount_Should_Return_Valid_Result()
    {
        // Create       
        var create = new CreateAccountRequest { KeyId = _keyId };

        var createResponse = await _sender.SendAsync(create);

        createResponse.IsSuccessStatusCode.Should().BeTrue();

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Deposit
        var deposit = new DepositAccountRequest { KeyId = _keyId, Amount = 100 };

        var depositResponse = await _sender.SendAsync(deposit);

        depositResponse.IsSuccessStatusCode.Should().BeTrue();

        depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Balance
        var balance = new GetBalanceAccountRequest { KeyId = _keyId };

        var balanceResponse = await _sender.SendAsync(balance);

        balanceResponse.IsSuccessStatusCode.Should().BeTrue();

        balanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        balanceResponse.Result.Should().Be(100);
    }
}
