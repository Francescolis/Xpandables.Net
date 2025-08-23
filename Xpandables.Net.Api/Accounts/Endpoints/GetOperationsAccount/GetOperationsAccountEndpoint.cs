using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions;
using Xpandables.Net.Repositories;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
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
        .Produces<IAsyncPagedEnumerable<OperationAccount>>(contentType: Rests.Rest.ContentType.Json);

        app.MapGet("/accounts/asyncEnum",
            (
                IEventStore eventStore,
                CancellationToken cancellationToken) =>
            {
                ExecutionResult result = ExecutionResult.Success(eventStore
                    .FetchAsync(
                    (IQueryable<EntityDomainEvent> query) => query
                        .OrderBy(o => o.Sequence)
                        .Skip(1)
                        .Take(10)
                        .Select(a => new
                        {
                            a.AggregateId,
                            a.AggregateName,
                            a.Name
                        }), cancellationToken));

                return result;
            })
        .WithTags("Accounts")
        .WithName("GetAsyncEnum")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Produces200OK<IAsyncPagedEnumerable<EntityDomainEvent>>();
    }
}