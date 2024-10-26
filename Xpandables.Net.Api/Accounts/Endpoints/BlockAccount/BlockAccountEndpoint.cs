
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Http;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed class BlockAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/block",
            async (
                [FromBody] BlockAccountRequest request,
                IDispatcher dispatcher,
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
        .WithXOperationResultMinimalApi()
        .AllowAnonymous()
        .Accepts<BlockAccountRequest>(HttpClientParameters.ContentType.Json)
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status401Unauthorized);
}