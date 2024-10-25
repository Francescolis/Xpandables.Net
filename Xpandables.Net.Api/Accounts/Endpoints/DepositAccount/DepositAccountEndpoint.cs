
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed class DepositAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/accounts/deposit", async (
            [FromBody] DepositAccountRequest request,
            IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            DepositAccountCommand command = new DepositAccountCommand
            {
                KeyId = request.KeyId,
                Amount = request.Amount
            };

            return await dispatcher
                .SendAsync(command, cancellationToken)
                .ConfigureAwait(false);
        })
        .WithTags("Accounts")
        .WithName("DepositAccount")
        .WithXOperationResultMinimalApi()
        .AllowAnonymous()
        .Accepts<DepositAccountRequest>(HttpClientParameters.ContentType.Json)
        .Produces(StatusCodes.Status200OK);
    }
}