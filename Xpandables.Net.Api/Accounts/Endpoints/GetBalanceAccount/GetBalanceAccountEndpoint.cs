using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

public sealed class GetBalanceAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/accounts/balance",
            async (
                [AsParameters] GetBalanceAccountRequest request,
                IDispatcher dispatcher,
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
        .WithXExecutionResultMinimalApi()
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);
}