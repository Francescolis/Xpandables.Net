
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Rests;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

public sealed class WithdrawAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/withdraw",
            async (
                [FromBody] WithdrawAccountRequest request,
                IMediator dispatcher,
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
        .WithXMinimalApi()
        .AllowAnonymous()
        .Accepts<WithdrawAccountRequest>(Rest.ContentType.Json)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
}