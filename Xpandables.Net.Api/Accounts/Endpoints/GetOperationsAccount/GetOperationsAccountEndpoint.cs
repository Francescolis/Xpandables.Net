using Microsoft.EntityFrameworkCore;

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
                DataContextEvent eventStore,
                CancellationToken cancellationToken) =>
            {
                ExecutionResult result = ExecutionResult.Success(eventStore
                    .Domains
                    .AsNoTracking()
                    .Skip(0)
                    .Take(5)
                    .Select(a => new
                    {
                        a.AggregateId,
                        a.AggregateName,
                        a.Status,
                        a.Name
                    })
                    .AsAsyncPagedEnumerable());

                return result;
            })
        .WithTags("Accounts")
        .WithName("GetAsyncEnum")
        .WithXMinimalApi()
        .AllowAnonymous()
        .Produces200OK<IAsyncPagedEnumerable<EntityDomainEvent>>();
    }
}