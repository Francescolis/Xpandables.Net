using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

public sealed class GetOperationsAccountQueryHandler(
    IEventStore eventStore) :
    IRequestHandler<GetOperationsAccountQuery>
{
    public async Task<ExecutionResult> HandleAsync(
        GetOperationsAccountQuery query,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        IEventFilter filter = new EntityDomainEventFilter
        {
            Predicate = e => (e.AggregateId == query.KeyId
                              && e.EventName == nameof(DepositMade)) || e.EventName == nameof(WithdrawMade),
            //EventDataPredicate = e => e.RootElement
            //    .GetProperty(nameof(EventEntityDomain.EventName))
            //    .GetString()!
            //    .EndsWith("Made"), // For Postgresql
            OrderBy = e => e.OrderByDescending(o => o.CreatedOn)
        };

        IAsyncEnumerable<IEvent> events = eventStore.FetchAsync(filter, cancellationToken);

        IAsyncEnumerable<OperationAccount> operations = GetOperations(events);

        return ExecutionResults.Ok(operations)
            .WithHeader("Count", $"{filter.TotalCount}")
            .Build();

        static async IAsyncEnumerable<OperationAccount> GetOperations(IAsyncEnumerable<IEvent> events)
        {
            await foreach (IEvent @event in events)
            {
                if (@event is DepositMade deposit)
                {
                    yield return new OperationAccount
                    {
                        Id = deposit.EventId,
                        Date = deposit.OccurredOn.DateTime,
                        Amount = deposit.Amount,
                        Type = "Deposit"
                    };
                }
                else if (@event is WithdrawMade withdraw)
                {
                    yield return new OperationAccount
                    {
                        Id = withdraw.EventId,
                        Date = withdraw.OccurredOn.DateTime,
                        Amount = withdraw.Amount,
                        Type = "Withdraw"
                    };
                }
            }
        }
    }
}