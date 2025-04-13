
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Rests;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed class DepositAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/deposit",
            async (
                [FromBody] DepositAccountRequest request,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                DepositAccountCommand command = new()
                {
                    DependencyKeyId = request.KeyId,
                    Amount = request.Amount
                };

                return await dispatcher
                    .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
        .WithTags("Accounts")
        .WithName("DepositAccount")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Accepts<DepositAccountRequest>(Rest.ContentType.Json)
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status401Unauthorized);
}