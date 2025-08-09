using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/accounts/balance",
            async (
                [AsParameters] GetBalanceAccountRequest request,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                GetBalanceAccountQuery command = new()
                {
                    KeyId = request.KeyId,
                };

                return await dispatcher
                    .SendAsync(command, cancellationToken)
                    .ConfigureAwait(false);
            })
        .WithTags("Accounts")
        .WithName("GetBalanceAccount")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);
}