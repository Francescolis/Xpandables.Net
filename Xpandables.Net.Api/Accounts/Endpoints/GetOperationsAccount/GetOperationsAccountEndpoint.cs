using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/accounts/operations",
            (
                [AsParameters] GetOperationsAccountRequest request,
                IDispatcher dispatcher,
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
        .WithXExecutionResultMinimalApi()
        .AllowAnonymous()
        .Produces<IAsyncEnumerable<OperationAccount>>();
}