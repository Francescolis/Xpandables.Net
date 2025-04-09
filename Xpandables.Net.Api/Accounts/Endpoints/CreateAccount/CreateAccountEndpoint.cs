
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

public sealed class CreateAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/accounts",
            async (
                [FromBody] CreateAccountRequest request,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                CreateAccountCommand command = new()
                {
                    KeyId = request.KeyId
                };

                return await dispatcher
                .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
            .WithTags("Accounts")
            .WithName("CreateAccount")
            .WithXMinimalApi()
            .AllowAnonymous()
            .Accepts<CreateAccountRequest>(Rest.ContentType.Json)
            .Produces(StatusCodes.Status200OK);
}
