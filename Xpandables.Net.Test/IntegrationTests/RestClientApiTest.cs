using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;
using Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;
using Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Rests;
using Xpandables.Net.Repositories;
using Xpandables.Net.Test.UnitTests;

namespace Xpandables.Net.Test.IntegrationTests;

[TestCaseOrderer("Xpandables.Net.Test.PriorityOrderer", "Xpandables.Net.Test")]
public sealed class RestClientApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IRestClient _restClient;
    private readonly Guid _keyId = Guid.NewGuid();
    public RestClientApiTest(WebApplicationFactory<Program> factory)
    {
        var services = new ServiceCollection();
        factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.Replace(new ServiceDescriptor(
                    typeof(IEventStore), typeof(InMemoryEventStore), ServiceLifetime.Scoped))));
        services.AddXRestAttributeProvider();
        services.AddXRestRequestBuilders();
        services.AddXRestResponseBuilders();
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
        // upload picture
        var uploadPictureRequest = new UploadPictureRequest();
        var uploadPictureResponse = await _restClient.SendAsync(uploadPictureRequest);
        uploadPictureResponse.IsSuccess.Should().BeTrue();

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

[RestPut("/accounts/picture",
    IsSecured = false,
    ContentType = Rest.ContentType.MultipartFormData,
    Location = Rest.Location.Body,
    BodyFormat = Rest.BodyFormat.Multipart)]
public sealed record UploadPictureRequest : IRestRequest, IRestMultipart
{
    public MultipartFormDataContent GetMultipartContent()
    {
        var multipartContent = new MultipartFormDataContent("boundary");
        var fileContent = new StreamContent(File.OpenRead(@"C:\\Users\\fewan\\OneDrive\\Documents\\Administratif\\Signature.png"))
        {
            Headers =
            {
                ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Rest.ContentType.Png)
            }
        }
        ;
        multipartContent.Add(fileContent, "formFile", "Signature.png");
        return multipartContent;
    }
}