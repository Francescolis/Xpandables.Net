using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/accounts/operations",
            (
                [AsParameters] GetOperationsAccountRequest request,
                IMediator dispatcher,
                CancellationToken cancellationToken) =>
            {
                GetOperationsAccountQuery command = new()
                {
                    KeyId = request.KeyId,
                };

                return dispatcher
                    .SendAsync(command, cancellationToken);
            })
        .WithTags("Accounts")
        .WithName("GetOperationsAccount")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Produces<IAsyncEnumerable<OperationAccount>>();
}