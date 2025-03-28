
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/block",
            async (
                [FromBody] BlockAccountRequest request,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                BlockAccountCommand command = new()
                {
                    KeyId = request.KeyId
                };

                return await dispatcher
                    .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
        .WithTags("Accounts")
        .WithName("BlockAccount")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Accepts<BlockAccountRequest>()
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status401Unauthorized);
}