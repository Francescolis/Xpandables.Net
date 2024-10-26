using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/accounts/balance", async (
            [AsParameters] GetBalanceAccountRequest request,
            IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            GetBalanceAccountQuery command = new GetBalanceAccountQuery
            {
                KeyId = request.KeyId,
            };

            return await dispatcher
                .SendAsync(command, cancellationToken)
                .ConfigureAwait(false);
        })
        .WithTags("Accounts")
        .WithName("GetBalanceAccount")
        .WithXOperationResultMinimalApi()
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);
    }
}