
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed class UnBlockAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts/unblock",
            async (
                [FromBody] UnBlockAccountRequest request,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                UnBlockAccountCommand command = new()
                {
                    KeyId = request.KeyId
                };

                return await dispatcher
                    .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
        .WithTags("Accounts")
        .WithName("UnBlockAccount")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Accepts<UnBlockAccountRequest>(MapRest.ContentType.Json)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
}