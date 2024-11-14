
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed class WithdrawAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/withdraw",
            async (
                [FromBody] WithdrawAccountRequest request,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                WithdrawAccountCommand command = new()
                {
                    KeyId = request.KeyId,
                    Amount = request.Amount
                };

                return await dispatcher
                    .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
        .WithTags("Accounts")
        .WithName("WithdrawAccount")
        .WithXOperationResultMinimalApi()
        .AllowAnonymous()
        .Accepts<WithdrawAccountRequest>(HttpClientParameters.ContentType.Json)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
}